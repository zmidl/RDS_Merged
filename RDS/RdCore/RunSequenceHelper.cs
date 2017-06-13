using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using RdCore.Enum;
using Sias.Core;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RdCore
{
    #region public partial class RunSequenceForm : Form

    /// <summary>
    /// RunSequenceWindow.xaml 的交互逻辑
    /// </summary>
    public class RunSequenceHelper
    {
        #region // \page   run_secuence_form   Run Sequence form

        /******************************************************************************/
        /*! \page   run_sequence_form   Run Sequence form
         * 
         * The \ref page_example contains a special dialog that can be showed while
         * a robot sequence is executing. The form can be used in different ways:
         *  - prepare a sequence method and pass this methods directly to the run 
         *    sequence form for execution. 
         *  - prepare a sequence ans set the sequecne as work method on the 
         *    run sequence form. To start the sequence the user can use the "run" 
         *    button on the run sequence form.
         *  .
         * 
         * \image html "RunSequenceForm.png"
         * 
         * The run sequecne form is thought as an example on 
         *  - how to start a sequence as a background thread
         *  - how to update dialog states from bckground thread
         *  - how to abort a running sequence.
         *  - how to pause and continue a running sequence.
         *  - how to use a log handler to get log messags.
         *  .
         * 
         * \section     create_run_form     Creating a run sequence form
         * To generate the run sequence form a valid \ref process_workbench is required.
         * Optionally an execution method may get passed.
         * \code 
         *   _RunSequence = new RunSequenceForm(Workbench, null);
         * \endcode
         * It is recommended to use only \b one run sequence form. This will take care 
         * that we have always only one thread accessing the robot. In case you want 
         * to have a second thread for the robot (multi robot threads) it is not 
         * recommended to have this attached to a second run sequence form as those 
         * would not be independend in terms of pause/continue and log handling.
         * 
         * \section     change_sequence         Change execution method
         * To change the execution method the RunSequenceForm.ChangeWorkerThread may 
         * get used. To start a new sequecne either the "run" button may be pressed 
         * or the RunSequenceForm.Start() method may be called.
         * 
         * For direct excution of an execution method RunSequenceForm.ExecuteSequence 
         * may be used. When starting a sequence while the run sequence dialog is not
         * visible the dialog will hide automatically after execution is finished.
         * 
         * \section     abort_sequence          Abort execution sequence
         * To abort a process simlpy the robot error requires to be set:
         * \code 
         *   Sias.CanDev.SCanIO.RobotError = 0xFF00; // set an error to abort
         * \endcode
         * In general the error code can be anything but is not allowed to be 0.
         * 
         * \section     pause_sequence          Pause / continue execution sequence
         * For pausing / continue the Sias.CanDev.SCanIO.OnModuleExecution event 
         * is used. This event is called before each module or module group execution.
         * as long as the method did not return command will not get executed and 
         * so the robot is not able to start any new command. 
         *      \b see RunSequenceForm.OnModuleExecution
         * 
         * \section     log_handling            Log message handling
         * For log handling the log handler requires to get registered at the 
         * \ref sect_logmanager. The log handler gets activated by
         *  \code SLogManager.AddHighLevelLogHandler(RunLogHandler); \endcode
         * in the Start() method and gets deactivated in OnEndThread() by
         *  \code SLogManager.RemoveHighLevelLogHandler(RunLogHandler); \endcode
         * \b Important: The log handler may be called from backgroudn thread so
         *               it is importnat to take care to use invoke in case of 
         *               accessing dialog components.
         * 
         * \section      sequence_examples       Run sequence examples
         * For examples on how to the run sequence form please refer to 
         * \ref CMainForm.InitButton_Click or \ref CMainForm.MoveItem.
         */
        /******************************************************************************/

        #endregion

        #region // internal fields

        private ProcessWorkbench WorkBench;
        public ExecutionMethod WorkMethod = null;
        private Thread WorkerThread = null;
        private bool ThreadActive;
        public bool SequenceActive;
        private bool ThreadIdle;
        private bool MultiStart;
        private EventWaitHandle StartSequence;

        #endregion

        #region public bool IsPaused // Paused flag

        /// <summary>Paused flag field</summary>
        private bool _IsPaused;

        /// <summary>Paused flag property</summary>
        [Browsable(false)]
        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                _IsPaused = value;
                //Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //    ButtonPauseResume.Content = (_IsPaused) ? "Continue" : "Pause";
                //    //20161025                    Refresh();
                //});
            }
        }

        #endregion

        #region public bool ChangeWorkerThread(ExecutionMethod NewWorkerMethod)

        public bool IsEnabledRun = false;
        public bool IsEnabledPause = false;
        public bool IsEnabledExit = false;
        
        /// <summary>
        /// Change worker method
        /// 
        /// This method is used to change the worker method. It is not allowed 
        /// to change the worker method while an execution thread is executed.
        /// </summary>
        /// <param name="NewWorkerMethod">New worker method</param>
        /// <returns>true if a valid worker method is set successfully, otherwise false</returns>
        public bool ChangeWorkerThread(ExecutionMethod NewWorkerMethod)
        {
            if (SequenceActive)
            {
                MessageBox.Show("Another active sequence, has to be " +
                                "stopped before preparing a new task!");
                return false;
            }
            else
            {
                WorkMethod = NewWorkerMethod;
                IsEnabledRun = (WorkMethod is ExecutionMethod);
                // 
                MultiStart = IsEnabledRun; // allow multi start if valid 
                //
                return IsEnabledRun;
            }
        }

        #endregion

        #region public RunSequenceWindow(ProcessWorkbench workBench, ExecutionMethod workMethod)

        /// <summary>
        /// Initializes a new instance of the <b>RunSequenceForm</b> class.
        /// </summary>
        /// <param name="workBench">The process workbench used for execution</param>
        /// <param name="workMethod">The method containing the execution sequence</param>
        public RunSequenceHelper(ProcessWorkbench workBench, ExecutionMethod workMethod)
        {
            //
            WorkMethod = workMethod;
            WorkBench = workBench;
            //
            IsEnabledRun = (WorkMethod is ExecutionMethod);
            IsEnabledPause = false;
            IsEnabledExit = true;
            //
            MultiStart = IsEnabledRun; // allow multi start if valid 
            //
            ThreadActive = false; // thread not running 
            ThreadIdle = true; // idle
            SequenceActive = false; // no sequence active
        }

        #endregion

        #region void OnApplicationExit(object sender, EventArgs e)

        /// <summary>
        /// Application exit method
        /// 
        /// This method is called in case of exiting the application.
        /// It is responsible for shutting down the run sequence worker 
        /// thread. In case the thread would not be shut down the would
        /// not be able to close and exit.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event arguments</param>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (ThreadActive)
            {
                // exit on next loop
                ThreadActive = false;
                // wait until thread gets 
                Trace.WriteLine("Wait for end of sequence ...", SLogManager.CategoryDebug);
                while (!ThreadIdle) Thread.Sleep(100);
                // start sequence to finish the thread
                if (StartSequence is EventWaitHandle)
                {
                    StartSequence.Set();
                }
            }
        }

        #endregion

        #region void RunLogHandler(string message, string categories, DateTime time)

        /// <summary>
        /// Log handler
        /// 
        /// This log handler is used to receive the high level log messages and
        /// appends those to the list box on the run sequence dialog.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="categories"></param>
        /// <param name="time"></param>
        private void RunLogHandler(string message, string categories, DateTime time)
        {
            //try
            //{
            //    Dispatcher.Invoke((MethodInvoker)delegate
            //    {
            //        ListBoxMessage.Items.Add(time.ToString("T") + " " + message);
            //    });
            //}
            //catch (Exception)
            //{
            //    // ignore exception in log handler 
            //}
        }

        #endregion

        #region void OnModuleExecution(short ModulAdr, byte Group)

        /// <summary>
        /// On module execution event
        /// 
        /// This event handler is called on each module execution. It may be 
        /// used pause the run sequence. Pausing means not more module can get 
        /// executed.\n
        /// While sequence is paused typically commands are prepared on some
        /// CAn Modules.
        /// </summary>
        /// <param name="ModulAdr">Module address (0 for a group)</param>
        /// <param name="Group">Execution group</param>
        private void OnModuleExecution(short ModulAdr, byte Group)
        {
            while (_IsPaused)
            {
                if (Sias.CanDev.SCanIO.RobotError != 0)
                {
                    // process interrupted ==> reset pause
                    IsPaused = false;
                }
                else
                {
                    // system is paused and sequence is not interrupted 
                    Thread.Sleep(1); // release CPU 
                }
            }
            return;

            #region unused

            //int count = 0;
            //String Info = "";
            //#region // build command list
            //if (ModulAdr > 0) {
            //    // Single Module execution
            //    SObjectCollection devList = SCanDevice.DevicesAtAddress(ModulAdr);
            //    if (devList.Count > 0) {
            //        SCanDevice dev = devList[0] as SCanDevice;
            //        if (dev is SMotorDevice) {
            //            Info = " - " + dev.DeviceName + ": " + dev.Action_Msg.ToString() + "\n";
            //            count++;
            //        }
            //    }
            //} else {
            //    for (short adr = 0x11; adr < 255; adr++) {
            //        SObjectCollection devList = SCanDevice.DevicesAtAddress(adr);
            //        if (devList.Count > 0) {
            //            SCanDevice dev = devList[0] as SCanDevice;
            //            if ((dev is SMotorDevice) && (dev.DeviceState.Prepared) && ((dev.ExecutionGroup & Group) != 0)) {
            //                Info = Info + " - " + dev.DeviceName + ": " + dev.Action_Msg.ToString() + "\n";
            //                count++;
            //            }
            //        }
            //    }
            //}
            //#endregion
            //if (count > 0) {
            //    // only show trace dialog if there is at least one motor
            //    DialogResult ret = MessageBox.Show(Info + "\n" +
            //                        Translator.Translate("Execute ? (No to abort sequence, Cancel to stop trace)"),
            //                        Translator.Translate("Trace"), MessageBoxButtons.YesNoCancel);
            //    // 
            //    switch (ret) {
            //        case DialogResult.Yes:
            //            break;  // ok ==> go on
            //        case DialogResult.No:
            //            SCanIO.RobotError = 0xFF00;
            //            #region // remove and clear prepared actions
            //            if (ModulAdr > 0) {
            //                // Single Module execution
            //                SObjectCollection devList = SCanDevice.DevicesAtAddress(ModulAdr);
            //                if (devList.Count > 0) {
            //                    SCanDevice dev = devList[0] as SCanDevice;
            //                    if (dev is SCanDevice) {
            //                        dev.RemoveAction();
            //                        if (dev is SMotorDevice) {
            //                            // clear command by preparing a relativ move by 0
            //                            dev.SendCANMsg(SCanMsg.CanMsg_Move((byte)eCOMMAND_MODE_FLAGS.MM_RELATIVE, 0, 0, 0));
            //                            // pCanIO->SendCANMsg(_Address, Msg_Execute(pAInfo->ExecGrp));
            //                        }
            //                    }
            //                }
            //            } else {
            //                for (short adr = 0x11; adr < 255; adr++) {
            //                    SObjectCollection devList = SCanDevice.DevicesAtAddress(adr);
            //                    if (devList.Count > 0) {
            //                        SCanDevice dev = devList[0] as SCanDevice;
            //                        if ((dev is SCanDevice) && (dev.DeviceState.Prepared) && ((dev.ExecutionGroup & Group) != 0)) {
            //                            dev.RemoveAction();
            //                            if (dev is SMotorDevice) {
            //                                // clear command by preparing a relativ move by 0
            //                                dev.SendCANMsg(SCanMsg.CanMsg_Move((byte)eCOMMAND_MODE_FLAGS.MM_RELATIVE, 0, 0, 0));
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            #endregion
            //            break;
            //        case DialogResult.Cancel:
            //            SCanIO.OnModuleExecution -= OnModuleExecution;
            //            chkBoxTraceMode.Checked = false;
            //            break;
            //    }
            //}

            #endregion
        }

        #endregion

        #region private void OnEndThread()

        /// <summary>
        /// On thread end method
        /// 
        /// This method is called from background thread if a sequence execution 
        /// has been finished. As this is called from another thread context it 
        /// requires invoke to access the dialog members.
        /// </summary>
        private void OnEndSequence()
        {
            try
            {
                //Dispatcher.Invoke((MethodInvoker)delegate
                //{
                // invalidate worker method if no multi start
                if (!MultiStart) WorkMethod = null;
                // 
                //ButtonRunStop.Content = "Run";
                IsEnabledRun = (WorkMethod is ExecutionMethod);
                IsEnabledPause = false;
                IsEnabledExit = true;
                // hide dialog on auto exit
                //if (CheckBoxAutoClose.IsChecked.Value) this.Visibility = Visibility.Hidden;
                //
                // });
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region private void ThreadMethod()

        /// <summary>
        /// Thread method
        /// 
        /// This method is used as thread method for the sequence execution.
        /// </summary>
        private void ThreadMethod()
        {
            ThreadActive = true;
            StartSequence = new EventWaitHandle(false, EventResetMode.AutoReset);
            //
            while (ThreadActive)
            {
                // 
                SequenceActive = true;
                SLogManager.AddHighLevelLogHandler(RunLogHandler);
                Sias.CanDev.SCanIO.OnModuleExecution += OnModuleExecution;
                try
                {
                    if (WorkMethod is ExecutionMethod)
                    {
                        //WorkMethod(WorkBench);
                        string Info;
                        bool DoExecute = true;
                        SThreadLock.doStopThreadLockWait = false; // make sure we are waiting for thread locks
                        SLogManager.Log("Start a new workthread", SLogManager.CategoryDebug);
                        if (WorkBench.Robot.ReleaseThreadLocks(false, out Info))
                        {
                            switch (MessageBox.Show("Locked thread locks on start of Execution:\n" + Info +
                                                    "Try to release the thread locks?",
                                "WARNING", MessageBoxButtons.YesNoCancel))
                            {
                                case System.Windows.Forms.DialogResult.Yes:
                                    if (WorkBench.Robot.ReleaseThreadLocks(true, out Info))
                                    {
                                        switch (MessageBox.Show("Some thread locks are not releasable:\n" + Info +
                                                                "Execute anyway?",
                                            "E R R O R", MessageBoxButtons.YesNo))
                                        {
                                            case System.Windows.Forms.DialogResult.Yes:
                                                break;
                                            case System.Windows.Forms.DialogResult.No:
                                                DoExecute = false; // do not execute
                                                break;
                                        }
                                    }
                                    break;
                                case System.Windows.Forms.DialogResult.No:
                                    break;
                                case System.Windows.Forms.DialogResult.Cancel:
                                    DoExecute = false; // do not execute
                                    break;
                            }
                        }
                        if (DoExecute)
                        {
                            WorkMethod(WorkBench);
                            if (WorkBench.Robot.ReleaseThreadLocks(false, out Info))
                            {
                                switch (MessageBox.Show("Locked thread locks on end of Execution:\n" + Info +
                                                        "Try to release?",
                                    "E R R O R", MessageBoxButtons.YesNo))
                                {
                                    case System.Windows.Forms.DialogResult.Yes:
                                        WorkBench.Robot.ReleaseThreadLocks(true, out Info);
                                        break;
                                    case System.Windows.Forms.DialogResult.No:
                                        break;
                                }
                            }
                        }
                        SLogManager.Log("thread finished", SLogManager.CategoryDebug);
                    }
                    //20161025                    this.Dispatcher.Invoke(new MethodInvoker(this.Refresh));
                }
                catch (Exception e)
                {
                    SLogManager.Log(e.Message + " on Worker thread", SLogManager.CategoryError);
                    Trace.WriteLine(e.StackTrace, SLogManager.CategoryDebug);
                }
                Sias.CanDev.SCanIO.OnModuleExecution -= OnModuleExecution;
                SLogManager.RemoveHighLevelLogHandler(RunLogHandler);
                OnEndSequence(); // update dialog
                SequenceActive = false;
                // wait for next sequence start event
                ThreadIdle = true;
                StartSequence.WaitOne();
                ThreadIdle = false;
            }
        }

        #endregion

        #region public bool Start()

        /// <summary>
        /// Start predefined worker method in a new thread
        /// </summary>
        /// <returns>True if sequence started successsfully, otherwise false.</returns>
        public bool Start()
        {
            // show dialog if not visible
            //if (Visibility != Visibility.Visible)
            //{
            //    CheckBoxAutoClose.IsChecked = true; // auto close if not visible
            //                                        //20170508                Show(); // show dialog
            //}
            // 
            if (!ThreadIdle) Thread.Sleep(10); // short wait in case of active sequence
            if (ThreadIdle)
            {
                if (WorkBench is ProcessWorkbench)
                {
                    // there is a valid work bench
                    if (WorkMethod is ExecutionMethod)
                    {
                        SLogManager.AddHighLevelLogHandler(RunLogHandler);
                        // 
                        // ButtonRunStop.Content = "Abort";
                        IsEnabledRun = true;
                        IsEnabledPause = true;
                        IsEnabledExit = false;
                        // 
                        if (!ThreadActive)
                        {
                            // no thread yet ==> start thread
                            Trace.WriteLine("Start Thread ...", SLogManager.CategoryDebug);
                            WorkerThread = new Thread(new ThreadStart(this.ThreadMethod));
                            WorkerThread.Name = "Worker Thread";
                            WorkerThread.Start();
                            //Application.ApplicationExit += new EventHandler(OnApplicationExit);
                            //App.Current.Exit += OnApplicationExit;
                            //this.Closing += OnApplicationExit;
                        }
                        else
                        {
                            // existing thread in idle mode start sequence
                            StartSequence.Set();
                        }
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Invalid worker method!", "Error");
                    }
                }
                else
                {
                    MessageBox.Show("No valid workbench to start sequence!", "Error");
                }
            }
            else
            {
                MessageBox.Show("Sequence already active!", "Error");
            }
            return false;
        }

        #endregion

        #region public bool ExecuteSequence(ExecutionMethod workMethod)

        /// <summary>
        /// Execute the sequence in given worker method.
        /// 
        /// Important: The worker methods gets automatically invalidated after execution!
        /// </summary>
        /// <param name="workMethod">The method containing the execution sequence</param>
        /// <returns>True if sequence started successsfully, otherwise false.</returns>
        public bool ExecuteSequence(ExecutionMethod workMethod)
        {
            if (!SequenceActive)
            {
                WorkMethod = workMethod;
                //
                MultiStart = false; // no multi start on direct execution
                //
                return Start();
            }
            else
            {
                MessageBox.Show("Another sequence already active!", "Error");
            }
            return false;
        }

        #endregion

        #region public bool ExecuteSequence(ProcessWorkbench workBench, ExecutionMethod workMethod)

        /// <summary>
        /// Execute the sequence in given worker method.
        /// 
        /// Important: The worker methods gets automatically invalidated after execution!
        /// </summary>
        /// <param name="workBench">The process workbench used for execution</param>
        /// <param name="workMethod">The method containing the execution sequence</param>
        /// <returns>True if sequence started successsfully, otherwise false.</returns>
        public bool ExecuteSequence(ProcessWorkbench workBench, ExecutionMethod workMethod)
        {
            if (!SequenceActive)
            {
                WorkMethod = workMethod;
                WorkBench = workBench;
                // 
                MultiStart = false; // no multi start on direct execution
                //
                return Start();
            }
            else
            {
                MessageBox.Show("Another sequence already active!", "Error");
            }
            return false;
        }

        #endregion

        #region private void RunStopButton_Click(object sender, EventArgs e)

        /// <summary>
        /// This method is called when the RunStopButton's Click event has been fired.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that fired the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> of the event.</param>
        private void RunStopButton_Click(object sender, EventArgs e)
        {
            if (SequenceActive)
            {
                // thread already running ==> stop
                Sias.CanDev.SCanIO.RobotError = 0xFF00; // set an error to abort
                // 
                //ButtonRunStop.Content = "Aborting ...";
                IsEnabledRun = false;
                IsEnabledPause = false;
                IsEnabledExit = false;
            }
            else
            {
                // no thread running ==> start thread
                Start();
            }
        }

        #endregion

        #region private void PauseResumeButton_Click(object sender, EventArgs e)

        /// <summary>
        /// This method is called when the PauseResumeButton's Click event has been fired.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that fired the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> of the event.</param>
        private void PauseResumeButton_Click(object sender, EventArgs e)
        {
            // invert paused
            IsPaused = !IsPaused;
        }

        #endregion

        #region private void ExitButton_Click(object sender, EventArgs e)

        ///// <summary>
        ///// This method is called when the ExitButton's Click event has been fired.
        ///// </summary>
        ///// <param name="sender">The <see cref="object"/> that fired the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> of the event.</param>
        //private void ExitButton_Click(object sender, EventArgs e)
        //{
        //    this.Visibility = Visibility.Hidden;
        //}

        #endregion

        #region private void ClearButton_Click(object sender, EventArgs e)

        ///// <summary>
        ///// This method is called when the ClearButton's Click event has been fired.
        ///// </summary>
        ///// <param name="sender">The <see cref="object"/> that fired the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> of the event.</param>
        //private void ClearButton_Click(object sender, EventArgs e)
        //{
        //    ListBoxMessage.Items.Clear();
        //}

        #endregion

    }

    #endregion

    #region public delegate void ExecutionMethod(ProcessWorkbench workbench)

    /// <summary>
    /// Execution method delegate
    /// </summary>
    /// <param name="workbench">Process workbench for execution</param>
    public delegate void ExecutionMethod(ProcessWorkbench workbench);

    #endregion
}
