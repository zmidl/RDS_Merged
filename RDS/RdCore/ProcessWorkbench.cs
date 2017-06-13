using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup.Localizer;
using System.Windows.Threading;
using RdCore.Enum;
using Rendu.RdsModule;
using Rendu.ShareDll;
using Sias.BaseDev;
using Sias.CanDev;
using Sias.Core;
using Sias.Core.Attributes;
using Sias.Core.Interfaces;
using Sias.DetectionManager;
using Sias.DeviceManager;
using Sias.DropStation;
using Sias.Layout;
using Sias.LayoutManager;
using Sias.LiquidManager;
using Sias.PipArm;
using Sias.RobotDev;
using Sias.TipManager;

namespace RdCore
{
    /// <summary>
    /// Process workbench class
    /// 
    /// It is much more nice to have a direct access to all required components 
    /// in a process. So we designed this singleton class as an environment for 
    /// a process.\n
    /// </summary>
    /// <details>
    /// The process workbench class will take care about invalidation and 
    /// refreshing of its references on reloading a robot or layout. 
    /// In addition it implements a background layout loading and layout 
    /// and state management with the ProcessWorkbench.ReportStateChanged 
    /// event.
    /// 
    /// For details please refer \ref process_workbench
    /// </details>
    public class ProcessWorkbench
    {
        #region // \page   process_workbench   Process workbench

        /******************************************************************************/
        /*! \page   process_workbench   Process workbench
         * 
         * The process workbench is a kind of wrapper for several Nelson SDK 
         * components. The Idea is to collect the required member for a process at
         * in a single class. This approach allows taking care about the "old"
         * references on loading/unloading robots or layouts.
         * 
         * Actually the process workbench class is implemented as singleton class. 
         * Most applications will only have only ONE active process. So it is 
         * good to have a single process workbench for the whole application.
         * 
         * The process workbench will contain:
         *  - The \ref sec_robot_management for the process 
         *  - The \ref sec_process_devices and arm methods for the process
         *  - The \ref sec_layout_management for the process 
         *  - The \ref sec_layout_references for the process
         *  - \ref Singleton_manager "Singleton manager" references 
         *     - \ref ProcessWorkbench.DeviceManager
         *     - \ref ProcessWorkbench.TipManager
         *     - \ref ProcessWorkbench.LiquidManager
         *     - \ref ProcessWorkbench.LayoutManager
         *     - \ref ProcessWorkbench.DetectionManager
         *     - \ref ProcessWorkbench.WashManager
         *     .
         *  - A set of volume band and \ref sec_liquid_class_helpers
         *  - Method for \ref sec_disposable_tip_management
         *  - A ProcessWorkbench.StartSequence method for preparing the 
         *    execution of a process sequence.
         *  .
         * 
         * In addition the process workbench includes a startup code making sure 
         * the Nelson assembly resolver gets activated by activating the static 
         * STypeManager class.
         * 
         * For finding the Nelson SDK files the workbench will search in 
         * the following sequence:
         *  - AppDomain.CurrentDomain.BaseDirectory + ProcessWorkbench.NelsonPath
         *  - SDK installation folder in registry (HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Sias\\Nelson)
         *    + ProcessWorkbench.NelsonPath
         *  - "<a>%ProgramFiles%</a>\Nelson" + ProcessWorkbench.NelsonPath
         *  - AppDomain.CurrentDomain.BaseDirectory
         *  - SDK installation folder in registry (HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Sias\\Nelson)
         *  - "<a>%ProgramFiles%</a>\Nelson"
         *  .
         * 
         * It is recommended to load ALL assemblies using the assembly resolver, as this 
         * will also take care about checking assemblies signatures an logging assemblies.
         * 
         */

        #endregion

        #region // workbench startup code and setup properties

        #region // static properties for workbench setup

        #region public String NelsonPath // Default nelson binary path

        /// <summary>Default nelson binary path field</summary>
        private static String _NelsonPath = "bin";

        /// <summary>Default nelson binary path property</summary>
        [Browsable(false)]
        public static String NelsonPath
        {
            get { return _NelsonPath; }
            set
            {
                if (_Workbench is ProcessWorkbench)
                {
                    Trace.WriteLine("Unable to change Nelson Path after workbench is created!",
                        SLogManager.CategoryWarning);
                }
                else
                {
                    _NelsonPath = value;
                }
            }
        }

        #endregion

        #region public static bool ShowErrors // Show startup errors as message boxes

        /// <summary>Show startup errors as message boxes field</summary>
        private static bool _ShowErrors = true;

        /// <summary>Show startup errors as message boxes property</summary>
        [Browsable(false)]
        public static bool ShowErrors
        {
            get { return _ShowErrors; }
            set { _ShowErrors = value; }
        }

        #endregion

        #region public static String ConfigFile // Configuration file name

        /// <summary>Configuration file name field</summary>
        private static String _ConfigFile = "Sias.Config.xml";

        /// <summary>Configuration file name property</summary>
        [Browsable(false)]
        public static String ConfigFile
        {
            get { return _ConfigFile; }
            set
            {
                if (_Workbench is ProcessWorkbench)
                {
                    Trace.WriteLine("Unable to change config file after workbench is created!",
                        SLogManager.CategoryWarning);
                }
                else
                {
                    _ConfigFile = value;
                }
            }
        }

        #endregion

        #region public static bool UseCanInterface // Use CAN interface flag

        /// <summary>Use CAN interface flag field</summary>
        private static bool _UseCanInterface = true;

        /// <summary>Use CAN interface flag property</summary>
        [Browsable(false)]
        public static bool UseCanInterface
        {
            get { return _UseCanInterface; }
            set
            {
                if (_Workbench is ProcessWorkbench)
                {
                    Trace.WriteLine("Unable to change CAN usage after workbench is created!",
                        SLogManager.CategoryWarning);
                }
                else
                {
                    _UseCanInterface = value;
                }
            }
        }

        #endregion

        #region public static bool UseExceptionHandler // Use unhandled exception handler

        /// <summary>Use unhandled exception handler field</summary>
        private static bool _UseExceptionHandler = true;

        /// <summary>Use unhandled exception handler property</summary>
        [Browsable(false)]
        public static bool UseExceptionHandler
        {
            get { return _UseExceptionHandler; }
            set
            {
                if (_Workbench is ProcessWorkbench)
                {
                    Trace.WriteLine("Unable to change exception handling after workbench ist created!",
                        SLogManager.CategoryWarning);
                }
                else
                {
                    _UseExceptionHandler = value;
                }
            }
        }

        #endregion

        #region public static bool ShowExceptionDialogs // Show exception dialog boxes

        /// <summary>Show exception dialog boxes field</summary>
        private static bool _ShowExceptionDialogs = true;

        /// <summary>Show exception dialog boxes property</summary>
        [SStreamable, Browsable(true), DisplayName("ShowExceptionDialogs")]
        [Description("Show exception dialog boxes")]
        public static bool ShowExceptionDialogs
        {
            get { return _ShowExceptionDialogs; }
            set { _ShowExceptionDialogs = value; }
        }

        #endregion

        #region public static int ExceptionCount // No of exceptions handled by exception handlers

        /// <summary>No of exceptions handled by exception handlers field</summary>
        private static int _ExceptionCount = 0;

        /// <summary>No of exceptions handled by exception handlers property</summary>
        [Browsable(false)]
        public static int ExceptionCount
        {
            get { return _ExceptionCount; }
        }

        #endregion

        #endregion

        #region // exception handling and visualization

        #region private static bool ReportException(String Info,Exception MyException)

        /// <summary>
        /// Report an exception by optionally showing an exception dialog
        /// </summary>
        /// <param name="Info">Exception type information</param>
        /// <param name="MyException">The exception to report</param>
        /// <returns>True if aborted otherwise false</returns>
        private static bool ReportException(String Info, Exception MyException)
        {
            _ExceptionCount++; // another unhandled thread exception occurred 
            DialogResult result = DialogResult.Ignore;
            //MessageBoxResult result = MessageBoxResult.None;

            try
            {
                if (String.IsNullOrEmpty(Info)) Info = "Undefined exception";
                String s = Info + " " + MyException.Message + "\n" + MyException.StackTrace;
                String[] lines = s.Split(new char[] {'\r', '\n'});
                foreach (String line in lines)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        Trace.WriteLine(line, Sias.Core.SLogManager.CategoryDebug);
                    }
                }
                if (ShowExceptionDialogs)
                {
                    ThreadExceptionDialog dlg = new ThreadExceptionDialog(MyException);
                    dlg.Text = Info;
                    result = dlg.ShowDialog();
                    //                    result = System.Windows.Forms.MessageBox.Show(MyException.Message, Info, MessageBoxButton.YesNoCancel);
                }
            }
            finally
            {
                if (result == DialogResult.Abort)
                    //if (result == MessageBoxResult.No)
                {
                    // Important: We can't access SCanIO as the SDK may not be loaded on 
                    //            on assigning the exception handler. This would result in 
                    //            an not found assembly.
                    ShutdownCAN(); // make sure to shutdown can connection (if connected)
                    // kill process to avoid additional handling by windows (only if we had a dialog)
                    if (ShowExceptionDialogs) Process.GetCurrentProcess().Kill();
                    // exit application
                    //                    Application.Current.Shutdown();
                }
            }
            return (result == DialogResult.Abort);
            //return (result == MessageBoxResult.No);
        }

        #endregion

        #region private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)

        /// <summary>
        /// This method is called when the Application's ThreadException event has been fired.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that fired the event.</param>
        /// <param name="e">The <see cref="ThreadExceptionEventArgs"/> of the event.</param>
        private static void Application_ThreadException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ReportException("Unhandled thread exception", e.Exception);
        }

        #endregion

        #region private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)

        /// <summary>
        /// This method is called when the CurrentDomain's UnhandledException event has been fired.
        /// </summary>
        /// <param name="sender">The <see cref="object"/> that fired the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> of the event.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportException("Unhandled exception", (Exception) e.ExceptionObject);
        }

        #endregion

        #region public static bool ReportException(Exception MyException)

        /// <summary>
        /// Show handled exception dialog
        /// </summary>
        /// <param name="MyException">The exception to report</param>
        /// <returns></returns>
        public static bool ReportException(Exception MyException)
        {
            return ReportException("Handled exception", MyException);
        }

        #endregion

        #endregion

        #region private static bool IsCanUsable()

        /// <summary>
        /// Check if can interface is usable
        /// 
        /// It is important to have this methods separated as the Can device 
        /// driver gets loaded on calling the method. So loader exception are 
        /// thrown on calling.
        /// </summary>
        /// <returns>True if can is usable, otherwise false.</returns>
        private static bool IsCanUsable()
        {
            return Sias.CanDev.SCanIO.IsUsable;
        }

        #endregion

        #region private static bool CheckAccessToCAN(bool ShowErrors)

        /// <summary>
        /// Check Access to CAN Interface
        /// This method is used to verify is the Nelson SDK CAN interface 
        /// (CAN Device) may be usable with this application. 
        /// </summary>
        /// <returns>true if CAN devices may be usable.</returns>
        private static bool CheckAccessToCAN()
        {
            // Make sure we have a 32 bit process
            if (IntPtr.Size != 4)
            {
                // The Nelson SDK CAN Interface (SCanIO/SCanDevice) is only usable in 32 bit 
                // applications. Please compile your executable with "\x86" platform option.
                if (ShowErrors)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "The Nelson SDK does only support CAN access for " +
                        "32 bit applications. Please check you installation.",
                        "Verification Error");
                    //string message = "The Nelson SDK does only support CAN access for 32 bit applications. Please check you installation.";
                    //ReportMessageEvent(PopupType.ShowMessage, message, null);
                }
                return false;
            }

            // Try loading and accessing Dev_CanDev.dll
            try
            {
                if (!IsCanUsable())
                {
                    //
                    // This may have two different reasons:
                    // - The Can I/O dll may be not found (it is part of the SDK and needs to be 
                    //   placed in the same folder as the Dev_CanDev.dll)
                    // - A Can I/O interface version mismatch may be occurred (incorrect Can I/O).
                    // Please take care to have a used proper version package
                    if (ShowErrors)
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Unable to access CAN interface. " +
                            "Please check your installation.",
                            "Verification Error");
                        //string message = "Unable to access CAN interface. " +"Please check your installation.";
                        //ReportMessageEvent(PopupType.ShowMessage, message, null);
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                // Mostly this exception comes up in case the Can device dll is used by 
                // an incompatible DotNet framework or with incorrect binding options.
                //
                // For example: When using the Nelson SDK in combination with a
                // DotNet Framework 4.0 application it is needed to define 
                //    <startup useLegacyV2RuntimeActivationPolicy="true">
                // in the <configuration> section of the application config file.
                if (ShowErrors)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Unable to load CAN interface assembly.\n" +
                        "Please check your installation.",
                        "Verification Error");
                    //string message = "Unable to load CAN interface assembly.\n" +"Please check your installation.";
                    //ReportMessageEvent(PopupType.ShowMessage, message, null);
                }
            }
            return true;
        }

        #endregion

        #region public static void ShutdownCAN()

        /// <summary>
        /// Shutdown CAN 
        /// 
        /// This method is called in case the application gets stopped 
        /// directly (e.g. caused by an exception). It should try to 
        /// disconnect from CAN in case the CAN Bus is connected.
        /// </summary>
        public static void ShutdownCAN()
        {
            try
            {
                SCanIO.Disconnect();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region private static bool VerifyNelsonSDK(bool ShowErrors, String SetupFile, String BinaryBasePath, bool NeedAccessToCAN)

        /// <summary>
        /// Verify Nelson SDK
        /// This method loads the given setup file and verifies the Nelson SDK for some known 
        /// SDK requirements.
        /// </summary>
        /// <param name="BinaryBasePath">The used binary base path.</param>
        /// <returns>True if Nelson SDK verification successfully, otherwise false</returns>
        private static bool VerifyNelsonSDK(String BinaryBasePath)
        {
            // optionally disable missing assembly dialog 
            Sias.Core.STypeManager.DisableMissingAssemblyDialog = true;

            // check used framework
            if (System.Environment.Version.Major < 2)
            {
                // Nelson SDK requires at least DotNet framework 2.0 
                if (ShowErrors)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Invalid DotNet Framework for Nelson SDK.\n" +
                        "Please check you installation.",
                        "Verification Error");
                    //string message = "Invalid DotNet Framework for Nelson SDK.\n" +"Please check you installation.";
                    //ReportMessageEvent(PopupType.ShowMessage, message, null);
                }
                return false;
            }

            // Set/Change configuration name and load configuration 
            Sias.Core.SConfigurationManager.ConfigurationFileName = ConfigFile;
            Sias.Core.SConfigurationManager.BinaryBasePath = BinaryBasePath;

//            SErrorInfo.ErrorInfoExtender += Sias.ErrorCodeManager.SErrorCodeManager.SErrorInfoDelegate;

            // Optionally Check CAN interface
            if (UseCanInterface)
            {
                if (!CheckAccessToCAN()) return false;
            }

            #region // Trace startup conditions

#pragma warning disable 0618
            // This traces get also written to log file if NeedAccessToCAN is true
            Trace.WriteLine("Startup:", Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Work Path = " + System.IO.Directory.GetCurrentDirectory(),
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - StartPath = " + AppDomain.CurrentDomain.BaseDirectory,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(
                " - Core Path = " +
                System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetAssembly(typeof (Sias.Core.STypeManager)).Location),
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - SetupFile = " + Sias.Core.SConfigurationManager.ConfigurationFileName,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine("SConfigurationManager base paths:", Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Old  Base Path = " + Sias.Core.SConfigurationManager.BaseDirectory,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Bin. Base Path = " + Sias.Core.SConfigurationManager.BinaryBasePath,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Data Base Path = " + Sias.Core.SConfigurationManager.DataBasePath,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine("SConfigurationManager:", Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Driver Path = " + Sias.Core.SConfigurationManager.DeviceDriverPath,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Data   Path = " + Sias.Core.SConfigurationManager.DataPath,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Layout Path = " + Sias.Core.SConfigurationManager.LayoutPath,
                Sias.Core.SLogManager.CategoryDebug);
            Trace.WriteLine(" - Device Path = " + Sias.Core.SConfigurationManager.DevicePath,
                Sias.Core.SLogManager.CategoryDebug);
#pragma warning restore 0618

            #endregion

            return true;
        }

        #endregion

        #region private static bool StartupNelsonSDK(String DefaultPath, bool ShowErrors, String SetupFile, bool NeedAccessToCAN)

        /// <summary>
        /// Startup Nelson SDK
        /// 
        /// This method is used to find and startup the Nelson SDK. 
        /// The path of this dll is also used as base path for finding the Nelson SDK. \n
        /// The Default search priority is
        ///  - Using SDK from default path based on application startup folder
        ///  - Using SDK from SDK installation folder in registry (HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Sias\\Nelson)
        ///  - Using SDK from default SDK installation folder: "<a>%ProgramFiles%</a>\Nelson"
        /// </summary>
        /// <returns>True in case the Nelson SDK is loaded successfully, otherwise false</returns>
        private static bool StartupNelsonSDK()
        {
            String[] SearchFolders = new String[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""), // first try on default path
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Nelson"),
                // gets replaced by registry
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Nelson"),
                // default install
            };
            // replace second entry by optional nelson installation path from registry
            Microsoft.Win32.RegistryKey RegKey =
                Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Sias\\Nelson");
            if (RegKey != null) SearchFolders[1] = RegKey.GetValue("BinaryPath", SearchFolders[1]) as String;

            #region // try finding on default folders + Default path

            foreach (String basePath in SearchFolders)
            {
                //
                String FilePath = System.IO.Path.Combine(System.IO.Path.Combine(basePath, NelsonPath), "Core.dll");
                if (System.IO.File.Exists(FilePath))
                {
                    // Nelson core assembly found ==> Activate Nelson type manager
                    // 
                    //   The Sias.Core.TypeManager includes an assembly resolver. This makes sure 
                    //   the Nelson assemblies can be found and loaded. So we will load the 
                    //   sias.core.dll manually and create a Sias.Core.TypeManager by reflection. 
                    Activator.CreateInstance(
                        System.Reflection.Assembly.LoadFrom(FilePath).GetType("Sias.Core.STypeManager"));

                    // load configuration and verify Environment for Nelson SDK usage
                    return VerifyNelsonSDK(basePath);
                }
            }

            #endregion

            #region // try finding directly on default folders

            foreach (String basePath in SearchFolders)
            {
                //
                String FilePath = System.IO.Path.Combine(basePath, "Core.dll");
                if (System.IO.File.Exists(FilePath))
                {
                    // Nelson core assembly found ==> Activate Nelson type manager
                    // 
                    //   The Sias.Core.TypeManager includes an assembly resolver. This makes sure 
                    //   the Nelson assemblies can be found and loaded. So we will load the 
                    //   sias.core.dll manually and create a Sias.Core.TypeManager by reflection. 
                    Activator.CreateInstance(
                        System.Reflection.Assembly.LoadFrom(FilePath).GetType("Sias.Core.STypeManager"));

                    // load configuration and verify Environment for Nelson SDK usage
                    return VerifyNelsonSDK(basePath);
                }
            }

            #endregion

            // Unable to find nelson SDK files
            if (ShowErrors)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find Nelson SDK! Please check Installation.");
                //string message = "Unable to find Nelson SDK! Please check Installation.";
                //ReportMessageEvent(PopupType.ShowMessage, message, null);
            }
            return false;
        }

        #endregion

        #endregion

        #region public static ProcessWorkbench Workbench // The one and only process workbench

        /// <summary>The one and only process workbench field</summary>
        private static ProcessWorkbench _Workbench;

        /// <summary>The one and only process workbench property</summary>
        [Browsable(false)]
        public static ProcessWorkbench Workbench
        {
            get
            {
                if (!(_Workbench is ProcessWorkbench))
                {
                    // no workbench created yet ==> try to create workbench
                    if (UseExceptionHandler)
                    {
                        // optional set application and domain exception handlers
                        Dispatcher.CurrentDispatcher.UnhandledException +=
                            new DispatcherUnhandledExceptionEventHandler(Application_ThreadException);
                        AppDomain.CurrentDomain.UnhandledException +=
                            new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                    }
                    // startup nelson SDK and create workbench
                    if (StartupNelsonSDK()) _Workbench = new ProcessWorkbench();
                }
                return _Workbench;
            }
        }

        #endregion

        #region private ProcessWorkbench()

        /// <summary>
        /// Initializes a new instance of the <b>ProcessWorkbench</b> class.
        /// </summary>
        private ProcessWorkbench()
        {
            // get default robot names
            RobotName = SConfigurationManager.DefaultRobot;
            //Modified by zj 20161219
            LayoutName = SConfigurationManager.DefaultLayout;
            //LayoutName = "Rendu4082.layout.xml";

            // make sure default data is loaded
            TipManager.Load(); // load default tip configuration 
            LiquidManager.Load(); // load default liquids 
            DetectionManager.Load(); // load default detection modes
            //            WashManager.Load(); // load default wash programs
            // robot and layout get loaded when required first time ...
        }

        #endregion

        #region private void ReportStateChanged(string StateInfo)

        // On this example we are using the robot state as "workbench state"
        /// <summary>workbench state changed handler</summary>
        /// <param name="workbench">The workbench changed its state</param>
        public delegate void WorkbenchStateChangedHandler(ProcessWorkbench workbench);

        /// <summary>On State changed event</summary>
        public static event WorkbenchStateChangedHandler OnStateChanged;

        /// <summary>
        /// Change workbench state
        /// </summary>
        /// <param name="workbench">The concerned workbench</param>
        private static void ReportStateChanged(ProcessWorkbench workbench)
        {
            if (OnStateChanged is WorkbenchStateChangedHandler) OnStateChanged(workbench);
        }

        #endregion

        #region // Manager properties

        #region public SDeviceManager DeviceManager // Device manager

        /// <summary>
        /// Device manager property
        /// 
        /// The device manager is a singleton class managing the devices and robots. There is no 
        /// public constructor. You can get the device manager using SDeviceManager.GetManager().
        /// For more readable code we defined a DeviceManager Property.
        /// </summary>
        [Browsable(false)]
        public SDeviceManager DeviceManager
        {
            [DebuggerStepThroughAttribute] get { return SDeviceManager.GetManager(); }
        }

        #endregion

        #region public STipManager TipManager // Tip manager

        /// <summary>
        /// Tip manager property
        /// 
        /// The tip manager is a singleton class managing the tip types and tip adapters. 
        /// There is no public constructor. You can get the tip manager using STipManager.GetTipManager().
        /// For more readable code we defined a TipManager Property.
        /// </summary>
        [Browsable(false)]
        public STipManager TipManager
        {
            get { return STipManager.GetManager(); }
        }

        #endregion

        #region public SLiquidManager LiquidManager // Liquid manager

        /// <summary>
        /// Liquid manager property
        /// 
        /// The liquid manager is a singleton class managing the liquid parameters. There is no 
        /// public constructor. You can get the liquid manager using SLiquidManager.GetManager().
        /// For more readable code we defined a LiquidManager Property.
        /// </summary>
        [Browsable(false)]
        public SLiquidManager LiquidManager
        {
            get { return SLiquidManager.GetManager(); }
        }

        #endregion

        #region public SLayoutManager LayoutManager // Layout manager

        /// <summary>
        /// Layout manager property
        /// 
        /// The Layout manager is a singleton class managing the Layout parameters. There is no 
        /// public constructor. You can get the Layout manager using SLayoutManager.GetManager().
        /// For more readable code we defined a LayoutManager Property.
        /// </summary>
        [Browsable(false)]
        public SLayoutManager LayoutManager
        {
            get { return SLayoutManager.GetManager(); }
        }

        #endregion

        #region public SDetectionManager DetectionManager // Detection Manager

        /// <summary>
        /// Detection Manager
        /// 
        /// The detection manager is responsible for loading and storing detection parameter
        /// records for the rest of the framework. It also provides an editor dialog
        /// to manipulate the detection parameter records.        
        /// </summary>
        [Browsable(false)]
        public SDetectionManager DetectionManager
        {
            get { return SDetectionManager.GetManager(); }
        }

        #endregion

        #region public SWashManager WashManager // Wash program Manager

        /// <summary>
        /// Wash program Manager property
        /// 
        /// The SWashManager is responsible for loading and storing wash programs and
        /// provide them to the washer device driver.  It also provides an editor dialog
        /// to create and modify those programs.
        /// </summary>
        //[Browsable(false)]
        //public SWashManager WashManager
        //{
        //    get { return SWashManager.GetManager(); }
        //}

        #endregion

        #endregion

        #region // Robot management

        /******************************************************************************/
        /*! \page       process_workbench
         *  \section    sec_robot_management    Robot management
         * 
         * For the robot management the process workbench contains the following 
         * methods and properties:
         *  - The \ref ProcessWorkbench.RobotState property contains the actual
         *    state of the robot. It may be NoRobot, Loaded, Connecting, Connected
         *    ConnectError or Disconnecting (see RobotStateValue). Each time the 
         *    robot state will change the ProcessWorkbench.ReportStateChanged event 
         *    is called.
         *  - The \ref ProcessWorkbench.RobotName property contains the robot name.
         *    On default the default robot name from configuration manager is used.
         *    If a new robot name is assigned a loaded robot (and its included devices)
         *    get invalidated.
         *  - The \ref ProcessWorkbench.Robot property contains the loaded robot device.
         *  - The \ref ProcessWorkbench.RobotConnected property contains the connect 
         *    flag of the loaded robot device. Assigning "true" will make sure the robot
         *    is connected while assigning "false" will disconnect the robot.
         *  .
         ******************************************************************************/

        #region public RobotStateValue RobotState // Workbench robot state

        /// <summary>Robot state values</summary>
        public enum RobotStateValue
        {
            /// <summary>No Robot</summary>
            NoRobot,

            /// <summary>Robot loaded</summary>
            Loaded,

            /// <summary>Connecting ...</summary>
            Connecting,

            /// <summary>Robot connected</summary>
            Connected,

            /// <summary>Connect error</summary>
            ConnectError,

            /// <summary>Disconnecting ...</summary>
            Disconnecting,
        }

        /// <summary>Workbench robot state field</summary>
        private RobotStateValue _RobotState;

        /// <summary>Workbench robot state property</summary>
        [Browsable(false)]
        public RobotStateValue RobotState
        {
            get { return _RobotState; }
            set
            {
                _RobotState = value;
                ReportStateChanged(this);
            }
        }

        /// <summary>Robot state text</summary>
        public String RobotStateText
        {
            get
            {
                if (!(_Robot is SRobotDevice)) return "no Robot";
                String s = "Robot '" + _Robot.DeviceName + "' " + _RobotState.ToString();
                if (_Robot.RobotError != 0)
                {
                    s += ", Error=" + _Robot.RobotError.ToString("X4");
                }
                else
                {
                    s += ", no Error";
                }
                return s;
            }
        }

        #endregion

        #region public String RobotName // Robot name

        /// <summary>Robot name field</summary>
        private String _RobotName;

        /// <summary>
        /// Robot name property
        /// 
        /// </summary>
        [SStreamable, Browsable(true), DisplayName("RobotName")]
        [Description("Robot name")]
        public String RobotName
        {
            get
            {
                if (String.IsNullOrEmpty(_RobotName)) return SConfigurationManager.DefaultRobot;
                return _RobotName;
            }
            set
            {
                _RobotName = value; // Set new robot name
                Robot = null; // robot not loaded
            }
        }

        #endregion

        #region public SRobotDevice Robot // The robot used for process

        /// <summary>Process robot field</summary>
        private SRobotDevice _Robot;

        /// <summary>
        /// The robot used on this process
        /// 
        /// This property reflects the robot used for the process. 
        /// It accesses directly the device managers current robot property. \n
        /// 
        /// <b>Important</b>: When assigning <b>null</b> to this property the robot
        /// gets unloaded. The property will take care to disconnect the robot before 
        /// unloading.
        /// </summary>
        public SRobotDevice Robot
        {
            get
            {
                if (!(_Robot is SRobotDevice))
                {
                    // try loading default robot by name
                    Robot = DeviceManager.LoadRobot(RobotName) as SRobotDevice;
                }
                return _Robot; // no robot 
            }
            set
            {
                if (!(value is SRobotDevice) && (_Robot is SRobotDevice))
                {
                    // There is a loaded robot which should be unloaded
                    // Make sure it is disconnected before unloading
                    if (_Robot.IsConnected)
                    {
                        RobotState = RobotStateValue.Disconnecting;
                        _Robot.Disconnect();
                    }
                    // optionally un attach layout
                    ReleaseLayout(); // release layout (undo attachment)
                    // 
                }
                // assign new robot
                _Robot = value;
                // make sure to clean all device references
                CleanDevices();
                // 
                if (_Robot is SRobotDevice)
                {
                    RobotState = RobotStateValue.Loaded;
                }
                else
                {
                    RobotState = RobotStateValue.NoRobot;
                }
            }
        }

        #endregion

        #region public bool RobotConnected

        /// <summary>
        /// Robot connected property
        /// 
        /// This property reflects the connection state of the loaded robot (\ref Robot).\n
        /// - Assigning <b>true</b> to the property will connect MyRobot if not already 
        ///   connected. \n 
        /// - Assigning <b>false</b> will disconnect MyRobot if connected.
        /// </summary>
        /// <value>
        /// <b>true</b>:  The robot is/gets connected \n
        /// <b>false</b>: The robot is/gets disconnected
        /// </value>
        public bool RobotConnected
        {
            get
            {
                bool connected = false;
                // get robot from device manager
                if (Robot is SRobotDevice)
                {
                    // valid robot found
                    AttachLayout(); // make sure layout is attached
                    // report if connected
                    connected = Robot.IsConnected;
                }
                return connected;
            }
            set
            {
                if (value)
                {
                    #region // Connect to robot

                    // check if robot is already connected
                    if (Robot is SRobotDevice)
                    {
                        if (!Robot.IsConnected)
                        {
                            // connect to the robot
                            RobotState = RobotStateValue.Connecting;

                            if (Robot.StartConnect("RD_Robot"))
                            {
                                // CAN Module requests started, init other managers and load 
                                // configuration data while requests are active.
                                // - Load default liquids 
                                if (LiquidManager.LiquidClasses != null) LiquidManager.Load();
                                // - make sure layout is loaded and attached
                                if (Layout is ILayout) AttachLayout();
                                // Wait until connection is established
                                if (Robot.WaitUntilConnected())
                                {
                                    RobotState = RobotStateValue.Connected;
                                }
                                else
                                {
                                    RobotState = RobotStateValue.ConnectError;
                                }
                            }
                            else
                            {
                                RobotState = RobotStateValue.ConnectError;
                            }
                        }
                    }
                    else
                    {
                        RobotState = RobotStateValue.NoRobot;
                    }

                    #endregion

                    shakerSlot1 = SLayoutManager.CurrentLayout.GetSlotByName("RD_ShakerRack1:RD_ShakerRack_CupSlot1");
                    shakerSlot2 = SLayoutManager.CurrentLayout.GetSlotByName("RD_ShakerRack1:RD_ShakerRack_CupSlot2");
                    shakerSlot3 = SLayoutManager.CurrentLayout.GetSlotByName("RD_ShakerRack1:RD_ShakerRack_CupSlot3");
                    heatingSlot1 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1");
                    heatingSlot2 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot2");
                    heatingSlot3 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot3");
                    magSlot1 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot1");
                    magSlot2 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot2");
                    magSlot3 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot3");
                    magSlot4 = SLayoutManager.CurrentLayout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot4");
                }
                else
                {
                    #region // Disconnect from robot

                    if (Robot is SRobotDevice)
                    {
                        if (Robot.IsConnected)
                        {
                            RobotState = RobotStateValue.Disconnecting;
                            Robot.Disconnect(); // disconnect
                        }
                        // disconnected, but remain loaded
                        RobotState = RobotStateValue.Loaded;
                    }

                    #endregion
                }
            }
        }

        #endregion

        #endregion

        #region // Process Devices

        /******************************************************************************/
        /*! \page           process_workbench
         *  \subsection     sec_process_devices     Process devices
         * 
         * The process workbench contains properties for the different process devices.
         * In general this devices are retrieved from loaded robot, but it is easier 
         * and faster to keep a reference so it is not required to retrieve it each 
         * time.\n
         * Unfortunately this remains in outdated references on reloading the robot. 
         * To avoid this it is important to invalidate the outdated references on 
         * unloding/reloading the robot. This is done in the private method 
         * ProcessWorkbench.CleanDevices.\n
         * 
         * Actually the following robot references are available:
         *  - ProcessWorkbench.PipMethods - Pipetting methods property\n
         *    \copydoc ProcessWorkbench::PipMethods
         *    - ProcessWorkbench.DTAdapter - Disposable tip adapter tip map
         *      \copydoc ProcessWorkbench::DTAdapter
         *    - ProcessWorkbench.WashableTips - Tip map with washable tips
         *      \copydoc ProcessWorkbench::WashableTips
         *  - ProcessWorkbench.HandlingMethods - Handling methods property\n
         *    \copydoc ProcessWorkbench::HandlingMethods
         *  - ProcessWorkbench.DropStation - Drop station device property\n
         *    \copydoc ProcessWorkbench::DropStation
         *  .
         * Important: In case of adding other device references take care those get 
         *            invalidated in the CleanDevices method.\n
         ******************************************************************************/

        #region public SPipettingMethods PipMethods // Pipetting methods

        /// <summary>Pipetting methods field</summary>
        private SPipettingMethods _PipMethods;

        /// <summary>
        /// Pipetting methods property
        /// 
        /// This property is used for a more easy Pipetting access. It provides
        /// a valid Pipetting method object if available. You can change this method
        /// that it can handle also robots having more than one arm with Pipetting 
        /// support.
        /// </summary>
        [Browsable(false)]
        public SPipettingMethods PipMethods
        {
            get
            {
                if (_PipMethods == null)
                {
                    SBaseArmMethodsCollection mc = Robot.GetArmMethods(typeof (SPipettingMethods));
                    if (mc.Count >= 1)
                    {
                        // more then one arm supports Pipetting ==> use the first one
                        // it would be possible to select which arm should be used...
                        _PipMethods = mc[0] as SPipettingMethods;
                    }
                    else if (mc.Count > 1)
                    {
                        _PipMethods = mc[0] as SPipettingMethods;
                    }
                }
                return _PipMethods;
            }
        }

        #endregion

        #region public STipMap DTAdapter    // Disposable tip adapters

        /// <summary>Disposable tip adapters field</summary>
        private STipMap _DTAdapter;

        /// <summary>
        /// Disposable tip adapters property
        /// 
        /// This tip map defines which tip adapters are disposable tip adapters.
        /// It is calculated only once and then stored as it will not change until 
        /// loading another robot.
        /// </summary>
        [Browsable(false), DisplayName("DTAdapter")]
        [Description("Disposable tip adapters")]
        public STipMap DTAdapter
        {
            get
            {
                if (!(_DTAdapter is STipMap))
                {
                    _DTAdapter = new STipMap();
                    if (PipMethods is SPipettingMethods)
                    {
                        int tip = 0;
                        foreach (SYZDevice yz in PipMethods.Arm.YZDevices)
                        {
                            if (yz is SPipYZDevice)
                            {
                                if ((yz as SPipYZDevice).TipAdapter is ITipAdapter)
                                {
                                    // it is a removable tip
                                    if ((yz as SPipYZDevice).TipAdapter.Tip is ITipType)
                                    {
                                        _DTAdapter[tip] = (yz as SPipYZDevice).TipAdapter.Tip.IsRemovable();
                                    }
                                    else
                                    {
                                        _DTAdapter[tip] = true; // no tip at adapter ==> it seems to be a dt adapter
                                    }
                                }
                            }
                            tip++; // next y/z device = next tip
                        }
                    }
                }
                return _DTAdapter;
            }
        }

        #endregion

        #region public STipMap WashableTips // Tip map with washable tips

        /// <summary>Tip map with washable tips field</summary>
        private STipMap _WashableTips;

        /// <summary>
        /// Tip map with washable tips property
        /// 
        /// This tip map defines the washable tips of the loaded robot. It is 
        /// calculated only once and then stored as it will not change until 
        /// loading another robot.
        /// </summary>
        [Browsable(false), DisplayName("WashableTips")]
        [Description("Tip map with washable tips")]
        public STipMap WashableTips
        {
            get
            {
                if (!(_WashableTips is STipMap))
                {
                    _WashableTips = new STipMap();
                    if (PipMethods is SPipettingMethods)
                    {
                        int tip = 0;
                        foreach (SYZDevice yz in PipMethods.Arm.YZDevices)
                        {
                            if (yz is SPipYZDevice)
                            {
                                if ((yz as SPipYZDevice).TipAdapter is ITipAdapter)
                                {
                                    // it is a removable tip
                                    if ((yz as SPipYZDevice).TipAdapter.Tip is ITipType)
                                    {
                                        _WashableTips[tip] = (yz as SPipYZDevice).TipAdapter.Tip.IsWashable();
                                    }
                                    else
                                    {
                                        _WashableTips[tip] = false; // no tip at adapter ==> it seems to be a dt adapter
                                    }
                                }
                            }
                            tip++; // next y/z device = next tip
                        }
                    }
                }
                return _WashableTips;
            }
        }

        #endregion

        #region public SHandlingMethods HandlingMethods // Handling Methods

        ///// <summary>Handling Methods field</summary>
        //private SHandlingMethods _HandlingMethods;

        ///// <summary>
        ///// Handling Methods property
        ///// 
        ///// This property is used for a more easy handling method access. It provides
        ///// a valid handling method object if available. You can change this method
        ///// that it can handle also robots having more than one arm with handling
        ///// support.
        ///// </summary>
        //[Browsable(false)]
        //public SHandlingMethods HandlingMethods
        //{
        //    get
        //    {
        //        if (_HandlingMethods == null)
        //        {
        //            SBaseArmMethodsCollection mc = Robot.GetArmMethods(typeof (SHandlingMethods));
        //            if (mc.Count >= 1)
        //            {
        //                // more then one arm supports handling ==> use the first one
        //                // it would be possible to select which arm should be used...
        //                _HandlingMethods = mc[0] as SHandlingMethods;
        //            }
        //            else if (mc.Count > 1)
        //            {
        //                _HandlingMethods = mc[0] as SHandlingMethods;
        //            }
        //        }
        //        return _HandlingMethods;
        //    }
        //}

        #endregion

        #region public SDropStation DropStation // Drop station device

        /// <summary>Drop station device field</summary>
        private SDropStation _DropStation;

        /// <summary>Drop station device property</summary>
        [SStreamable, Browsable(true), DisplayName("DropStation")]
        [Description("Drop station device")]
        public SDropStation DropStation
        {
            get
            {
                if (!(_DropStation is SDropStation))
                {
                    _DropStation = Workbench.Robot.GetDevice(typeof (SDropStation)) as SDropStation;
                }
                return _DropStation;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////
        // Important: In case of adding other device references take care those  //
        //            get cleared when changing or unloading the robot           //
        ///////////////////////////////////////////////////////////////////////////

        #region private void CleanDevices()

        /// <summary>
        /// Clean device references
        /// 
        /// Reset all robot depending references to avoid invalid references
        /// </summary>
        private void CleanDevices()
        {
            // reset all robot depending references to avoid invalid references
            _PipMethods = null; // pipetting methods are invalid
            _DTAdapter = null; // DT adapter tip map is invalid
            _WashableTips = null; // Washable tip map is invalid
            //           _HandlingMethods = null; // handling methods are invalid
            _DropStation = null;
        }

        #endregion

        #endregion

        #region // Layout management

        /******************************************************************************/
        /*! \page       process_workbench
         *  \section    sec_layout_management    Layout management
         * 
         * For the layout management the process workbench contains the following 
         * methods and properties:
         *  - The \ref ProcessWorkbench.LayoutState property contains the actual
         *    state of the robot. It may be NoLayout, Loading, Loaded, LoadError
         *    or Attached (see LayoutStateValue). 
         *    Each time the layout state will change the ProcessWorkbench.ReportStateChanged 
         *    event is called.
         *  - The \ref ProcessWorkbench.LayoutName property contains the layout name.
         *    On default the default layout name from configuration manager ist used.
         *    If a new layout name is assigned a loaded layout gets invalidated and the 
         *    new layout starts loading.
         *  - The \ref ProcessWorkbench.Layout property contains the loaded layout.
         *  - The \ref ProcessWorkbench.RobotConnected property contains the connect 
         *    flag of the loaded robot device. Assigning "true" will make sure the robot
         *    is connected while assigning "false" will disconnect the robot.
         ******************************************************************************/

        #region public LayoutStateValue LayoutState // Workbench robot state

        /// <summary>Robot state values</summary>
        public enum LayoutStateValue
        {
            /// <summary>No Layout</summary>
            NoLayout,

            /// <summary>Layout loading</summary>
            Loading,

            /// <summary>Layout loaded</summary>
            Loaded,

            /// <summary>Layout load error</summary>
            [Description("load error")] LoadError,

            /// <summary>Layout attached</summary>
            Attached,
        }

        /// <summary>Workbench robot state field</summary>
        private LayoutStateValue _LayoutState;

        /// <summary>Workbench robot state property</summary>
        [Browsable(false)]
        public LayoutStateValue LayoutState
        {
            get { return _LayoutState; }
            set
            {
                _LayoutState = value;
                ReportStateChanged(this);
            }
        }

        /// <summary>Layout state text</summary>
        public String LayoutStateText
        {
            get
            {
                if (isLoading)
                {
                    String s = System.IO.Path.GetFileName(bgLayoutName);
                    if (s.EndsWith(".layout.xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        s = s.Substring(0, s.Length - 11);
                    }
                    else if (s.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        s = s.Substring(0, s.Length - 4);
                    }
                    return "loading Layout from '" + s + "'";
                }
                if (!(_Layout is ILayout)) return "no Layout";
                return "Layout '" + _Layout.Name + "' " + _LayoutState.ToString();
            }
        }

        #endregion

        #region // background layout loading

        private String LoadedLayoutName = "";
        private String bgLayoutName = "";
        private bool isLoading = false;

        #region private void bgLayoutLoad()

        /// <summary>
        /// 
        /// </summary>
        private void bgLayoutLoad()
        {
            isLoading = true;
            ILayout bgLayout = null;
            try
            {
                bgLayout = LayoutManager.LoadLayout(bgLayoutName);
                LoadedLayoutName = bgLayoutName;
                isLoading = false;
                Layout = bgLayout;
            }
            catch (Exception e)
            {
                // exception on loading layout 
                Trace.WriteLine("Exception on loading layout: " + e);
                isLoading = false;
                LayoutState = LayoutStateValue.LoadError;
            }
            isLoading = false;
        }

        #endregion

        #region private void WaitUntilLayoutLoaded()

        /// <summary>
        /// Wait until layout is loaded
        /// </summary>
        private void WaitUntilLayoutLoaded()
        {
            while (isLoading) Thread.Sleep(20); // wait until loaded
            if (LayoutState == LayoutStateValue.Loading)
            {
                if (_Layout is ILayout)
                {
                    LayoutState = LayoutStateValue.Loaded;
                }
                else
                {
                    LayoutState = LayoutStateValue.LoadError;
                }
            }
        }

        #endregion

        #region private void StartLoadLayout(String Name)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        private void StartLoadLayout(String Name)
        {
            // make sure last load is finished
            WaitUntilLayoutLoaded();
            // 
            if (LoadedLayoutName != Name)
            {
                // we have to load another layout
                bgLayoutName = Name; // layout to load
                // start bg thread
                Thread MyThread1 = new Thread(new ThreadStart(this.bgLayoutLoad));
                MyThread1.Name = "Layout Load Thread";
                MyThread1.Start();
                // make sure loading is tsrated
                for (int i = 0; i < 100 && !isLoading; i++) Thread.Sleep(10);
                // 
                LayoutState = LayoutStateValue.Loading;
            }
        }

        #endregion

        #region private void LoadLayout(String Name)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        private void LoadLayout(String Name)
        {
            // start layout loading
            StartLoadLayout(Name);
            // Wait until loaded
            WaitUntilLayoutLoaded();
        }

        #endregion

        #endregion

        #region public String LayoutName // Actual layout name

        /// <summary>Actual layout name field</summary>
        private String _LayoutName;

        /// <summary>Actual layout name property</summary>
        [SStreamable, Browsable(true), DisplayName("LayoutName")]
        [Description("Actual layout name")]
        public String LayoutName
        {
            get
            {
                if (String.IsNullOrEmpty(_LayoutName))
                    return SConfigurationManager.DefaultLayout;
                return _LayoutName;
            }
            set
            {
                _LayoutName = value; // set new layout name
                Layout = null; // layout is not loaded yet
                StartLoadLayout(value); // Start layout loading when new name is known
            }
        }

        #endregion

        #region public ILayout Layout // The layout used for process

        /// <summary>Layout field</summary>
        private ILayout _Layout;

        /// <summary>
        /// The layout used for process
        /// 
        /// This property reflects the layout used for the process. 
        /// It accesses directly the layout managers current layout property. \n
        /// </summary>
        [SStreamable]
        public ILayout Layout
        {
            get
            {
                if (!(_Layout is ILayout))
                {
                    // no valid layout loaded yet
                    LoadLayout(LayoutName);
                }
                return _Layout;
            }
            set
            {
                if ((_Layout is ILayout) && (Robot is SRobotDevice))
                {
                    ReleaseLayout();
                }
                // set new layout 
                _Layout = value;
                // clear layout references 
                CleanLayout();
                // update state
                if (_Layout is ILayout)
                {
                    LayoutState = LayoutStateValue.Loaded;
                }
                else
                {
                    LayoutState = LayoutStateValue.NoLayout;
                }
            }
        }

        #endregion

        #region private ReleaseLayout()

        /// <summary>
        /// Release layout (undo attachment)
        /// </summary>
        private void ReleaseLayout()
        {
            if (_Robot is SRobotDevice)
            {
                // 
                if (_Robot.IsLayoutAttached)
                {
                    _Robot.AttachLayout(null);
                    if (_Layout is ILayout)
                    {
                        LayoutState = LayoutStateValue.Loaded;
                    }
                    else
                    {
                        LayoutState = LayoutStateValue.NoLayout;
                    }
                }

            }
        }

        #endregion

        #region private void AttachLayout()

        /// <summary>
        /// 
        /// </summary>
        private void AttachLayout()
        {
            if (_Robot is SRobotDevice)
            {
                if (!ReferenceEquals(_Robot.AttachedLayout, _Layout))
                {
                    // 
                    _Robot.AttachLayout(Layout);
                    //
                    if (_Layout is ILayout)
                    {
                        LayoutState = LayoutStateValue.Attached;
                    }
                    else
                    {
                        LayoutState = LayoutStateValue.NoLayout;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region // Process layout references

        /******************************************************************************/
        /*! \page           process_workbench
         *  \subsection     sec_layout_references   Process layout reference
         * 
         * The process workbench contains properties for the different layout references.
         * In general this layout references are retrieved from loaded layout, but it is 
         * easier and faster to keep a reference so it is not required to retrieve it each 
         * time.\n
         * Unfortunately this remains in outdated references on reloading the layout. 
         * To avoid this it is important to invalidate the outdated refereences on 
         * unloding/reloading the layout. This is done in the private method 
         * ProcessWorkbench.CleanLayout.\n
         * 
         * Actually the following robot references are available:
         *  - ProcessWorkbench.TipRacks - list of tip racks property\n
         *    \copydoc ProcessWorkbench::TipRacks
         *  .
         * Important: In case of adding other layout references take care those get 
         *            invalidated in the CleanLayout method.\n
         ******************************************************************************/

        #region public List<IItem> TipRacks // List of all tip racks in layout

        /// <summary>List of all tip racks in layout field</summary>
        private List<IItem> _TipRacks;

        /// <summary>List of all tip racks in layout property</summary>
        [Browsable(false)]
        public List<IItem> TipRacks
        {
            get
            {
                if (!(_TipRacks is List<IItem>))
                {
                    _TipRacks = new List<IItem>();
                    STipPositionCollection tpc = Layout.GetTipPositions();
                    foreach (ITipPosition tp in tpc)
                    {
                        if (!_TipRacks.Contains(tp.TipRack))
                        {
                            // another tip rack found
                            _TipRacks.Add(tp.TipRack);
                        }
                    }
                }
                return _TipRacks;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////////
        // Important: In case of adding other layout references take care those  //
        //            get cleared when changing or unloading the layout          //
        ///////////////////////////////////////////////////////////////////////////

        #region private void CleanLayout()

        /// <summary>
        /// Clean layout references
        /// 
        /// Reset all layout depending references to avoid invalid references
        /// </summary>
        private void CleanLayout()
        {
            // reset all layout depending references to avoid invalid references
            _TipRacks = null; // invalidate list of tip racks
        }

        #endregion

        #endregion

        #region // liquid parameters & Volume bands

        /**********************************************************Button********************/
        /*! \page           process_workbench
         *  \section        sec_liquid_class_helpers    Liquid class helper
         * 
         * On most processes it is not required to have individual liquid parameters
         * for each tip. So using the liquid manager may hide the important stuff of 
         * a sequence and making the code less readable.\n
         * Thats why we prepared a set of "easy to use" methods transfering the liquid
         * class parameters to the piptting parameter classes.
         *  - ProcessWorkbench.GetVolumeBand - get volume band 
         *    \copydoc ProcessWorkbench::GetVolumeBand
         *  - ProcessWorkbench.GetAirPar - get air gap parameter
         *    \copydoc ProcessWorkbench::GetAirPar 
         *  - ProcessWorkbench.GetAspVolumePar - get aspirate pipetting parameter
         *    \copydoc ProcessWorkbench::GetAspVolumePar 
         *  - ProcessWorkbench.GetDispVolumePar - get dispense pipetting parameter
         *    \copydoc ProcessWorkbench::GetDispVolumePar 
         *  .
         * For an example how this is used please refer 
         *      \ref CMainForm.ButtonTransfer_Click .
         ******************************************************************************/

        #region public SVolumeBand GetVolumeBand(string className, string categoryName, double vol)

        /// <summary>
        /// Get the volume band for the defined liquid, category and volume
        /// 
        /// The Method takes also care about the pipetting arms tip type. \n
        /// Important: We assume all tip adapters are identical for this example.
        /// If this is not the case you may have individual volume bands for each 
        /// tip of the pipetting arm!
        /// </summary>
        /// <param name="className">Name of the liquid class</param>
        /// <param name="categoryName">Name of the category</param>
        /// <param name="vol">The volume to aspirate/dispense</param>
        /// <returns>The specified volume band</returns>
        public SVolumeBand GetVolumeBand(string className, string categoryName, double vol, double tipV = 1000)
        {

            string tipName = "";
            if ((TipManager.TipTypeList != null) && (TipManager.TipTypeList.Count > 0))
            {
                // default: first tip type of tip manager
                tipName = TipManager.TipTypeList[0].Name;
            }
            if ((PipMethods == null) || (PipMethods.Arm == null))
            {
                // no pipetting support ==> no known tip type
                //! TODO: handle "no pipetting support" error
            }
            else
            {
                SPipYZDevice YZDev = PipMethods.Arm.YZDevices[0] as SPipYZDevice;
                if (YZDev == null)
                {
                    // no valid pipetting Y/Z device found ==> no known tip type
                    //! TODO: "handle no valid pipetting Y/Z device found" error
                }
                else if (YZDev.TipAdapter == null)
                {
                    // no tip adapter ==> no known tip type
                    //! TODO: handle "no tip adapter" error
                }
                else if (YZDev.TipAdapter.Tip == null)
                {
                    if (tipV.Equals(300.0))
                    {
                        tipName = "Rainin Universal 300";
                    }
                    else
                    {
                        tipName = "Rainin Universal 1000";
                    }
                    //// no tip at adapter ==> get first accepted tip type 
                    //ITipType[] ttypes = YZDev.TipAdapter.GetAcceptedTipTypes();
                    ////! TODO: optionally search for optimal tip type
                    //if (ttypes.Length > 0) tipName = ttypes[0].Name;
                }
                else
                {
                    // we have a tip ==> use this
                    tipName = YZDev.TipAdapter.Tip.Name;
                }
            }
            SVolumeBand vb = LiquidManager.GetVolumeBand(className, categoryName, tipName, vol);
            return vb;
        }

        #endregion

        #region public SPipettingPar GetAirPar(SAirgap AirPar)

        /// <summary>
        /// Create pipetting parameters for an air gap parameter class
        /// 
        /// Create a new pipetting parameter instance of the parameters defined 
        /// in the given SAirgap class.\n
        /// Important:  This is only usable if all tips are using the \b same
        ///             air gap parameters!
        /// </summary>
        /// <param name="AirPar">The air gap parameter</param>
        /// <returns>The pipetting parameters</returns>
        public SPipettingPar GetAirPar(SAirgap AirPar)
        {
            return new SPipettingPar(PipMethods.Arm.YZDevices.Count, AirPar.Volume,
                AirPar.Speed, AirPar.Ramp, AirPar.Delay);
        }

        #endregion

        #region public SPipettingPar GetAspVolumePar(double Volume, SVolumeBand VolPar)

        /// <summary>
        /// Get default pipetting parameters for aspiration from volume band
        /// 
        /// Create a new pipetting parameter instance of the aspiration parameters 
        /// of the given volume band.\n
        /// Important:  This is only usable if all tips are using the \b same
        ///             volume band parameter!
        /// </summary>
        /// <param name="Volume">volume to aspirate</param>
        /// <param name="VolPar">the used volume band</param>
        /// <returns>The pipetting parameters</returns>
        public SPipettingPar GetAspVolumePar(double Volume, SVolumeBand VolPar)
        {
            return new SPipettingPar(PipMethods.Arm.YZDevices.Count, Volume,
                VolPar.AspirateParameter.Speed,
                VolPar.AspirateParameter.Ramp,
                VolPar.AspirateParameter.StopSpeed, // take care stop speed is not supported by the driver yet
                SPipettingPar.ModeConst.DefaultPosition,
                0, // offset to default position
                VolPar.AspirateParameter.Delay);
        }

        #endregion

        #region public SPipettingPar GetDispVolumePar(double Volume, SVolumeBand VolPar)

        /// <summary>
        /// Get default pipetting parameters for aspiration from volume band
        /// 
        /// Create a new pipetting parameter instance of the dispense parameters 
        /// of the given volume band.\n
        /// Important:  This is only usable if all tips are using the \b same
        ///             volume band parameter!
        /// </summary>
        /// <param name="Volume">volume to aspirate</param>
        /// <param name="VolPar">the used volume band</param>
        /// <returns>The pipetting parameters</returns>
        public SPipettingPar GetDispVolumePar(double Volume, SVolumeBand VolPar)
        {
            return new SPipettingPar(PipMethods.Arm.YZDevices.Count, Volume,
                VolPar.DispenseParameter.Speed,
                VolPar.DispenseParameter.Ramp,
                VolPar.DispenseParameter.StopSpeed, // take care stop speed is not supported by the driver yet
                SPipettingPar.ModeConst.DefaultPosition,
                0, // offset to default position
                VolPar.DispenseParameter.Delay);
        }

        #endregion

        #endregion

        #region // tip rack management

        /******************************************************************************/
        /*! \page           process_workbench
         *  \section        sec_disposable_tip_management    Disposable tip management
         * 
         * On processes using robots with disposable this it is required to update 
         * the states of the tip positions. This requires to be done directly on the 
         * layout components.\n
         * Mostly the tips are managed in tip racks. The process workbench provides 
         * a set of methods for simplified access to the tip position states of the 
         * tip racks of the loaded layout.
         * In addition it contains methods for saving and loading the tip states, so 
         * it is possible to continue with preused racks for another process using 
         * the same layout base.
         * Properties and methods for disposable tip management:
         *  - ProcessWorkbench.TipRacks - a list of all tip racks of the loaded layout
         *    \copydoc ProcessWorkbench::TipRacks
         *  - ProcessWorkbench.SetTipRackState - set state of all tip positions of a tip rack.
         *    \copydoc ProcessWorkbench::SetTipRackState
         *  - ProcessWorkbench.GetNumberOfTips - get number of tips on defined tip rack
         *    \copydoc ProcessWorkbench::GetNumberOfTips
         *  .
         * 
         * Mostly custom application want to continue a new process with the the remaining 
         * tips of the last process. So it may be required to save the states of the tip 
         * positions to a file.\n
         * Therefore the process workbench provides
         *  - ProcessWorkbench.SaveTipStates - save tip states to xml file
         *    \copydoc ProcessWorkbench::SaveTipStates
         *  - ProcessWorkbench.LoadTipStates - load tip states from xml file
         *    \copydoc ProcessWorkbench::LoadTipStates
         *  .
         * 
         ******************************************************************************/

        #region public void SetTipRackState(IItem TipRack, STipPositionState NewState)

        /// <summary>
        /// Set tip rack state
        /// 
        /// Set all tip positions of a tip rack to a new state. Take care, some 
        /// states are managed by layout and can not be set individual.\n
        /// In general only three of the tipposition states are useful:
        ///  - Empty - to set a tip position to empty (ther is not tip)
        ///  - Pickable - to set a tip position to have a tip\n
        ///    Take Care: Depending on the position of the tip rack the state 
        ///               may be reflected as NotPickable or NotAccessible
        ///               also.
        ///  - BadTip - to set the tip as "bad" (this tip is not pickable)
        ///  .
        /// </summary>
        /// <param name="TipRack">The Tips rack to change the tip position states</param>
        /// <param name="NewState">The new state </param>
        public void SetTipRackState(IItem TipRack, STipPositionState NewState)
        {
            STipPositionCollection tpc = TipRack.TipPositions;
            if (tpc != null)
            {
                foreach (ITipPosition tp in tpc)
                {
                    tp.State = NewState;
                }
            }
        }

        public void DeactiveTipRack(IItem TipRack)
        {
            STipPositionCollection tpc = TipRack.TipPositions;
            if (tpc != null)
            {
                foreach (ITipPosition tp in tpc)
                {
                    if (tp.State == STipPositionState.Pickable)
                    {
                        tp.State = STipPositionState.BadTip;
                    }
                }
            }
        }

        public void ActiveTipRack(IItem TipRack)
        {
            STipPositionCollection tpc = TipRack.TipPositions;
            if (tpc != null)
            {
                foreach (ITipPosition tp in tpc)
                {
                    if (tp.State == STipPositionState.BadTip)
                    {
                        tp.State = STipPositionState.Pickable;
                    }
                }
            }
        }

        #endregion

        #region public int GetNumberOfTips(IItem TipRack, STipPositionState State)

        /// <summary>
        /// Get the number tips with a specific state of a tip rack.
        /// 
        /// This method gets the number of tip positions with the defined state
        /// on the defined tip rack. e.g. \code
        ///     GetNumberOfTips(TipRack,STipPositionState.Pickable);
        /// \endcode will return all pickable tips on the defined tip rack.
        /// </summary>
        /// <param name="TipRack">The tip rack to check </param>
        /// <param name="State">The state to check</param>
        /// <returns>The number of tip positions on the defined rack having the defined state.</returns>
        public int GetNumberOfTips(IItem TipRack, STipPositionState State)
        {
            int Count = 0;
            STipPositionCollection tpc;
            if (TipRack is IItem)
            {
                tpc = TipRack.TipPositions;
            }
            else
            {
                tpc = Layout.GetTipPositions();
            }
            if (tpc != null)
            {
                foreach (ITipPosition tp in tpc)
                {
                    if (tp.State == State) Count++;
                }
            }
            return Count;
        }

        #endregion

        #region private class TipStateSaveData

        /// <summary>
        /// Tip state save data
        /// 
        /// This class is used to save the tip states of a layout. In addition to 
        /// the tip states (organized by the tip racks) the layout name is stored.
        /// It will also be possible to store the tip position array of a layout 
        /// directly, but this will have no real information about the concerned 
        /// tip racks and the layout in addition the tip position order will not 
        /// be defined.\n
        /// Important:  As this class may get created by SXmlConverter it is important 
        ///             that it has a default constructor!
        /// </summary>
        private class TipStateSaveData
        {
            #region private class TipRackSaveData

            /// <summary>
            /// internal class to manage tip rack save data
            /// 
            /// This class is used to store tip rack save data. In addition to the tip 
            /// position states it will store the no of tip positions on each rack and 
            /// the tip rack name for verification.\n
            /// Before getting and setting the tip positions states the tip position are
            /// sorted by its position. This will avoid discrepancies by changing the tip 
            /// position order in layout.\n
            /// Important:  As this class my get crated by SXmlConverter it is important 
            ///             that it has a default constructor!
            /// </summary>
            public class TipRackSaveData
            {
                #region public String TipRackName // Tip rack Name

                /// <summary>Tip rack Name field</summary>
                private String _TipRackName;

                /// <summary>Tip rack Name property</summary>
                [SStreamable, Browsable(true), DisplayName("TipRackName")]
                [Description("Tip rack Name")]
                public String TipRackName
                {
                    get { return _TipRackName; }
                    set { _TipRackName = value; }
                }

                #endregion

                #region public int NoOfTipPos // No of tip positions

                /// <summary>No of tip positions property</summary>
                [SStreamable]
                public int NoOfTipPos
                {
                    get
                    {
                        if (String.IsNullOrEmpty(_TipPosStates)) return 0;
                        return _TipPosStates.Length;
                    }
                    set
                    {
                        if (String.IsNullOrEmpty(_TipPosStates))
                        {
                            // no string yet ==> assume all empty
                            _TipPosStates = ""; // .PadLeft(value,'0');
                        }
                        _TipPosStates = _TipPosStates.PadLeft(value, '0').Substring(0, value);
                    }
                }

                #endregion

                #region public String TipPosStates // Tip position states

                /// <summary>Tip position states field</summary>
                private String _TipPosStates;

                /// <summary>Tip position states property</summary>
                [SStreamable, Browsable(true), DisplayName("TipPosStates")]
                [Description("Tip position states")]
                public String TipPosStates
                {
                    get { return _TipPosStates; }
                    set { _TipPosStates = value; }
                }

                #endregion

                #region TipRackSaveData()

                /// <summary>
                /// Initializes a new instance of the <b>TipRackSaveData</b> class.
                /// </summary>
                public TipRackSaveData()
                {
                    _TipRackName = "unknown";
                    _TipPosStates = "";
                }

                #endregion

                #region TipRackSaveData(IItem tipRack)

                /// <summary>
                /// Initializes a new instance of the <b>TipRackSaveData</b> class.
                /// </summary>
                /// <param name="tipRack"></param>
                public TipRackSaveData(IItem tipRack)
                {
                    _TipRackName = tipRack.Name;
                    STipPositionCollection tpc = tipRack.TipPositions;
                    tpc.Sort(new SPipettingMethods.STipPositionCoordinateComparer());
                    NoOfTipPos = tpc.Count;
                    StringBuilder sb = new StringBuilder(_TipPosStates);
                    int i = 0;
                    foreach (ITipPosition tp in tpc)
                    {
                        sb[i++] = (char) (tp.State + '0');
                    }
                    _TipPosStates = sb.ToString();
                }

                #endregion

                #region bool SetToTipRack(IItem tipRack)

                /// <summary>
                /// Set tip rack state back to tip rack
                /// </summary>
                /// <param name="tipRack">The tip rack to set the state</param>
                /// <returns>true if all ok, false in case name of count does not match</returns>
                public bool SetToTipRack(IItem tipRack)
                {
                    bool ret = true;
                    if (!_TipRackName.Equals(tipRack.Name))
                    {
                        // name not identical 
                        Trace.WriteLine("Tip rack names do not match on 'SetToTipRack'! " +
                                        "Old name = '" + _TipRackName + "'" +
                                        "New name = '" + tipRack.Name + "'");
                        ret = false; // not realy ok
                    }
                    STipPositionCollection tpc = tipRack.TipPositions;
                    tpc.Sort(new SPipettingMethods.STipPositionCoordinateComparer());
                    if (NoOfTipPos != tpc.Count)
                    {
                        Trace.WriteLine("No of tip positions does not match on 'SetToTipRack'! " +
                                        "Old = '" + NoOfTipPos + "'" +
                                        "New = '" + tpc.Count + "'");
                        NoOfTipPos = tpc.Count;
                        ret = false; // not realy ok
                    }
                    int i = 0;
                    foreach (ITipPosition tp in tpc)
                    {
                        tp.State = (STipPositionState) (_TipPosStates[i++] - '0');
                    }
                    return ret;
                }

                #endregion
            }

            #endregion

            #region public String LayoutName // Layout name

            /// <summary>Layout name field</summary>
            private String _LayoutName;

            /// <summary>Layout name property</summary>
            [SStreamable, Browsable(true), DisplayName("LayoutName")]
            [Description("Layout name")]
            public String LayoutName
            {
                get { return _LayoutName; }
                set { _LayoutName = value; }
            }

            #endregion

            #region public List<TipRackSaveData> LayoutTipState // layout tip state

            /// <summary>layout tip state field</summary>
            private List<TipRackSaveData> _LayoutTipState;

            /// <summary>layout tip state property</summary>
            [SStreamable, Browsable(true), DisplayName("LayoutTipState")]
            [Description("layout tip state")]
            protected List<TipRackSaveData> LayoutTipState
            {
                get { return _LayoutTipState; }
                set { _LayoutTipState = value; }
            }

            #endregion

            #region public TipStateSaveData()

            /// <summary>
            /// Initializes a new instance of the <b>TipStateSaveData</b> class.
            /// </summary>
            public TipStateSaveData()
            {
                _LayoutTipState = new List<TipRackSaveData>();
            }

            #endregion

            #region public void GetFromTipRacks(ILayout Layout, List<IItem> TipRacks)

            /// <summary>
            /// Get tip position states from tip racks
            /// </summary>
            /// <param name="Layout">The concerned layout</param>
            /// <param name="TipRacks">The concerned racks</param>
            public void GetFromTipRacks(ILayout Layout, List<IItem> TipRacks)
            {
                LayoutName = Layout.Name;
                LayoutTipState = new List<TipRackSaveData>();
                foreach (IItem tipRack in TipRacks)
                {
                    LayoutTipState.Add(new TipRackSaveData(tipRack));
                }
            }

            #endregion

            #region public bool SetToTipRacks(ILayout Layout)

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Layout"></param>
            /// <returns></returns>
            public bool SetToTipRacks(ILayout Layout)
            {
                bool ret = true;
                foreach (TipRackSaveData trs in LayoutTipState)
                {
                    IItem tipRack = Layout.GetItemByName(trs.TipRackName);
                    if (tipRack is IItem)
                    {
                        // seems to be a valid tip rack ==> set loaded tip states 
                        ret |= trs.SetToTipRack(tipRack);
                    }
                    else
                    {
                        ret = false; // tip rack missing, may be incorrect state
                    }
                }
                return ret;
            }

            #endregion
        }

        #endregion

        #region private String ValidatedTipStateFileName(String fileName)

        /// <summary>
        /// Validate tip state file name
        /// 
        /// This internal method is used to validate a given tip state file.
        /// In case there is no file name of the file path is not rooted it 
        /// will complete the required information.
        /// </summary>
        /// <param name="fileName">given file name</param>
        /// <returns>Rooted file name for saving tip states</returns>
        private String ValidatedTipStateFileName(String fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                // no name defined ==> use layout name 
                if (String.IsNullOrEmpty(LayoutName))
                {
                    // no valid layout name ==> use default name
                    fileName = SConfigurationManager.GetString("DefaultTipStateFile", "Default.TipState.xml");
                }
                else
                {
                    fileName = LayoutName;
                    if (fileName.EndsWith(".Layout.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        // cut full extension 
                        fileName = fileName.Substring(0, fileName.Length - 11);
                    }
                    else if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        // cut short extension 
                        fileName = fileName.Substring(0, fileName.Length - 4);
                    }
                    fileName = fileName + ".TipState.xml";
                }
            }
            if (!Path.IsPathRooted(fileName))
            {
                // file not rooted ==> expand with layout path (this is layout data)
                fileName = Path.Combine(SConfigurationManager.LayoutPath, fileName);
            }
            return fileName;
        }

        #endregion

        #region public bool SaveTipStates(String fileName)

        /// <summary>
        /// Save tip states to file
        /// 
        /// This method shows how tip state saving can be done tip rack 
        /// depending. To reload the stored tip states you can use the 
        /// LoadTipStates method.
        /// </summary>
        /// <param name="fileName">The name of the file to save the tip states</param>
        /// <returns>true if saved successfully, otherwise false</returns>
        public bool SaveTipStates(String fileName)
        {
            TipStateSaveData SaveState = new TipStateSaveData();
            SaveState.GetFromTipRacks(Layout, TipRacks);
            // validate / get save file name
            fileName = ValidatedTipStateFileName(fileName);
            // save tip states to file 
            bool ret = SXmlConverter.ToXMLFile(fileName, SaveState, true);
            //
            return ret;
        }

        #endregion

        #region public bool LoadTipStates(String fileName)

        /// <summary>
        /// Load tip states to file
        /// 
        /// This method shows how tip state loading can be done tip rack 
        /// depending. To save the tip states you can use the SaveTipStates 
        /// method.
        /// </summary>
        /// <param name="fileName">The name of the file to load the tip states</param>
        /// <returns>true if saved successfully, otherwise false</returns>
        public bool LoadTipStates(String fileName)
        {
            TipStateSaveData SaveState = new TipStateSaveData();
            // validate / get save file name
            fileName = ValidatedTipStateFileName(fileName);
            // save tip states to file 
            bool ret = SXmlConverter.FromXMLFile(SaveState, fileName);
            //
            ret &= SaveState.SetToTipRacks(Layout);
            //
            return ret;
        }


        #endregion

        #endregion

        #region public bool StartSequence(STipMap UsedTips)

        /// <summary>
        /// Start a process sequence
        /// 
        /// This method is used to prepare the execution of a process sequence.\n
        /// Actually it takes care
        ///  - that the loaded robot is connected;
        ///  - that the robot error is reset;
        ///  - that the tips used on the sequecne are marked as "ok".
        ///  .
        /// </summary>
        /// <param name="UsedTips">The tips used on the sequence</param>
        /// <returns>true is sequence start successful</returns>
        public bool StartSequence(STipMap UsedTips)
        {
            // optionally connect to robot
            if (!RobotConnected) RobotConnected = true;
            // 
            bool ret = RobotConnected;
            // make sure the robot error is reset
            Robot.RobotError = 0; // no error yet
            // make sure all tips are ok
            if ((UsedTips is STipMap) && UsedTips.AnyTipUsed)
            {
                // sequence requires tips
                if (PipMethods is SPipettingMethods)
                {
                    // set all tips to usable
                    PipMethods.OkTips = UsedTips;
                }
                else
                {
                    ret = false;
                }
            }
            // here you may add other prerequirements for your sequence

            return ret;
        }

        #endregion

        //add by zj 20170119
        //public SShakerDevice Shaker
        //{
        //    get
        //    {
        //        return new SShakerDevice(114, 1000, 500, 0, 3); //Shaker Address = 72; 即114；
        //    }
        //}


        /// <summary>
        /// 使用的ADP数量=3
        /// </summary>
        private const int ADP_COUNT = 3;

        /// <summary>
        /// ADP打开取六联排时的间距
        /// </summary>
        private const double STRIP_PADDING_MAX = 119d;

        /// <summary>
        /// ADP加紧六联排时的间距
        /// </summary>
        private const double STRIP_PADDING_MIN = 109d;

        /// <summary>
        /// ADP由打开变为加紧时，每个ADP移动的距离=(STRIP_PADDING_MAX-STRIP_PADDING_MIN)/2
        /// </summary>
        private const double GRIP_PADDING = 5;

        /// <summary>
        /// Z轴安全高度，要保证夹着六联排时，六联排底部不会撞到其他部件
        /// </summary>
        private const double SAFE_Z = 230d;
            //ProcessWorkbench.Workbench.PipMethods.Arm.YZDevices[0].ZWorktablePos(0);//230d;

        /// <summary>
        /// /*
        /// * Block move mode:
        /// * 0: normal move (full power) 
        /// * 1: block move 1 (full power, reduced block recognition time) 
        /// * 2: block move 2 (medium power, reduced block recognition time) 
        /// * 3: block move 3 (low power, reduced block recognition time)  
        /// */
        /// </summary>
        private const byte BLOCK_MODE = 0;

        /// <summary>
        /// The moving speed (all the same, 0=default motor speed) 
        /// </summary>
        private const double GRIP_SPEED = 0;

        #region Move Item

        /// <summary>
        /// 移动六联排
        /// </summary>
        /// <param name="source">源位置</param>
        /// <param name="destination">目标位置</param>
        public void MoveItem(SPosition source, SPosition destination)
        {
            const double Xoffset = 2.84;
            const double Yoffset = 8.84;
            const double dADP = 6.8; //ADP 直径
            const double RDCupWidth = 119; // 六联排宽度=119
            const double RDCupLength = 17; //六联排长度=117
            const double rGripCavity = 3.25; //抓孔半径=3.25
            const double dADPMin = 5; //ADP小直径=5
            const double padding = 0.75; //抓孔与六联排顶/底部的边距
            const double hSlot = 42.5; //Slot高度=42.5
            const double hADP = 18; //ADP高度=18

            STipMap UsedTips = Workbench.PipMethods.AllTips.SingleTip(1) + Workbench.PipMethods.AllTips.SingleTip(2);
            //取得source的各个坐标
            double sourceX = source.X + 8.5 - Xoffset; // X offset = 2.84
            //由中心位置计算出两端位置
            double sourceY1 = source.Y - Yoffset - 1.8;
            double sourceY2 = source.Y - Yoffset + RDCupWidth + 1.8;
            double[] sourceY = new[] {0, sourceY1, sourceY2};
            //计算出夹取位置
            double sourceGripY1 = source.Y - Yoffset + padding + rGripCavity - dADPMin/2;
            double sourceGripY2 = source.Y - Yoffset + RDCupWidth - padding - rGripCavity + dADPMin/2;
            double[] sourceGripY = new double[] {0, sourceGripY1, sourceGripY2};
            //由Z坐标计算夹取时的Z坐标
            double sourceZ = source.Z - 5;
            //X,Y移动至源位置
            Workbench.PipMethods.MoveXY(sourceX, sourceY);
            Robot.GetArm(0).NoTravelMove = true;
            //Z轴下移
            Workbench.PipMethods.BlockMoveZ(UsedTips, sourceZ, BLOCK_MODE, GRIP_SPEED);

            //ADP向内加紧
            Workbench.PipMethods.MoveXY(sourceX, sourceGripY);
            Robot.GetArm(0).NoTravelMove = false;
            //Z轴上移
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);

            //取得destination的各个坐标
            double destinationX = destination.X + RDCupLength/2 - Xoffset;
            double destinationY1 = destination.Y - Yoffset - 1.8;
            double destinationY2 = destination.Y - Yoffset + RDCupWidth + 1.8;
            double[] destinationY = new double[] {0, destinationY1, destinationY2};
            double destinationGripY1 = destination.Y - Yoffset + padding + rGripCavity - dADPMin/2;
            double destinationGripY2 = destination.Y - Yoffset + RDCupWidth - padding - rGripCavity + dADPMin/2;
            double[] destinationGripY = new double[] {0, destinationGripY1, destinationGripY2};
            double destinationZ = destination.Z + hSlot - hADP;

            //X,Y移动至目标位置
            Workbench.PipMethods.MoveXY(destinationX, destinationGripY);
            //Z轴下移
            Workbench.PipMethods.BlockMoveZ(UsedTips, destinationZ, BLOCK_MODE, GRIP_SPEED);
            Robot.GetArm(0).NoTravelMove = true;
            //ADP向两侧移动，松开六联排
            Workbench.PipMethods.MoveXY(destinationX, destinationY);
            Robot.GetArm(0).NoTravelMove = false;
            //Z轴上移
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);
        }

        private const double Xoffset = 2.84;
        private const double Yoffset = 8.84;
        private const double dADP = 6.8; //ADP 直径
        private const double RDCupWidth = 119; // 六联排宽度=119
        private const double RDCupLength = 17; //六联排长度=117
        private const double rGripCavity = 3.25; //抓孔半径=3.25
        private const double dADPMin = 5; //ADP小直径=5
        private const double padding = 0.75; //抓孔与六联排顶/底部的边距
        private const double hSlot = 42.5; //Slot高度=42.5
        private const double hADP = 18; //ADP高度=18


        /// <summary>
        /// 搬运六联排
        /// </summary>
        /// <param name="source">Source Item</param>
        /// <param name="destination">Destination Slot</param>
        public bool MoveItem(IItem source, ISlot destination)
        {
            bool flag = false;
            //当前Item为空
            if (source == null)
            {
                //System.Windows.MessageBox.Show("No item at source position. ");
                string message = "No item at source position. ";
                ReportMessageEvent(PopupType.ShowMessage, message, null);
                return flag;
            }
            //目标Slot已被占用
            if (destination.CurrentItem != null)
            {
                //System.Windows.MessageBox.Show("There is an item at destination position.");
                string message = "There is an item at destination position.";
                ReportMessageEvent(PopupType.ShowMessage, message, null);
                return flag;
            }

            int from = GetSlotPos(source.CurrentSlot);
            int to = GetSlotPos(destination);
            RefreshUiEvent(RdAction.Moving, from, to); //开始搬运，刷新界面
            //当前Item所在的Slot
            ISlot slot = source.CurrentSlot;

            //取得source的各个坐标
            double sourceX = source.Position.X + RDCupLength/2 - Xoffset;
            //计算出两端位置
            double sourceY1 = source.Position.Y - Yoffset - 1.8;
            double sourceY2 = source.Position.Y - Yoffset + RDCupWidth + 1.8;
            double[] sourceY = new double[ADP_COUNT];

            sourceY = new[] {0, sourceY1, sourceY2}; //三通道
            //sourceY = new[] { sourceY1, sourceY2 }; //2通道

            //计算出夹取位置
            double sourceGripY1 = source.Position.Y - Yoffset + padding + rGripCavity - dADPMin/2;
            double sourceGripY2 = source.Position.Y - Yoffset + RDCupWidth - padding - rGripCavity + dADPMin/2;
            double[] sourceGripY = new double[ADP_COUNT];
            sourceGripY = new double[] {0, sourceGripY1, sourceGripY2}; //三通道
            //sourceGripY = new double[] {sourceGripY1, sourceGripY2 }; //2通道
            //由Z坐标计算夹取时的Z坐标
            //  double sourceZ = source.Position.Z - 26.5-18;//2;//3:ADP变径处与六联排夹取处的空间;此处将-3改为-2，让ADP抓取时尽量靠上方。
            double sourceZ = source.Position.Z - 26.5 - 18 - 2; //X-VR中再减2
            STipMap usedTip = new STipMap();
            //三通道，使用第二通道和第三通道；
            usedTip = Workbench.PipMethods.AllTips.SingleTip(1) + Workbench.PipMethods.AllTips.SingleTip(2);
            //1通道，使用第1通道和第2通道；
            //usedTip = Workbench.PipMethods.AllTips.SingleTip(0) + Workbench.PipMethods.AllTips.SingleTip(1);

            //Z轴安全高度
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);
            //X,Y移动至源位置
            Workbench.PipMethods.MoveXY(sourceX, sourceY);
            //Z轴下移
            Workbench.PipMethods.BlockMoveZ(usedTip, sourceZ, BLOCK_MODE, GRIP_SPEED);
            //不强制Z轴到安全高度
            Robot.GetArm(0).NoTravelMove = true;
            //ADP向内加紧
            Workbench.PipMethods.MoveXY(sourceX, sourceGripY);
            //强制Z轴到安全高度
            Robot.GetArm(0).NoTravelMove = false;
            //Z轴上移
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);
            ////清空Slot的CurrentItem
            //slot.CurrentItem = null;
            ////更新当前Item所在的Slot
            //source.CurrentSlot = null;

            //取得destination的各个坐标
            double destinationX = destination.Position.X + RDCupLength/2 - Xoffset;
            double destinationY1 = destination.Position.Y - Yoffset - 1.8;
            double destinationY2 = destination.Position.Y - Yoffset + RDCupWidth + 1.8;
            double[] destinationY = new double[ADP_COUNT];
            destinationY = new double[] {0, destinationY1, destinationY2}; //三通道
            //destinationY = new double[] { destinationY1, destinationY2 }; //2通道
            double destinationGripY1 = destination.Position.Y - Yoffset + padding + rGripCavity - dADPMin/2;
            double destinationGripY2 = destination.Position.Y - Yoffset + RDCupWidth - padding - rGripCavity + dADPMin/2;
            double[] destinationGripY = new double[ADP_COUNT];
            destinationGripY = new double[] {0, destinationGripY1, destinationGripY2}; //三通道
            //destinationGripY = new double[] {destinationGripY1, destinationGripY2 }; //2通道
            //double destinationZ = destination.Position.Z + hSlot - hADP;//实际运行
            double destinationZ = destination.Position.Z + hSlot - hADP - 2; //X-VR中再减2
            //X,Y移动至目标位置
            Workbench.PipMethods.MoveXY(destinationX, destinationGripY);
            //Z轴下移
            Workbench.PipMethods.BlockMoveZ(usedTip, destinationZ, BLOCK_MODE, GRIP_SPEED);
            //不强制Z轴到安全高度
            Robot.GetArm(0).NoTravelMove = true;
            //ADP向两侧移动，松开六联排
            Workbench.PipMethods.MoveXY(destinationX, destinationY);
            //强制Z轴到安全高度
            Robot.GetArm(0).NoTravelMove = false;
            //Z轴上移
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);
            //清空Slot的CurrentItem
            slot.CurrentItem = null;
            //更新当前Item所在的Slot
            source.CurrentSlot = destination;
            if (destination.Name.Contains("RD_DropStation"))
            {
                destination.CurrentItem = null;
                Workbench.Layout.RemoveItem(source);
            }
            flag = true;
            RefreshUiEvent(RdAction.MoveItem, from, to); //搬运完成刷新界面
            return flag;
        }

        #endregion

        /// <summary>
        /// 移动至洗针位置
        /// </summary>
        public void MoveToWashStation()
        {
            ICavity washCavity = Workbench.Layout.GetCavityByName("RD_WashStation1:RD_WashStation_Cavity1");
            double washX = (washCavity.FirstPos.X + washCavity.LastPos.X)/2;
            double washY1 = (washCavity.FirstPos.Y + washCavity.LastPos.Y)/2 - 10;
            double washY2 = (washCavity.FirstPos.Y + washCavity.LastPos.Y)/2;
            double washY3 = (washCavity.FirstPos.Y + washCavity.LastPos.Y)/2 + 10;
            double[] washY = new double[] {washY1, washY2, washY3};
            double washZ = washCavity.FirstPos.Z;
            //Z轴上移
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, GRIP_SPEED);
            Workbench.PipMethods.MoveXY(washX, washY);
            Workbench.PipMethods.BlockMoveZ(null, washZ, BLOCK_MODE, GRIP_SPEED);
        }

        /// <summary>
        /// 洗针
        /// </summary>
        /// <param name="volume">清洗液量</param>
        public void FlushADP(double volume)
        {
            MoveToWashStation();

            SVolumeBand vb = Workbench.GetVolumeBand("Default", "Default", volume);

            SPipettingMethods PipMethods = Workbench.PipMethods;

            //// make sure the robot is connected
            Workbench.StartSequence(PipMethods.AllTips); // start sequence using all tips

            SPipettingPar Liquid = Workbench.GetAspVolumePar(volume, vb);

            // prepare dispense parameters
            Liquid = Workbench.GetDispVolumePar(volume, vb);

            //使用所有的ADP
            STipMap usedTip = PipMethods.AllTips;

            //Flush
            PipMethods.Flush(usedTip, Liquid);
        }

        public void Aspirate(STipMap usedTips, SCavityCollection sccCavities, double volume)
        {
            #region VolumeBand

            SVolumeBand vb = new SVolumeBand();

            if (sccCavities[0].Name.Contains("MB"))
            {
                vb = Workbench.GetVolumeBand("Default", "Mag6", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("AMP"))
            {
                vb = Workbench.GetVolumeBand("Default", "AMP", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("Cup"))
            {
                vb = Workbench.GetVolumeBand("Default", "Waste", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("ReagentBox4") || sccCavities[0].Name.Contains("Paraffin"))
            {
                vb = Workbench.GetVolumeBand("Default", "Oil", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("Ez"))
            {
                vb = Workbench.GetVolumeBand("Default", "Enzyme", volume, 300);
            }
            else if (sccCavities[0].Name.Contains("PN"))
            {
                vb = Workbench.GetVolumeBand("Default", "Default", volume, 300);
            }
            else
            {
                vb = Workbench.GetVolumeBand("Default", "Default", volume, 1000);
            }

            #endregion VolumeBand

            SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");
            SPipettingPar aspLiquid = Workbench.GetAspVolumePar(volume + vb.SpitBack, vb); //实际吸液体积需加上SpitBack；
            SPipettingPar TransAir = Workbench.GetAirPar(vb.TransportationAirgap);
            SPipettingPar Spit = Workbench.GetAspVolumePar(vb.SpitBack, vb);
            SDetectionPar DetPar = dc.DetectionParameters;
            SMixPar MixPar = new SMixPar();
            SPipettingPar SysAir = Workbench.GetAirPar(vb.SystemAirgap);

            PipMethods.ExecuteTemplate(usedTips, sccCavities,
                new RdAspTemp(SysAir, aspLiquid, TransAir, Spit, MixPar, DetPar));
        }

        public void Dispense(STipMap usedTips, SCavityCollection sccCavities, double volume)
        {
            #region VolumeBand

            SVolumeBand vb = new SVolumeBand();

            if (sccCavities[0].Name.Contains("MB"))
            {
                vb = Workbench.GetVolumeBand("Default", "Mag6", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("AMP"))
            {
                vb = Workbench.GetVolumeBand("Default", "AMP", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("Cup"))
            {
                vb = Workbench.GetVolumeBand("Default", "Waste", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("ReagentBox4") || sccCavities[0].Name.Contains("Paraffin"))
            {
                vb = Workbench.GetVolumeBand("Default", "Oil", volume, 1000);
            }
            else if (sccCavities[0].Name.Contains("Ez"))
            {
                vb = Workbench.GetVolumeBand("Default", "Enzyme", volume, 300);
            }
            else if (sccCavities[0].Name.Contains("PN"))
            {
                vb = Workbench.GetVolumeBand("Default", "Default", volume, 300);
            }
            else
            {
                vb = Workbench.GetVolumeBand("Default", "Default", volume, 1000);
            }

            #endregion VolumeBand

            SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");
            SPipettingPar dspLiquid = Workbench.GetAspVolumePar(volume, vb);
            SPipettingPar TransAir = Workbench.GetAirPar(vb.TransportationAirgap);
            SDetectionPar DetPar = dc.DetectionParameters;
            SMixPar MixPar = new SMixPar();
            //SPipettingPar SysAir = Workbench.GetAirPar(vb.SystemAirgap);

            PipMethods.ExecuteTemplate(usedTips, sccCavities, new RdDspTemp(dspLiquid, MixPar, DetPar, TransAir));
        }

        //sdk used
        public void DropTips(STipMap usedTips)
        {
            double safeZ = ProcessWorkbench.Workbench.PipMethods.Arm.YZDevices[0].ZWorktablePos(0);
            Workbench.PipMethods.BlockMoveZ(PipMethods.AllTips, safeZ, 0, 0);

            double X = Workbench.PipMethods.Arm.XWorktablePosition;
            double y0 = Workbench.PipMethods.Arm.YZDevices[0].YWorktablePosition;
            double y1 = Workbench.PipMethods.Arm.YZDevices[1].YWorktablePosition;
            double y2 = Workbench.PipMethods.Arm.YZDevices[2].YWorktablePosition;
            double[] Y = new double[] {y0, y1, y2};

            double X1 = Workbench.Layout.GetItemByName("RD_Heating1").Position.X;
            if (X < X1)
            {
                double tempy2 = Workbench.Layout.GetSlotByName("RD_CupRack1:RD_CupRack_CupSlot1").Position.Y - 6;
                    //6:离孔6mm的 位置;
                double tempy1 = tempy2 - 9;
                double tempy0 = tempy2 - 18;
                double[] tempY = {tempy0, tempy1, tempy2};
                PipMethods.MoveXY(X + 8.5, Y);
                PipMethods.MoveXY(X + 8.5, tempY);
                PipMethods.MoveXY(X1, tempY);

                //更新当前坐标
                X = Workbench.PipMethods.Arm.XWorktablePosition;
                y0 = Workbench.PipMethods.Arm.YZDevices[0].YWorktablePosition;
                y1 = Workbench.PipMethods.Arm.YZDevices[1].YWorktablePosition;
                y2 = Workbench.PipMethods.Arm.YZDevices[2].YWorktablePosition;
                Y = new double[] {y0, y1, y2};
            }

            double X2 = Workbench.Layout.GetSlotByName("DropStation1:RD_DropStation_CupSlot1").Position.X;
            //          double Y1 = Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y;//Heating CupSlot最内侧
            double Y2 = Workbench.Layout.GetSlotByName("RD_Reader1:RD_Reader_CupSlot1").Position.Y + 101.32;
                //Reader CupSlot最外侧

            double[] tempY2 = {Y2, Y2 - 9, Y2 - 18};

            PipMethods.MoveXY(X + 8.5, Y);
            PipMethods.MoveXY(X + 8.5, tempY2);
            PipMethods.MoveXY(X2, tempY2);

            SDropStation ds = Workbench.DropStation;
            SDropTipsPar DropTipsPar = new SDropTipsPar();
            DropTipsPar.DisplayDropTipsError = true;

            PipMethods.DropTips(usedTips, DropTipsPar, ds);
        }

        /// <summary>
        /// 加液
        /// </summary>
        /// <param name="Destination">目标位置</param>
        /// <param name="volume">加液量</param>
        public void Dispense(SCavityCollection Destination, double volume, string Category = "Default")
        {
            SPipettingMethods PipMethods = Workbench.PipMethods;

            SVolumeBand vb = new SVolumeBand();
            switch (Category)
            {
                case "Mag6":
                    vb = Workbench.GetVolumeBand("Default", "Mag6", volume);
                    break;
                case "Washing":
                    vb = Workbench.GetVolumeBand("Default", "Washing", volume);
                    break;
                case "AMP":
                    vb = Workbench.GetVolumeBand("Default", "AMP", volume);
                    break;
                case "Waste":
                    vb = Workbench.GetVolumeBand("Default", "Waste", volume);
                    break;
                case "Oil":
                    vb = Workbench.GetVolumeBand("Default", "Oil", volume);
                    break;
                case "Enzyme":
                    vb = Workbench.GetVolumeBand("Default", "Enzyme", volume);
                    break;
                case "Urine":
                    vb = Workbench.GetVolumeBand("Default", "Urine", volume);
                    break;
                case "Default":
                default:
                    vb = Workbench.GetVolumeBand("Default", "Default", volume);
                    break;
            }
            SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");

            STipMap UsedTips = PipMethods.AllTips.SingleTip(0);
            //double aspV = volume + vb.SpitBack + vb.WasteVolume;
            double aspV = volume;
            // first air gap to separate aspirated liquid 
            // from system liquid 
            //SPipettingPar SepAir = GetAirPar(vb.SeparationAirgap);    // not used, as we only use one liquid
            SPipettingPar Liquid = Workbench.GetAspVolumePar(aspV, vb);
            // liquid volume (including speeds and ramps)
            SPipettingPar TransAir = Workbench.GetAirPar(vb.TransportationAirgap);

            // Get spit back parameters. 
            // Detection parameter
            SDetectionPar DetPar = dc.DetectionParameters;
            // DetPar.UseLiquidDetection = true;  // it is also possible to change the detection mode afterwards
            SMixPar MixPar = new SMixPar();

            #region // Dispense liquid

            //Z轴安全高度
            Workbench.PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, 220d, 0, 0);

            // prepare dispense parameters
            Liquid = Workbench.GetDispVolumePar(volume, vb);
            Liquid += TransAir;
            // on example we used fix detection/tracking parameters
            DetPar = new SDetectionPar((int) SDetectionPar.ModeMask.UseTracking);

            if (Destination[0].Name == "RD_WashStation1:RD_WashStation_Cavity1")
            {
                //不强制Z轴到安全高度
                Workbench.Robot.GetArm(0).NoTravelMove = true;
            }
            // Dispense
            PipMethods.Dispense(UsedTips, Destination, Liquid, MixPar, DetPar, TransAir);
            if (Destination[0].Name == "RD_WashStation1:RD_WashStation_Cavity1")
            {
                //不强制Z轴到安全高度
                Workbench.Robot.GetArm(0).NoTravelMove = false;
            }

            #endregion
        }

        public bool GetTips(STipMap UsedTips, double tipV)
        {
            bool flag = false;
            //ExecutionMethod Sequence = null;
            //Sequence = delegate(ProcessWorkbench WorkBench)
            //{
            SPipettingMethods PipMethods = Workbench.PipMethods;

            // make sure the robot is connected
            Workbench.StartSequence(PipMethods.AllTips); // start sequence using all tips

            //STipMap UsedTips = PipMethods.AllTips.SingleTip(0);
            STipMap UsedDT = Workbench.DTAdapter.Combine(UsedTips); // used DT adapters

            #region // Get disposable tip (optional)

            if (UsedDT.AnyTipUsed)
            {
                // We have used disposable tip on the sequence
                // Important: Please note that Tip check is not working yet!
                SGetTipsPar gtp = new SGetTipsPar
                {
                    DisplayHasTipsError = true,
                    DisplayBadTipsError = true
                };
                //2017-03-09 zhangjing
                //Tip头用完后，提示用户填充Tip头；
                if (tipV == 300)
                {
                    ITipType tt = new STipType("Rainin Universal 300");
                    IItem tipRack = Workbench.Layout.GetItemByName("DTRack1");
                    int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                    if (tipNum < UsedDT.NoOfUsedTips)
                    {
                        //System.Windows.MessageBox.Show("300ul Tip头不足，请补充后点击确定按钮！");
                        string message = "300ul Tip头不足，请补充后点击确定按钮！";
                        ReportMessageEvent(PopupType.ShowMessage, message, null);
                        FillTips(1);
                    }
                    PipMethods.GetTips(UsedDT, tt, gtp);
                    //保存Tip头信息
                    Workbench.SaveTipStates("Rendu.TipStates.xml");
                    //界面更新Tip头状态
                    RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                    flag = true;
                }
                else if (tipV == 1000)
                {
                    ITipType tt = new STipType("Rainin Universal 1000");
                    IItem tipRack = Workbench.Layout.GetItemByName("DTRack9");
                    int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                    if (tipNum < UsedDT.NoOfUsedTips)
                    {
                        //System.Windows.MessageBox.Show("1000ul Tip头不足，请补充后点击确定按钮！");
                        string message = "1000ul Tip头不足，请补充后点击确定按钮！";
                        ReportMessageEvent(PopupType.ShowMessage, message, null);
                        Fill1000Tips();
                    }
                    PipMethods.GetTips(UsedDT, tt, gtp);
                    //保存Tip头信息
                    Workbench.SaveTipStates("Rendu.TipStates.xml");
                    //界面更新Tip头状态
                    RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                    flag = true;
                }
            }
            return flag;

            #endregion
        }

        /// <summary>
        /// 磁珠抽打次数
        /// </summary>
        private const int MAG_MIX_TIMES = 3;

        /// <summary>
        /// The moving speed (all the same, 0=default motor speed) 
        /// </summary>
        private const double ADP_MOVE_SPEED = 0;

        private const double CavityTravelHeight = 3;

        //public bool MyErrorHandler(Object sender, SErrorArgs errorArgs, bool handled)
        //{
        //    string strLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag;
        //    Type t = sender.GetType();
        //    string strLocation=t.Assembly.Location;
        //    string strError= SErrorInfo.GetErrorMessage(errorArgs.Code, sender, errorArgs.AdditionalInfo);
        //    if (sender is SPipettingMethods)
        //    {
        //        if (errorArgs.Code == SPipettingMethods.ErrorCodes.NotEnoughLiquidDetected)
        //        {
        //            errorArgs.Result=SErrorResult.Ignore;
        //            errorArgs.HandledBy = this;
        //            return true;
        //        }
        //    }
        //    return false;
        //}



        #region 吸多加多

        /// <summary>
        /// 吸多加多
        /// </summary>
        /// <param name="Source">吸的孔</param>
        /// <param name="Destination">加的孔集合</param>
        /// <param name="Volume">每孔加的量</param>
        /// <param name="tipV">使用的Tip头规格</param>
        /// <param name="Category">液体参数类型</param>
        public void Transfer(SCavityCollection Source, SCavityCollection Destination, double Volume, double tipV = 1000,
            string Category = "Default")
        {
            //如果源位置与目标位置个数不符，则提示
            if (Source.Count != Destination.Count)
            {
                //System.Windows.MessageBox.Show("Count of source cavities does not match number of destination cavities","Warning");
                string message = "Count of source cavities does not match number of destination cavities";
                ReportMessageEvent(PopupType.ShowMessage, message, null);
                return;
            }

            SVolumeBand vb = new SVolumeBand();
            switch (Category)
            {
                case "Mag6":
                    vb = Workbench.GetVolumeBand("Default", "Mag6", Volume, tipV);
                    break;
                case "Washing":
                    vb = Workbench.GetVolumeBand("Default", "Washing", Volume, tipV);
                    break;
                case "AMP":
                    vb = Workbench.GetVolumeBand("Default", "AMP", Volume, tipV);
                    break;
                case "firstWaste":
                case "secondWaste":
                case "lastWaste":
                    vb = Workbench.GetVolumeBand("Default", "Waste", Volume, tipV);
                    break;
                case "Oil":
                    vb = Workbench.GetVolumeBand("Default", "Oil", Volume, tipV);
                    break;
                case "Enzyme":
                    vb = Workbench.GetVolumeBand("Default", "Enzyme", Volume, tipV);
                    break;
                case "Urine":
                    vb = Workbench.GetVolumeBand("Default", "Urine", Volume, tipV);
                    break;
                case "Default":
                default:
                    vb = Workbench.GetVolumeBand("Default", "Default", Volume, tipV);
                    break;
            }
            //SVolumeBand vb = Workbench.GetVolumeBand("Default", "Default", Volume, tipV);
            SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");

            ////add 20170608
            //int usedTipCount = 3;
            //switch (Source.Count)
            //{
            //    case 6:
            //        if (IsSame(Source))
            //        {
            //            usedTipCount = 3;
            //        }
            //        else
            //        {
            //            usedTipCount = 1;
            //        }

            //        break;
            //    case 5:
            //        if (IsSame(Source))
            //        {
            //            if (Source[0].Name.Contains("ReagentBox3") || Source[0].Name.Contains("ParaffinBox"))
            //            {
            //                usedTipCount = 3;
            //            }
            //            else
            //            {
            //                usedTipCount = 1;
            //            }
            //        }
            //        break;
            //    case 4:
            //        if (IsSame(Source))
            //        {
            //            if (Source[0].Name.Contains("ReagentBox3") || Source[0].Name.Contains("ParaffinBox"))
            //            {
            //                usedTipCount = 2;
            //            }
            //            else
            //            {
            //                usedTipCount = 1;
            //            }
            //        }
            //        break;
            //    case 3:
            //        if (Source[0].Name == Source[1].Name && Source[0].Name == Source[2].Name)
            //        {

            //            usedTipCount = 1;
            //        }
            //        else if (Source[0].Name == Source[1].Name)
            //        {
            //            usedTipCount = 2;
            //        }
            //        else if (Source[0].Name == Source[2].Name)
            //        {
            //            usedTipCount = 2;
            //        }
            //        else if (Source[1].Name == Source[2].Name)
            //        {
            //            usedTipCount = 2;
            //        }
            //        else
            //        {
            //            usedTipCount = 3;
            //        }
            //        break;
            //    case 2:
            //        if (Source[0].Name == Source[1].Name)
            //        {
            //            usedTipCount = 1;
            //        }
            //        else
            //        {
            //            usedTipCount = 2;
            //        }
            //        break;
            //    case 1:
            //        usedTipCount = 1;
            //        break;
            //}
            //加样次数
            int addTimes = Destination.Count/ ADP_COUNT;

            //如果有余数，加样次数加1
            if (Destination.Count% ADP_COUNT != 0)
            {
                addTimes += 1;
            }
            STipMap UsedTips = new STipMap();
            SPipettingMethods PipMethods = Workbench.PipMethods;
            // make sure the robot is connected
            Workbench.StartSequence(PipMethods.AllTips); // start sequence using all tips
            //如果去同一个孔吸，并且吸的量*孔数<Tip头容量，则只用第一个Tip去吸
            //if (IsSame(Source) && (Volume * Source.Count < tipV))
            //{
            //    UsedTips = PipMethods.AllTips.SingleTip(0);
            //    Volume = Volume * Source.Count;//每孔量*孔数
            //}
            //else
            //{
                UsedTips = new STipMap(0, Source.Count - 1); // - ~PipMethods.AllTips;
            //}
            //ExecutionMethod Sequence = null;
            //Sequence = delegate(ProcessWorkbench WorkBench)
            //{

            STipMap UsedDT = Workbench.DTAdapter.Combine(UsedTips); // used DT adapters

            //加样次数循环
            for (var i = 1; i <= addTimes; i++)
            {
                Workbench.FlushADP(400);
                Workbench.PipMethods.BlockMoveZ(PipMethods.AllTips, SAFE_Z, 0, 0);
                #region // Get disposable tip (optional)

                if (UsedDT.AnyTipUsed)
                {
                    // We have used disposable tip on the sequence
                    // Important: Please note that Tip check is not working yet!
                    SGetTipsPar gtp = new SGetTipsPar
                    {
                        DisplayHasTipsError = true,
                        DisplayBadTipsError = true
                    };

                    //2017-03-09 zhangjing
                    //Tip头用完后，提示用户填充Tip头；
                    if (tipV == 300)
                    {
                        ITipType tt = new STipType("Rainin Universal 300");
                        IItem tipRack = Workbench.Layout.GetItemByName("DTRack1");
                        int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                        if (tipNum < UsedDT.NoOfUsedTips)
                        {
                            //System.Windows.MessageBox.Show("300ul Tip头不足，请补充后点击确定按钮！");
                            string message = "300ul Tip头不足，请补充后点击确定按钮！";
                            ReportMessageEvent(PopupType.ShowMessage, message, null);
                            FillTips(1);
                        }
                        PipMethods.GetTips(UsedDT, tt, gtp);
                        //保存Tip头信息
                        Workbench.SaveTipStates("Rendu.TipStates.xml");
                        //界面更新Tip头状态
                        RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                    }
                    else if (tipV == 1000)
                    {
                        ITipType tt = new STipType("Rainin Universal 1000");
                        IItem tipRack = Workbench.Layout.GetItemByName("DTRack9");
                        int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                        if (tipNum < UsedDT.NoOfUsedTips)
                        {
                            //System.Windows.MessageBox.Show("1000ul Tip头不足，请补充后点击确定按钮！");
                            string message = "1000ul Tip头不足，请补充后点击确定按钮！";
                            ReportMessageEvent(PopupType.ShowMessage, message, null);
                            Fill1000Tips();
                        }
                        PipMethods.GetTips(UsedDT, tt, gtp);
                        //保存Tip头信息
                        Workbench.SaveTipStates("Rendu.TipStates.xml");
                        //界面更新Tip头状态
                        RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                    }
                }

                #endregion

                //Y方向最外侧
                //double[] y = new double[] { 586, 595 };//595.5=ADP Y方向最大值
                double[] maxY = {371.4, 380.2, 389}; //389=ADP Y方向最大值;最大值取388，余量1；
                double destinationX;
                double[] destinationY = new double[3];

                #region // Aspirate liquid

                double aspirateV = 0; //实际吸液量
                if (tipV == 1000) //如果是1000ul的Tip头，多吸50ul
                {
                    aspirateV = Volume + 50;
                }
                else if (tipV == 300) //如果是300ul的Tip头，多吸10ul
                {
                    aspirateV = Volume + 10;
                    if (Source[0].Name.Contains("RD_EzBottole")) //如果是酶，再多吸10ul
                    {
                        aspirateV = Volume + 10;
                    }
                }

                // prepare aspirate parameters
                SPipettingPar SysAir = Workbench.GetAirPar(vb.SystemAirgap);
                // first air gap to separate aspirated liquid 
                // from system liquid 
                //SPipettingPar SepAir = GetAirPar(vb.SeparationAirgap);    // not used, as we only use one liquid
                //SPipettingPar aspLiquid = Workbench.GetAspVolumePar(aspirateV + vb.SpitBack + vb.WasteVolume, vb);
                SPipettingPar aspLiquid = Workbench.GetAspVolumePar(aspirateV, vb);
                // liquid volume (including speeds and ramps)
                SPipettingPar TransAir = Workbench.GetAirPar(vb.TransportationAirgap);
                // transportation air gap
                // Take care: To keep it more simple we are using the volume band of the 
                // aspiration volume. It would be more correct to use the volume band of the spit back volume. 
                SPipettingPar Spit = Workbench.GetAspVolumePar(vb.SpitBack, vb);
                // Get spit back parameters. 
                // Detection parameter
                SDetectionPar DetPar = dc.DetectionParameters;

                //debug 
                //DetPar.DisplayErrors = false;
                DetPar.DisplayErrors = true;

                // DetPar.UseLiquidDetection = true;  // it is also possible to change the detection mode afterwards
                SMixPar MixPar = new SMixPar();

                //本次加样需要吸取的孔位
                SCavityCollection usedSource = new SCavityCollection();
                //double[] cavityVolumes = new double[addTimes];
                for (var j = 0; j < Source.Count; j++)
                {
                    if (j < i*UsedTips.NoOfUsedTips && j >= (i - 1)*UsedTips.NoOfUsedTips)
                    {
                        usedSource.Add(Source[j]);
                        //cavityVolumes[i - 1] = 0;
                    }
                }
                //如果是吸废液，要先跟随液面吸，再扎底吸
                //if (Destination[0].Name.Contains("RD_WashStation1:RD_WashStation_Cavity1"))
                //{

                //2017-03-09 zhangjing
                //取完Tip头后，如果是去震荡模块或者吸磁模块，需避让读数模块
                if (usedSource[0].ParentItem.CurrentSlot.Name.Contains("RD_ShakerRack") ||
                    usedSource[0].ParentItem.CurrentSlot.Name.Contains("RD_Mag"))
                {
                    //Z轴上升到安全距离
                    PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
                    Item paraffinBox = (Item) Workbench.Layout.GetItemByName("RD_ParaffinBox1");
                    double x = paraffinBox.Position.X + 19.4; //19.4=ParaffinBox长度
                    double y0 = paraffinBox.Position.Y - 50;
                    double y1 = paraffinBox.Position.Y - 40;
                    double y2 = paraffinBox.Position.Y - 30;
                    double[] y = new double[] {y0, y1, y2};
                    PipMethods.MoveXY(x, y);
                }

                //debug
                //DetPar.DisplayErrors = false;
                DetPar.DisplayErrors = true;
                if (Category == "Oil")
                {
                    DetPar.UseLiquidDetection = false;
                    DetPar.UseTracking = false;
                    DetPar.LimitVolumeToPickable = false;

                    // Aspirate
                    PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid, TransAir, Spit, MixPar, DetPar);
                    //吸液后刷新界面
                    RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedSource), GetVolume(usedSource));
                }
                else if (Category == "firstWaste" || Category == "secondWaste")
                {
                    //第一次吸完剩90
                    //DetPar.UseLiquidDetection = true;
                    //DetPar.UseTracking = true;
                    //DetPar.LimitVolumeToPickable = false;
                    //aspLiquid.ZMode=SPipettingPar.ModeConst.RelativeToMaxHeight;

                    DetPar.UseLiquidDetection = false;
                    DetPar.UseTracking = true;
                    DetPar.LimitVolumeToPickable = false;
                    aspLiquid.ZMode = SPipettingPar.ModeConst.RelativeToLiquidPosition;

                    for (var k = 0; k < UsedTips.MaxTip(usedSource); k++)
                    {
                        //usedSource[k].Liquid.Volume = aspLiquid.VolArr[k];
                        usedSource[k].Liquid = new SLiquid("sample", aspLiquid.VolArr[k], new SLiquidState());
                        aspLiquid.ZOffset[k] = -2;
                    }


                    //                    SErrorManager.AddQuietHandler(MyErrorHandler);
                    // Aspirate
                    PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid, TransAir, Spit, MixPar, DetPar);
                    //                   SErrorManager.RemoveQuietHandler(MyErrorHandler);
                    //刷新界面
                    RefreshUiEvent(RdAction.Waste, GetNapId(usedSource), GetWasteCells(usedSource));
                }
                else if (Category == "lastWaste")
                {
                    //DetPar.UseLiquidDetection = true;
                    //DetPar.UseTracking = true;
                    //DetPar.LimitVolumeToPickable = false;
                    SPipettingPar aspLiquid2 = Workbench.GetAspVolumePar(800, vb);

                    DetPar.UseLiquidDetection = false;
                    DetPar.UseTracking = true;
                    DetPar.LimitVolumeToPickable = false;
                    aspLiquid2.ZMode = SPipettingPar.ModeConst.RelativeToLiquidPosition;

                    for (var k = 0; k < UsedTips.MaxTip(usedSource); k++)
                    {
                        //usedSource[k].Liquid.Volume = aspLiquid.VolArr[k];
                        usedSource[k].Liquid = new SLiquid("sample", aspLiquid2.VolArr[k], new SLiquidState());
                        aspLiquid2.ZOffset[k] = -2;
                    }

                    //触底吸950ul

                    PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid2, TransAir, Spit, MixPar, DetPar);

                    //DetPar.UseLiquidDetection = false;
                    //DetPar.UseTracking = false;
                    //DetPar.LimitVolumeToPickable = false;
                    //SPipettingPar aspLiquid3 = Workbench.GetAspVolumePar(150, vb);
                    //PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid3, TransAir, Spit, MixPar, DetPar);
                    //刷新界面
                    RefreshUiEvent(RdAction.Waste, GetNapId(usedSource), GetWasteCells(usedSource));
                }
                // }
                else
                {
                    DetPar.UseLiquidDetection = true;
                    DetPar.UseTracking = true;
                    DetPar.LimitVolumeToPickable = false;


                    // Aspirate
                    PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid, TransAir, Spit, MixPar, DetPar);

                    RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedSource), GetVolume(usedSource)); //吸液后刷新界面
                }

                #endregion

                ////如果是最后一次吸油，吸完之后直接打掉Tip头
                //if (Category != "lastWaste")
                //{

                #region 走折线

                //Z轴上升到安全距离
                //PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

                double tempy = Workbench.Layout.GetSlotByName("RD_CupRack1:RD_CupRack_CupSlot1").Position.Y - 2;
                double[] tempY = new double[3];
                switch (UsedTips.NoOfUsedTips)
                {
                    case 3:
                        tempY[0] = tempy - 17.6;
                        tempY[1] = tempy - 8.8;
                        tempY[2] = tempy;
                        break;
                    case 2:
                        tempY[0] = tempy - 17.6;
                        tempY[1] = tempy - 8.8;
                        tempY[2] = maxY[2];
                        break;
                    case 1:
                        tempY[0] = tempy - 17.6;
                        tempY[1] = maxY[1];
                        tempY[2] = maxY[2];
                        break;
                }

                if (Destination[0].Name == "RD_WashStation2:RD_WashStation_Cavity1")
                {
                    if (usedSource[0].ParentItem.CurrentSlot.Name == "RD_Mag1:RD_Mag_CupSlot4")
                    {
                        //TODO
                    }
                    else
                    {
                        //全都向右走
                        double x = (Source[(i - 1)*ADP_COUNT].FirstPos.X + Source[(i - 1)*ADP_COUNT].LastPos.X)/2 +
                                   17.0/2; //17=一个六联排的宽度

                        double y0 = (Source[(i - 1)*ADP_COUNT].FirstPos.Y + Source[(i - 1)*ADP_COUNT].LastPos.Y)/2;
                        double y1 = (Source[(i - 1)*ADP_COUNT + 1].FirstPos.Y + Source[(i - 1)*ADP_COUNT + 1].LastPos.Y)/
                                    2;
                        double y2 = (Source[(i - 1)*ADP_COUNT + 2].FirstPos.Y + Source[(i - 1)*ADP_COUNT + 2].LastPos.Y)/
                                    2;
                        double[] y = new double[] {y0, y1, y2};
                        
                        Robot.GetArm(0).NoTravelMove = true;

                        double z0 = Source[0].FirstPos.Z+3;
                        PipMethods.BlockMoveZ(UsedTips, z0, 0, 0);
                        PipMethods.MoveXY(x, y);

                        double z = Source[0].FirstPos.Z - CavityTravelHeight - 3;
                        PipMethods.BlockMoveZ(UsedTips, z, 0, 0);
                        PipMethods.MoveXY(x, maxY); //移动至六联排左侧Y方向最大值位置

                        IItem washStation2 = Workbench.Layout.GetItemByName("RD_WashStation2"); //洗站为WashStation2；
                        x = washStation2.Position.X;
                        PipMethods.MoveXY(x, maxY); //移动至洗站左侧Y方向最大值位置

                        //目标位置Y
                        //ICavity washStationCavity =
                        //    Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
                        //destinationY[0] = (washStationCavity.FirstPos.Y + washStationCavity.LastPos.Y)/2 - 10;
                        //destinationY[1] = destinationY[0] + 10;
                        //destinationY[2] = destinationY[0] + 20;

                        //PipMethods.MoveXY(x, destinationY);
                        Robot.GetArm(0).NoTravelMove = false;
                    }
                }
                else
                {
                    //样本左侧间隙
                    //double x = (usedSource[(i - 1) * ADP_COUNT].FirstPos.X + usedSource[(i - 1) * ADP_COUNT].LastPos.X) / 2 - 19.85 / 2;//19.85=一排样本架的宽度
                    double x = (usedSource[0].FirstPos.X + usedSource[0].LastPos.X)/2 - 19.85/2; //19.85=一排样本架的宽度
                    //移动至样本左侧间隙，Y方向最大值
                    PipMethods.MoveXY(x, maxY);

                    //移动至第四个样本架与第一个六联排载架之间，Y方向最大值
                    double tempX = Workbench.Layout.GetItemByName("RD_CupRack1").Position.X;
                    PipMethods.MoveXY(tempX, maxY);

                    if (Destination[0].ParentItem.CurrentSlot.Name == "RD_CupRack1:RD_CupRack_CupSlot1")
                    {
                        //目标六联排左侧间隙
                        destinationX = (Destination[(i - 1)* ADP_COUNT].FirstPos.X +
                                        Destination[(i - 1)* ADP_COUNT].LastPos.X)/2 - 8.5;
                    }
                    else
                    {
                        //移动至第四个样本架与第一个六联排载架之间，Y方向在第三个Tip盒和第一个六联排载架之间
                        PipMethods.MoveXY(tempX, tempY);

                        //目标六联排左侧间隙
                        destinationX = (Destination[(i - 1)* ADP_COUNT].FirstPos.X +
                                        Destination[(i - 1)* ADP_COUNT].LastPos.X)/2 - 8.5;
                        //8.5=六联排宽度的一半
                        //移动至目标六联排左侧，Y方向在第三个Tip盒和第一个六联排载架之间
                        PipMethods.MoveXY(destinationX, tempY);
                    }

                    switch (UsedTips.NoOfUsedTips)
                    {
                        case 3:
                            //目标位置Y
                            destinationY[0] = (Destination[(i - 1)* ADP_COUNT].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT].LastPos.Y)/2;
                            destinationY[1] = (Destination[(i - 1)* ADP_COUNT + 1].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT + 1].LastPos.Y)/2;
                            destinationY[2] = (Destination[(i - 1)* ADP_COUNT + 2].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT + 2].LastPos.Y)/2;
                            break;
                        case 2:
                            destinationY[0] = (Destination[(i - 1)* ADP_COUNT].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT].LastPos.Y)/2;
                            destinationY[1] = (Destination[(i - 1)* ADP_COUNT + 1].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT + 1].LastPos.Y)/2;
                            destinationY[2] = maxY[2];
                            break;
                        case 1:
                            destinationY[0] = (Destination[(i - 1)* ADP_COUNT].FirstPos.Y +
                                               Destination[(i - 1)* ADP_COUNT].LastPos.Y)/2;
                            destinationY[1] = maxY[1];
                            destinationY[2] = maxY[2];
                            break;
                    }
                    //移动至目标位置左侧
                    PipMethods.MoveXY(destinationX, destinationY);
                }

                #endregion

                #region // Dispense liquid

                // prepare dispense parameters
                SPipettingPar dispenseLiquid = Workbench.GetDispVolumePar(Volume, vb);

                //20170316 zhangjing
                //第一次打废液450
                if (Category == "firstWaste") //20170316
                {
                    dispenseLiquid = Workbench.GetDispVolumePar(350, vb);
                }
                //第二次打废液700
                if (Category == "secondWaste") //20170316
                {
                    dispenseLiquid = Workbench.GetDispVolumePar(500, vb);
                }
                //最后一次打废液600
                if (Category == "lastWaste") //20170316
                {
                    dispenseLiquid = Workbench.GetDispVolumePar(550, vb);
                }

                //20170310 zhangjing 
                //dispenseLiquid += TransAir;
                // on example we used fix detection/tracking parameters
                //DetPar = new SDetectionPar((int) SDetectionPar.ModeMask.UseTracking);
                //20170317
                DetPar.UseLiquidDetection = false;
                DetPar.UseTracking = false;
                DetPar.LimitVolumeToPickable = false;

                //本次加样需要加注的孔位
                SCavityCollection usedDestination = new SCavityCollection();
                for (var j = 0; j < Destination.Count; j++)
                {
                    if (j < i* ADP_COUNT && j >= (i - 1)* ADP_COUNT)
                    {
                        usedDestination.Add(Destination[j]);
                    }
                }

                // Dispense
                PipMethods.Dispense(UsedTips, usedDestination, dispenseLiquid, MixPar, DetPar, TransAir);
                //如果不是抽废液，打液后更新界面
                if (!Category.Contains("Waste"))
                {
                    RefreshUiEvent(RdAction.Dispense, GetNapId(usedDestination), GetDspCells(usedDestination));
                }

                #endregion

                //}

                #region // drop tips

                if (UsedDT.AnyTipUsed)
                {
                    // We have used disposable tip on the sequence
                    //SDropStation ds = Workbench.Robot.GetDevice(typeof(SDropStation)) as SDropStation;
                    SDropStation ds = Workbench.DropStation;
                    if (ds != null)
                    {
                        // valid drop station found 
                        SDropTipsPar DropTipsPar = new SDropTipsPar();
                        DropTipsPar.DisplayDropTipsError = true;

                        //#region 走折线

                        //if (Destination[0].Name == "RD_WashStation2:RD_WashStation_Cavity1")
                        //{
                        //    //ISlot slot = Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1");
                        //    //double z = slot.Position.Z + 42.5; //slot.Z+42.5=加热模块平面高度；
                        //    //Robot.GetArm(0).NoTravelMove = true;
                        //    //PipMethods.BlockMoveZ(UsedTips, z, BLOCK_MODE, ADP_MOVE_SPEED);
                        //    //destinationX =
                        //    //    Workbench.Layout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot1").Position.X - 14;
                        //    ////震荡模块和吸磁模块中间位置
                        //    //double y0 = (Destination[0].FirstPos.Y + Destination[0].LastPos.Y) / 2-9;
                        //    //double y1 = (Destination[0].FirstPos.Y + Destination[0].LastPos.Y)/2 ;
                        //    //double y2 = (Destination[0].FirstPos.Y + Destination[0].LastPos.Y)/2 + 9;
                        //    //destinationY = new double[] {y0,y1, y2};

                        //    //PipMethods.MoveXY(destinationX, destinationY);
                        //    //PipMethods.MoveXY(destinationX, maxY);

                        //    Robot.GetArm(0).NoTravelMove = false;
                        //    //Z轴上升到安全距离
                        //    PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

                        //    ICavity washCavity =
                        //        Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
                        //    double x = (washCavity.FirstPos.X + washCavity.LastPos.X) / 2;
                        //    PipMethods.MoveXY(x, maxY);//移动至洗站位置Y方向最大值
                        //}
                        //else
                        //{
                        //    //Z轴上升到安全距离
                        //    PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
                        //    PipMethods.MoveXY(destinationX, maxY);
                        //}

                        //PipMethods.MoveXY(10, maxY); //试剂架左侧空隙位置

                        //#endregion

                        #region 走折线

                        //Z轴上升到安全距离
                        PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

                        double x = (Destination[0].FirstPos.X + Destination[0].LastPos.X)/2 + 8.5; //8.5=六联排宽度的一半；
                        double[] minY = {0, 9, 18};

                        double[] desY = new double[3];
                        desY[0] = Destination[0].ParentItem.CurrentSlot.Position.Y + 101.32 + 50;
                        //101.32=cupslot长度，50=cupslot向外延伸距离
                        desY[1] = desY[0] + 9;
                        desY[2] = desY[1] + 9;
                        double desX = Workbench.Layout.GetLayoutDeviceByName("DropStation1").Position.X;

                        if (Destination[0].ParentItem.CurrentSlot.Name.Contains("RD_Reader1"))
                        {
                            PipMethods.MoveXY(x, minY); //移动至最内侧
                            PipMethods.MoveXY(desX, minY);
                        }
                        else if (Destination[0].Name == "RD_WashStation2:RD_WashStation_Cavity1")
                        {
                            //TODO
                        }
                        else
                        {
                            double tempX = Workbench.Layout.GetItemByName("RD_Heating1").Position.X;
                            double tempy2 = Workbench.Layout.GetItemByName("RD_Heating1").Position.Y;
                            double[] tempY2 = new double[3];
                            switch (UsedTips.NoOfUsedTips)
                            {
                                case 3:
                                    tempY2[0] = tempy2 - 17.6;
                                    tempY2[1] = tempy2 - 8.8;
                                    tempY2[2] = tempy2;
                                    break;
                                case 2:
                                    tempY2[0] = tempy2 - 17.6;
                                    tempY2[1] = tempy2 - 8.8;
                                    tempY2[2] = maxY[2];
                                    break;
                                case 1:
                                    tempY2[0] = tempy2 - 17.6;
                                    tempY2[1] = maxY[1];
                                    tempY2[2] = maxY[2];
                                    break;
                            }

                            PipMethods.MoveXY(x, tempY); //X移动至六联排右侧，Y移动至六联排载架与Tip头载架之间
                            PipMethods.MoveXY(tempX, tempY); //X移动至加热模块左侧，Y移动至六联排载架与Tip头载架之间
                            PipMethods.MoveXY(tempX, tempY2); //X移动至加热模块左侧，Y移动至Reader外侧；
                            PipMethods.MoveXY(desX, tempY2); //X移动至DropStation左侧，Y移动至Reader外侧；
                        }

                        #endregion

                        PipMethods.DropTips(UsedDT, DropTipsPar, ds);
                    }
                    else
                    {
                        //System.Windows.MessageBox.Show("No valid drop station on DT robot. Tips are not dropped!");
                        string message = "No valid drop station on DT robot. Tips are not dropped!";
                        ReportMessageEvent(PopupType.ShowMessage, message, null);
                    }
                }
                GC.Collect();

                #endregion
            }

            //};
            //if (Sequence != null)
            //{
            //    RunSequence.ExecuteSequence(Sequence);
            //}
        }

        #endregion 吸多加多

        private const double MaxX = 1018;

        //private void Transfer(SCavityCollection sccSource, SCavityCollection sccDes, double volume, double tipType = 1000)
        //{
        //    if (sccSource == null)
        //    {

        //    }
        //    if (sccSource.Count > 3)
        //    {
        //        MessageBox.Show("Source is not correct!");
        //        return;
        //    }
        //    if (sccSource.Count == 1)
        //    {
        //        int addTimes = 0;
        //        int dispTimes = 0;
        //        double aspVolume = 0;

        //        if (sccSource[0].Name.Contains("RD_ParaffinBox") || sccSource[0].Name.Contains("RD_ReagentBox"))
        //        {
        //            for (var i = 0; i < sccDes.Count; i++)
        //            {

        //            }
        //        }
        //    }
        //    else if(sccSource.Count>1&&sccSource.Count<=3)
        //    {

        //    }
        //    else
        //    {
        //        MessageBox.Show("Source is not correct!");
        //    }
        //}

        #region 吸一加多

        /// <summary>
        /// 吸一加多
        /// </summary>
        /// <param name="Source">吸液孔</param>
        /// <param name="Destination">加液孔集合</param>
        /// <param name="Volume">每孔加液体积</param>
        /// <param name="tipV">Tip头体积</param>
        /// <param name="Category">液体参数类型</param>
        public void Transfer(ICavity Source, SCavityCollection Destination, double Volume, double tipV = 1000,
            string Category = "Default")
        {
            //加样次数
            int addTimes = 0;
            if (Source.Name.Contains("RD_ParaffinBox") || Source.Name.Contains("RD_ReagentBox"))
            {
                addTimes = Destination.Count/ADP_COUNT;
                //如果有余数，加样次数加1
                if (Destination.Count%ADP_COUNT != 0)
                {
                    addTimes += 1;
                }
            }
            else
            {
                addTimes = Destination.Count;
            }

            #region 计算吸液次数及加液次数

            //根据加样量计算最大 吸一加几
            int maxDispenseTimes = GetMaxAspirateTimes(Volume, tipV);

            //一个Tip可以加几个孔
            int tipCells = 0;
            if (tipV%Volume > 0)
            {
                tipCells = Convert.ToInt32(tipV/Volume) + 1;
            }
            else
            {
                tipCells = Convert.ToInt32(tipV/Volume);
            }

            //吸一次加几次
            //int dispenseTimes = Convert.ToInt32(((Destination.Count * Volume) / MAX_TIP_VOLUME) / ADP_COUNT);
            //共需要吸多少液体
            double totalV = Destination.Count*Volume;
            //使用的ADP个数，即使用的Tip头个数。
            //因为吸同一种液体，可重复使用Tip头。
            //目前的情况，吸一加多都是使用一个ADP
            int usedADPCount = 1;
            if (Source.Name.Contains("RD_ParaffinBox") || Source.Name.Contains("RD_ReagentBox"))
            {
                if (Source.Name == "RD_ReagentBox4:RD_ReagentBox_Cavity1")
                {
                }
                else
                {
                    usedADPCount = ADP_COUNT;
                }
            }

            //吸液次数
            int aspirateTimes = Destination.Count/(maxDispenseTimes*ADP_COUNT);
            if (Destination.Count%(maxDispenseTimes*ADP_COUNT) > 0)
            {
                aspirateTimes += 1;
            }
            //吸一次打几次
            int dispenseTimes = Destination.Count/usedADPCount;
            //一次吸多少
            double dispenseV = dispenseTimes*Volume;
            while (dispenseV >= tipV)
            {
                dispenseTimes -= 1;
                dispenseV -= Volume;
            }

            #region notused

            ////if (totalV%tipV > 0)
            ////{
            ////    tipCount = Convert.ToInt32(totalV/tipV) + 1;
            ////}
            ////else
            ////{
            ////    tipCount = Convert.ToInt32(totalV/tipV);
            ////}

            ////如果 吸一加多次数 超过 需要加样的孔数，则用1个Tip头。（由于只有2个ADP，其他情况不考虑）
            //if (maxDispenseTimes >= Destination.Count)
            //{
            //    usedADPCount = 1;
            //}
            //else
            //{
            //    usedADPCount = ADP_COUNT;
            //}

            //一次加样能加几个孔
            //int cells = tipCells * usedADPCount;

            ////吸一次加几次
            //int dispenseTimes;
            //if (maxDispenseTimes > Destination.Count)
            //{
            //    dispenseTimes = Destination.Count;
            //}
            //else
            //{
            //    dispenseTimes = maxDispenseTimes;
            //}

            ////共需要吸几次
            //int + = 1;
            ////吸几次
            //if (totalV % dispenseV > 0)
            //{
            //    aspirateTimes = Convert.ToInt32(totalV / dispenseV) + 1;
            //}
            //else
            //{
            //    aspirateTimes = Convert.ToInt32(totalV / dispenseV);
            //}
            //吸一次加几次

            //如果有余数，加样次数加1
            //if (Convert.ToInt32(((Destination.Count * Volume) %(MAX_TIP_VOLUME) * ADP_COUNT)) > 0)
            //{
            //    dispenseTimes += 1;
            //}
            //int dispenseTimes = Convert.ToInt32(MAX_TIP_VOLUME / Volume);
            //if (MAX_TIP_VOLUME%Volume > 0)
            //{
            //    dispenseTimes += 1;
            //}
            //if (dispenseTimes%ADP_COUNT > 0)
            //{
            //    dispenseTimes = dispenseTimes/ADP_COUNT + 1;
            //}
            //else
            //{
            //    dispenseTimes = dispenseTimes/ADP_COUNT;
            //}

            //if (dispenseTimes > maxDispenseTimes)
            //{
            //    aspirateTimes = dispenseTimes / maxDispenseTimes;
            //    if (dispenseTimes % maxDispenseTimes > 0)
            //    {
            //        aspirateTimes += 1;
            //    }
            //    dispenseTimes = maxDispenseTimes;
            //}

            #endregion

            #endregion

            double aspirateV = 0; //实际吸液量
            if (tipV == 1000) //如果是1000ul的Tip头，多吸50ul
            {
                aspirateV = dispenseV + 50;
                if (Category == "Oil") //如果是油，多吸150
                {
                    aspirateV += 150;
                }
            }
            else if (tipV == 300) //如果是300ul的Tip头，多吸10ul
            {
                aspirateV = dispenseV + 10;
                //if (Source.Name.Contains("RD_EzBottole")) //如果是酶，再多吸20ul
                //{
                //    aspirateV = dispenseV + 20;
                //}
            }

            SVolumeBand vb = new SVolumeBand();
            switch (Category)
            {
                case "Mag6":
                    vb = Workbench.GetVolumeBand("Default", "Mag6", Volume, tipV);
                    break;
                case "Washing":
                    vb = Workbench.GetVolumeBand("Default", "Washing", Volume, tipV);
                    break;
                case "AMP":
                    vb = Workbench.GetVolumeBand("Default", "AMP", Volume, tipV);
                    break;
                case "firstWaste":
                case "secondWaste":
                case "lastWaste":
                    vb = Workbench.GetVolumeBand("Default", "Waste", Volume, tipV);
                    break;
                case "Oil":
                    vb = Workbench.GetVolumeBand("Default", "Oil", Volume, tipV);
                    break;
                case "Enzyme":
                    vb = Workbench.GetVolumeBand("Default", "Enzyme", Volume, tipV);
                    break;
                case "Urine":
                    vb = Workbench.GetVolumeBand("Default", "Urine", Volume, tipV);
                    break;
                case "Default":
                default:
                    vb = Workbench.GetVolumeBand("Default", "Default", Volume, tipV);
                    break;
            }

            //SVolumeBand vb = Workbench.GetVolumeBand("Default", "Default", dispenseV,tipV);
            SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");

            //ExecutionMethod Sequence = null;
            //Sequence = delegate(ProcessWorkbench WorkBench)
            //{
            SPipettingMethods PipMethods = Workbench.PipMethods;
            //var CycleCount = (Count > PipMethods.Arm.YZDevices.Count) ? PipMethods.Arm.YZDevices.Count : Count;

            // make sure the robot is connected
            //Workbench.StartSequence(PipMethods.AllTips); // start sequence using all tips
            Workbench.StartSequence(PipMethods.AllTips);
            STipMap UsedTips = new STipMap(0, usedADPCount - 1) - ~PipMethods.AllTips;
            //STipMap UsedTips = Workbench.PipMethods.AllTips.SingleTip(0) + Workbench.PipMethods.AllTips.SingleTip(1);//两通道
            STipMap UsedDT = Workbench.DTAdapter.Combine(UsedTips); // used DT adapters
            //STipMap UsedTips = new STipMap(0, usedADPCount - 1);
            //switch (Convert.ToInt32(tipV))
            //{
            //    case 300:
            //        STipMap tm=new STipMap();

            //        break;
            //    case 1000:
            //    default:

            //        break;
            //}

            //STipMap UsedDT = Workbench.DTAdapter.Combine(UsedTips);

            //取Tip头前，洗针
            Workbench.FlushADP(400);
            Workbench.PipMethods.BlockMoveZ(PipMethods.AllTips,SAFE_Z,0,0);
            #region // Get disposable tip (optional)

            if (UsedDT.AnyTipUsed)
            {
                // We have used disposable tip on the sequence
                // Important: Please note that Tip check is not working yet!
                SGetTipsPar gtp = new SGetTipsPar
                {
                    DisplayHasTipsError = true,
                    DisplayBadTipsError = true
                };
                //STipTypeCollection ttc = new STipTypeCollection();

                //2017-03-09 zhangjing
                //Tip头用完后，提示用户填充Tip头；
                if (tipV == 300)
                {
                    ITipType tt = new STipType("Rainin Universal 300");
                    IItem tipRack = Workbench.Layout.GetItemByName("DTRack1");
                    int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                    if (tipNum < UsedDT.NoOfUsedTips)
                    {
                        //System.Windows.MessageBox.Show("300ul Tip头不足，请补充后点击确定按钮！");
                        string message = "300ul Tip头不足，请补充后点击确定按钮！";
                        ReportMessageEvent(PopupType.ShowMessage, message, null);
                        FillTips(1);
                    }
                    PipMethods.GetTips(UsedDT, tt, gtp);
                    //保存Tip头信息
                    Workbench.SaveTipStates("Rendu.TipStates.xml");
                    //界面更新Tip头状态
                    RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                }
                else if (tipV == 1000)
                {
                    ITipType tt = new STipType("Rainin Universal 1000");
                    IItem tipRack = Workbench.Layout.GetItemByName("DTRack9");
                    int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
                    if (tipNum < UsedDT.NoOfUsedTips)
                    {
                        //System.Windows.MessageBox.Show("1000ul Tip头不足，请补充后点击确定按钮！");
                        string message = "1000ul Tip头不足，请补充后点击确定按钮！";
                        ReportMessageEvent(PopupType.ShowMessage, message, null);
                        Fill1000Tips();
                    }
                    PipMethods.GetTips(UsedDT, tt, gtp);
                    //保存Tip头信息
                    Workbench.SaveTipStates("Rendu.TipStates.xml");
                    //界面更新Tip头状态
                    RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
                }
            }

            #endregion

            //Y方向最外侧
            double[] maxY = {371.4, 380.2, 389}; //389=ADP Y方向最大值；最大值取388，余量1；
            double destinationX = 10;
            double[] destinationY = new double[3];

            //for (var m = 0; m < aspirateTimes; m++)
            //{

            #region // Aspirate liquid

            // prepare aspirate parameters
            SPipettingPar SysAir = Workbench.GetAirPar(vb.SystemAirgap);
            // first air gap to separate aspirated liquid 
            // from system liquid 
            //SPipettingPar SepAir = GetAirPar(vb.SeparationAirgap);    // not used, as we only use one liquid
            //SPipettingPar aspLiquid = Workbench.GetAspVolumePar(aspirateV + vb.SpitBack + vb.WasteVolume, vb);
            SPipettingPar aspLiquid = Workbench.GetAspVolumePar(aspirateV, vb);

            //SPipettingPar aspirateLiquid = WorkBench.GetAspVolumePar(dispenseV, vb);
            //SPipettingPar dispenseLiquid = WorkBench.GetAspVolumePar(Volume, vb);
            // liquid volume (including speeds and ramps)
            SPipettingPar TransAir = Workbench.GetAirPar(vb.TransportationAirgap);
            // transportation air gap
            // Take care: To keep it more simple we are using the volume band of the 
            // aspiration volume. It would be more correct to use the volume band of the spit back volume. 
            SPipettingPar Spit = Workbench.GetAspVolumePar(vb.SpitBack, vb);
            // Get spit back parameters. 
            // Detection parameter
            SDetectionPar DetPar = dc.DetectionParameters;

            //debug 
            //DetPar.DisplayErrors = false;
            DetPar.DisplayErrors = true;

            //DetPar.UseLiquidDetection = true;  // it is also possible to change the detection mode afterwards
            SMixPar MixPar = new SMixPar();

            //本次加样需要吸取的孔位
            SCavityCollection usedSource = new SCavityCollection();
            usedSource.Add(Source);
            //如果是石蜡或洗涤液，使用3个ADP去吸液
            if ((Source.Name.Contains("RD_ParaffinBox") || Source.Name.Contains("RD_ReagentBox")))
            {
                if (Source.Name == "RD_ReagentBox4:RD_ReagentBox_Cavity1")
                {
                    //TODO
                }
                else
                {
                    usedSource.Add(Source);
                    usedSource.Add(Source);
                }
            }

            ////add by zj 20170106
            //if (Category == "Mag6") //如果是磁珠，吸液之前先固定位置抽打三次
            //{
            //    DetPar.UseLiquidDetection = false;
            //    DetPar.UseTracking = false;
            //    DetPar.LimitVolumeToPickable = false;
            //    for (var t = 1; t <= MAG_MIX_TIMES; t++)
            //    {
            //        SPipettingPar aLiquid = Workbench.GetAspVolumePar(800, vb);
            //        SPipettingPar dLiquid = Workbench.GetDispVolumePar(800, vb);
            //        PipMethods.Aspirate(UsedTips, usedSource, SysAir, aLiquid, TransAir, Spit, MixPar, DetPar);
            //        PipMethods.Dispense(UsedTips, usedSource, dLiquid, MixPar, DetPar, TransAir);
            //    }
            //}

            //2017-03-09 zhangjing
            //取完Tip头后，如果是去震荡模块或者吸磁模块，需避让读数模块
            if (usedSource[0].ParentItem.CurrentSlot.Name.Contains("RD_ShakerRack") ||
                usedSource[0].ParentItem.CurrentSlot.Name.Contains("RD_Mag"))
            {
                //Z轴上升到安全距离
                PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
                IItem paraffinBox = Workbench.Layout.GetItemByName("RD_ParaffinBox1");
                double x = paraffinBox.Position.X + 19.4;
                double y0 = paraffinBox.Position.Y - 50;
                double y1 = paraffinBox.Position.Y - 40;
                double y2 = paraffinBox.Position.Y - 30;
                double[] y = new double[] {y0, y1, y2};
                PipMethods.MoveXY(x, y);
            }

            double tempy = Workbench.Layout.GetSlotByName("RD_CupRack1:RD_CupRack_CupSlot1").Position.Y - 2;
            double[] tempY = new double[3];
            switch (UsedTips.NoOfUsedTips)
            {
                case 3:
                    tempY[0] = tempy - 17.6;
                    tempY[1] = tempy - 8.8;
                    tempY[2] = tempy;
                    break;
                case 2:
                    tempY[0] = tempy - 17.6;
                    tempY[1] = tempy - 8.8;
                    tempY[2] = maxY[2];
                    break;
                case 1:
                    tempY[0] = tempy - 17.6;
                    tempY[1] = maxY[1];
                    tempY[2] = maxY[2];
                    break;
            }

            double tempX = Workbench.Layout.GetItemByName("RD_Heating1").Position.X;
            double tempy2 = Workbench.Layout.GetItemByName("RD_Heating1").Position.Y;
            double[] tempY2 = new double[3];
            switch (UsedTips.NoOfUsedTips)
            {
                case 3:
                    tempY2[0] = tempy2 - 17.6;
                    tempY2[1] = tempy2 - 8.8;
                    tempY2[2] = tempy2;
                    break;
                case 2:
                    tempY2[0] = tempy2 - 17.6;
                    tempY2[1] = tempy2 - 8.8;
                    tempY2[2] = maxY[2];
                    break;
                case 1:
                    tempY2[0] = tempy2 - 17.6;
                    tempY2[1] = maxY[1];
                    tempY2[2] = maxY[2];
                    break;
            }

            for (var i = 1; i <= aspirateTimes; i++)
            {
                if (Category == "Oil")
                {
                    DetPar.UseLiquidDetection = false;
                    DetPar.UseTracking = false;
                    DetPar.LimitVolumeToPickable = false;
                }
                else
                {
                    DetPar.UseLiquidDetection = true;
                    DetPar.UseTracking = true;
                    DetPar.LimitVolumeToPickable = true;
                }

                // Aspirate
                PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid, TransAir, Spit, MixPar, DetPar);
                //刷新界面
                RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedSource), GetVolume(usedSource));

                if (Category == "Oil") //如果是油，吸完后回吐100
                {
                    SPipettingPar disLiquid = Workbench.GetDispVolumePar(100, vb);
                    //DetPar = new SDetectionPar((int) SDetectionPar.ModeMask.UseTracking);
                    PipMethods.Dispense(UsedTips, usedSource, disLiquid, MixPar, DetPar, TransAir);
                    //Thread.Sleep(5000);
                    //刷新界面
                    RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedSource), GetVolume(usedSource));
                }

                #endregion

                for (var m = 1; m <= dispenseTimes; m++)
                {
                    #region // Dispense liquid

                    // prepare dispense parameters

                    SPipettingPar dispenseLiquid = Workbench.GetDispVolumePar(Volume, vb);
                    dispenseLiquid += TransAir;
                    // on example we used fix detection/tracking parameters
                    //20161227 zhangjing
                    //加洗涤液时，需要tracking
                    if (Source.Name.Contains("RD_ReagentBox3") || Source.Name.Contains("RD_ReagentBox4")) //洗涤液盒子
                    {
                        DetPar.UseLiquidDetection = false;
                        DetPar.UseTracking = true;
                        DetPar.LimitVolumeToPickable = true;
                    }
                    else
                    {
                        DetPar.UseLiquidDetection = false;
                        DetPar.UseTracking = false;
                        DetPar.LimitVolumeToPickable = false;
                    }

                    SCavityCollection usedDestination = new SCavityCollection();
                    for (var n = 0; n < usedADPCount; n++)
                    {
                        usedDestination.Add(Destination[(((i - 1)*dispenseTimes) + m - 1)*usedADPCount + n]);
                    }

                    //如果是吸完液体后 第一次去加样，则走折线
                    if (m == 1)
                    {
                        #region 走折线

                        //Z轴上升到安全距离
                        PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
                        //第一排小瓶
                        if (usedSource[0].Name.Contains("RD_EzBottole"))
                        {
                            //读数模块第一排六连杯
                            if (Destination[0].ParentItem.CurrentSlot !=
                                Workbench.Layout.GetSlotByName("RD_Reader1:RD_Reader_CupSlot1"))
                            {
                                double x = (usedSource[0].FirstPos.X + usedSource[0].LastPos.X)/2 + 10.9/2;
                                //10.9=RD_Bottole直径
                                double y = Workbench.Layout.GetSlotByName("RD_Reader1:RD_Reader_CupSlot1").Position.Y +
                                           101.32 + 20; //Slot位置+Slot长度+20（与Slot最外端的距离）
                                double[] readerMaxY = new double[3];
                                readerMaxY[0] = y;
                                readerMaxY[1] = y + 8.8;
                                readerMaxY[2] = y + 17.6;
                                destinationX = (Destination[0].FirstPos.X + Destination[0].LastPos.X)/2 - 17.0/2;
                                destinationY[0] = (Destination[0].FirstPos.Y + Destination[0].LastPos.Y)/2;
                                destinationY[1] = readerMaxY[1];
                                destinationY[1] = readerMaxY[2];
                                PipMethods.MoveXY(x, readerMaxY);
                                PipMethods.MoveXY(destinationX, readerMaxY);
                                PipMethods.MoveXY(destinationX, destinationY);
                            }
                        }
                        else if (usedSource[0].Name.Contains("RD_ParaffinBox"))
                        {
                            if (Destination[0].ParentItem.CurrentSlot.Name == "RD_Heating1:RD_Heating_CupSlot4")
                            {
                                //TODO
                            }
                            else
                            {
                                double x = (usedSource[0].FirstPos.X + usedSource[0].LastPos.X)/2 - 8.5; //8.5=六联排宽度的一半
                                double[] y = new double[3];
                                switch (UsedTips.NoOfUsedTips)
                                {
                                    case 3:
                                        y[0] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            19.8;
                                        y[1] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            11;
                                        y[2] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            2;
                                        break;
                                    case 2:
                                        y[0] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            19.8;
                                        y[1] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            11;
                                        y[2] = maxY[2];
                                        break;
                                    case 1:
                                        y[0] =
                                            Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1").Position.Y -
                                            19.8;
                                        y[1] = maxY[1];
                                        y[2] = maxY[2];
                                        break;
                                }
                                PipMethods.MoveXY(x, y);
                                destinationX = (usedDestination[0].FirstPos.X + usedDestination[0].LastPos.X)/2 + 8.5;
                                PipMethods.MoveXY(destinationX, y);
                            }
                        }
                        else
                        {
                            //样本左侧间隙
                            double x = (usedSource[0].FirstPos.X + usedSource[0].LastPos.X)/2 - 19.85/2;
                                //19.85=一排样本架的宽度

                            //目标六联排左侧间隙
                            destinationX = (usedDestination[0].FirstPos.X + usedDestination[0].LastPos.X)/2 - 8.5;
                                //8.5=六联排宽度的一半

                            switch (UsedTips.NoOfUsedTips)
                            {
                                case 3:
                                    destinationY[0] = (usedDestination[0].FirstPos.Y + usedDestination[0].LastPos.Y)/2;
                                    destinationY[1] = (usedDestination[1].FirstPos.Y + usedDestination[1].LastPos.Y)/2;
                                    destinationY[2] = (usedDestination[2].FirstPos.Y + usedDestination[2].LastPos.Y)/2;
                                    break;
                                case 2:
                                    destinationY[0] = (usedDestination[0].FirstPos.Y + usedDestination[0].LastPos.Y)/2;
                                    destinationY[1] = (usedDestination[1].FirstPos.Y + usedDestination[1].LastPos.Y)/2;
                                    destinationY[2] = maxY[2];
                                    break;
                                case 1:
                                    destinationY[0] = (usedDestination[0].FirstPos.Y + usedDestination[0].LastPos.Y)/2;
                                    destinationY[1] = maxY[1];
                                    destinationY[2] = maxY[2];
                                    break;
                            }
                            //移动至样本左侧间隙，Y方向最大值
                            PipMethods.MoveXY(x, maxY);

                            //移动至第四个样本架与第一个六联排载架之间，Y方向最大值
                            double tempx = Workbench.Layout.GetItemByName("RD_CupRack1").Position.X;
                            PipMethods.MoveXY(tempx, maxY);

                            if (Destination[0].ParentItem.CurrentSlot.Name == "RD_CupRack1:RD_CupRack_CupSlot1")
                            {
                                //TODO
                            }
                            else
                            {
                                //移动至第四个样本架与第一个六联排载架之间，Y方向在第三个Tip盒和第一个六联排载架之间
                                PipMethods.MoveXY(tempx, tempY);

                                //目标六联排左侧间隙
                                destinationX = (Destination[(i - 1)*ADP_COUNT].FirstPos.X +
                                                Destination[(i - 1)*ADP_COUNT].LastPos.X)/2 - 8.5;
                                //8.5=六联排宽度的一半
                                //移动至目标六联排左侧，Y方向在第三个Tip盒和第一个六联排载架之间
                                PipMethods.MoveXY(destinationX, tempY);

                                //移动至目标六联排左侧
                                PipMethods.MoveXY(destinationX, tempY);
                            }
                            //移动至目标位置左侧
                            PipMethods.MoveXY(destinationX, destinationY);
                        }

                        #endregion
                    }

                    // Dispense
                    PipMethods.Dispense(UsedTips, usedDestination, dispenseLiquid, MixPar, DetPar, TransAir);
                    //刷新界面
                    RefreshUiEvent(RdAction.Dispense, GetNapId(usedDestination), GetDspCells(usedSource));
                    #endregion
                }

                //如果吸液次数大于1
                if (aspirateTimes > 1)
                {
                    //并且不是最后一次吸液
                    if (i < aspirateTimes)
                    {
                        #region 走折线

                        //Z轴上升到安全距离
                        PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
                        //移动至目标位置左侧
                        PipMethods.MoveXY(destinationX, destinationY);
                        //移动至Y方向最大处
                        PipMethods.MoveXY(destinationX, maxY);
                        //样本左侧间隙
                        double x = (usedSource[0].FirstPos.X + usedSource[0].LastPos.X)/2 - 19.85/2;
                        PipMethods.MoveXY(x, maxY);
                        double[] y = new double[3];
                        y[0] = (usedSource[0].FirstPos.Y + usedSource[0].LastPos.Y)/2;
                        //如果有2个source
                        if (usedSource.Count == 2)
                        {
                            //如果2个ADP吸同一个孔
                            if (usedSource[0].Name == usedSource[1].Name)
                            {
                                y[1] = y[0] + 8.8;
                            }
                            else
                            {
                                y[1] = (usedSource[1].FirstPos.Y + usedSource[1].LastPos.Y)/2;
                            }
                        }
                        //如果有3个source
                        if (usedSource.Count == 3)
                        {
                            //如果3个ADP吸同一个孔
                            if (usedSource[0].Name == usedSource[1].Name)
                            {
                                y[1] = y[0] + 8.8;
                                y[2] = y[0] + 17.6;
                            }
                            else
                            {
                                y[1] = (usedSource[1].FirstPos.Y + usedSource[1].LastPos.Y)/2;
                                y[2] = (usedSource[2].FirstPos.Y + usedSource[2].LastPos.Y)/2;
                            }
                        }
                        else
                        {
                            y[1] = maxY[1];
                            y[2] = maxY[2];
                        }
                        PipMethods.MoveXY(x, y);

                        #endregion 走折线
                    }
                }
            }
            //}

            #region // drop tips

            if (UsedDT.AnyTipUsed)
            {
                // We have used disposable tip on the sequence
                //SDropStation ds = Workbench.Robot.GetDevice(typeof(SDropStation)) as SDropStation;
                SDropStation ds = Workbench.DropStation;
                if (ds != null)
                {
                    // valid drop station found 
                    SDropTipsPar DropTipsPar = new SDropTipsPar();
                    DropTipsPar.DisplayDropTipsError = true;

                    #region 走折线

                    //Z轴上升到安全距离
                    PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

                    double x = (Destination[0].FirstPos.X + Destination[0].LastPos.X)/2 + 8.5; //8.5=六联排宽度的一半；
                    double[] minY = {0, 8.8, 17.6};

                    double desX = Workbench.Layout.GetLayoutDeviceByName("DropStation1").Position.X;

                    if (Destination[0].ParentItem.CurrentSlot.Name.Contains("RD_Reader1"))
                    {
                        PipMethods.MoveXY(x, minY); //移动至最内侧
                        PipMethods.MoveXY(desX, minY);
                    }
                    else if (Destination[0].ParentItem.CurrentSlot.Name.Contains("RD_Heating1"))
                    {
                        PipMethods.MoveXY(tempX, tempY2); //X移动至加热模块左侧，Y移动至Reader外侧；
                        PipMethods.MoveXY(desX, tempY2); //X移动至DropStation左侧，Y移动至Reader外侧；
                    }
                    else
                    {
                        switch (UsedTips.NoOfUsedTips)
                        {
                            case 3:
                                tempY2[0] = tempy2 - 17.6;
                                tempY2[1] = tempy2 - 8.8;
                                tempY2[2] = tempy2;
                                break;
                            case 2:
                                tempY2[0] = tempy2 - 17.6;
                                tempY2[1] = tempy2 - 8.8;
                                tempY2[2] = maxY[2];
                                break;
                            case 1:
                                tempY2[0] = tempy2 - 17.6;
                                tempY2[1] = maxY[1];
                                tempY2[2] = maxY[2];
                                break;
                        }

                        PipMethods.MoveXY(x, tempY); //X移动至六联排右侧，Y移动至六联排载架与Tip头载架之间
                        PipMethods.MoveXY(tempX, tempY); //X移动至加热模块左侧，Y移动至六联排载架与Tip头载架之间
                        PipMethods.MoveXY(tempX, tempY2); //X移动至加热模块左侧，Y移动至Reader外侧；
                        PipMethods.MoveXY(desX, tempY2); //X移动至DropStation左侧，Y移动至Reader外侧；
                    }

                    #endregion

                    PipMethods.DropTips(UsedDT, DropTipsPar, ds);

                }
                else
                {
                    //System.Windows.MessageBox.Show("No valid drop station on DT robot. Tips are not dropped!");
                    string message = "No valid drop station on DT robot. Tips are not dropped!";
                    ReportMessageEvent(PopupType.ShowMessage, message, null);
                }
            }

            #endregion

            // GC.Collect();
            //};
            //if (Sequence != null)
            //{
            //    RunSequence.ExecuteSequence(Sequence);
            //}
        }

        #endregion 吸一加多

        /// <summary>
        /// 计算最大吸一加几
        /// </summary>
        /// <param name="v">吸液量</param>
        /// <param name="tipV">Tip头容积</param>
        /// <returns>最大吸一加几</returns>
        private int GetMaxAspirateTimes(double v, double tipV)
        {
            int max = 6;

            if (v > tipV/2) max = 1;
            else if (v > tipV/3 && v < tipV/2) max = 2;
            else if (v > tipV/4 && v < tipV/3) max = 3;
            else if (v > tipV/5 && v < tipV/4) max = 4;
            else if (v > tipV/6 && v < tipV/5) max = 5;
            else if (v > 0 && v < tipV/6) max = 6;

            return max;
        }

        #region public RunSequenceForm RunSequence // Run sequence form (for execution)

        /// <summary>Run sequence form (for execution) field</summary>
        private RunSequenceHelper _RunSequence;

        /// <summary>Run sequence form (for execution) property</summary>
        [Browsable(false)]
        public RunSequenceHelper RunSequence
        {
            get
            {
                if (!(_RunSequence is RunSequenceHelper))
                {
                    _RunSequence = new RunSequenceHelper(Workbench, null);
                }
                return _RunSequence;
            }
        }

        #endregion

        #region Init

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            try
            {
                ExecutionMethod Sequence = null;
                Sequence = delegate(ProcessWorkbench WorkBench)
                {
                    // make sure the robot is connected
                    Workbench.StartSequence(null);
                    // 
                    SLogManager.Log("Init Robot (" + Workbench.Robot.RobotError + ")", SLogManager.CategoryInfo);
                    Workbench.Robot.Init(SDeviceInitOptions.InitAllLevelsOfAllGroups);
                };
                if (Sequence is ExecutionMethod) RunSequence.ExecuteSequence(Sequence);
            }
            catch (Exception eee)
            {
                ProcessWorkbench.ReportException(eee);
            }
        }

        #endregion

        private ISlot shakerSlot1;
        private ISlot shakerSlot2;
        private ISlot shakerSlot3;
        private ISlot heatingSlot1;
        private ISlot heatingSlot2;
        private ISlot heatingSlot3;
        private ISlot magSlot1;
        private ISlot magSlot2;
        private ISlot magSlot3;
        private ISlot magSlot4;




        #region 流程动作

        /// <summary>
        /// 搬运至震荡模块
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public ISlot MoveToShaker(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            ISlot destination = Workbench.Layout.GetSlotByName("RD_ShakerRack1:RD_ShakerRack_CupSlot1");


            if (shakerSlot1.CurrentItem == null)
            {
                destination = shakerSlot1;
            }
            else if (shakerSlot2.CurrentItem == null)
            {
                destination = shakerSlot2;
            }
            else if (shakerSlot3.CurrentItem == null)
            {
                destination = shakerSlot3;
            }
            else
            {
                //System.Windows.MessageBox.Show("Shaker Slot is not available, Please wait!");
                string message = "Shaker Slot is not available, Please wait!";
                ReportMessageEvent(PopupType.ShowMessage, message, null);
            }
            int from = GetSlotPos(source.CurrentSlot);
            int to = GetSlotPos(destination);
            Workbench.MoveItem(source, destination);
            RefreshUiEvent(RdAction.MoveItem, from, to);
            return destination;
        }

        /// <summary>
        /// 搬运至吸磁位置
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToMag(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            ISlot destination = Workbench.Layout.GetSlotByName("RD_Mag1:RD_Mag_CupSlot1");
            if (magSlot1.CurrentItem == null)
            {
                destination = magSlot1;
            }
            else if (magSlot2.CurrentItem == null)
            {
                destination = magSlot2;
            }
            else if (magSlot3.CurrentItem == null)
            {
                destination = magSlot3;
            }
            else if (magSlot4.CurrentItem == null)
            {
                destination = magSlot4;
            }
            else
            {
                //System.Windows.MessageBox.Show("Mag Slot is not available, Please wait!");
                string message = "Mag Slot is not available, Please wait!";
                ReportMessageEvent(PopupType.ShowMessage, message, null);
            }

            Workbench.MoveItem(source, destination);
        }

        /// <summary>
        /// 搬运至加热位置
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToHeating(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            ISlot destination = Workbench.Layout.GetSlotByName("RD_Heating1:RD_Heating_CupSlot1");
            if (heatingSlot1.CurrentItem == null)
            {
                destination = heatingSlot1;
            }
            else if (heatingSlot2.CurrentItem == null)
            {
                destination = heatingSlot2;
            }
            else if (heatingSlot3.CurrentItem == null)
            {
                destination = heatingSlot3;
            }
            else
            {
                System.Windows.MessageBox.Show("Heating Slot is not available, Please wait!");
                //string message = "Heating Slot is not available, Please wait!";
                //ReportMessageEvent(PopupType.ShowMessage, message, null);
				
            }

            Workbench.MoveItem(source, destination);
        }

        /// <summary>
        /// 搬运至冷却位（即六联排原始位置）
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToCool(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            int rackID = 1;
            int slotID = cupIndex;
            if (slotID > 7)
            {
                rackID = 2;
                slotID = slotID - 7;
            }
            ISlot cupSlot =
                Workbench.Layout.GetSlotByName(string.Format("RD_CupRack{0}:RD_CupRack_CupSlot{1}", rackID, slotID));
            Workbench.MoveItem(source, cupSlot);
        }

        /// <summary>
        /// 搬运至读数位
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToReader(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            int slotID = cupIndex;
            while (slotID > 5)
            {
                slotID -= 5;
            }
            ISlot destination = Workbench.Layout.GetSlotByName(string.Format("RD_Reader1:RD_Reader_CupSlot{0}", slotID));

            Workbench.MoveItem(source, destination);
        }

        /// <summary>
        /// 搬运至读数2模块上的冷却位位置
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToCoolDown(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            ISlot destination = Workbench.Layout.GetSlotByName("RD_Reader1:RD_Reader_CoolDownSlot1");
            Workbench.MoveItem(source, destination);
        }

        /// <summary>
        /// 搬运至六联排丢弃位
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void MoveToDrop(int cupIndex)
        {
            IItem source = Workbench.Layout.GetItemByName(string.Format("RD_Cup{0}", cupIndex));
            ISlot destination = Workbench.Layout.GetSlotByName("DropStation1:RD_DropStation_CupSlot1");

            Workbench.MoveItem(source, destination);
        }

        //sdk used
        public void AddMb(SCavityCollection dspCavities)
        {
            if (dspCavities.Count == 0)
            {
                return;
            }
            ICavity c = Workbench.Layout.GetCavityByName("RD_MBBottole1:RD_MBBottole_Cavity1"); //核酸提取液位置
            //SCavityCollection aspCavities = new SCavityCollection();
            //for (int i = 0; i < dspCavities.Count; i++)
            //{
            //    aspCavities.Add(c);
            //}
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(c, dspCavities, 100, 1000, "Mag6");
        }

        /// <summary>
        /// 加磁珠
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void AddMb(int cupIndex)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_MBBottole1:RD_MBBottole_Cavity1"); //核酸提取液位置

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 0;
                sccDestination.Add(cavity);
            }
            Workbench.Transfer(cSource, sccDestination, 100, 1000, "Mag6");
        }

        //sdk used
        public void AddIs(SCavityCollection aspCavities, SCavityCollection dspCavities)
        {
            if (aspCavities.Count == 0 || dspCavities.Count == 0)
            {
                return;
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(aspCavities, dspCavities, 10, 300, "Default");
        }

        /// <summary>
        /// 加内标
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void AddIS(int cupIndex)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_ISBottole1:RD_Bottole_Cavity1"); //内标位置

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 0;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(cSource, sccDestination, 10, 300, "Urine");
        }

        /// <summary>
        /// 加样本
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        public void AddSample(int cupIndex)
        {
            SCavityCollection sccSource = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                int tubeID = (cupIndex - 1)*6 + i;
                while (tubeID > 60)
                {
                    tubeID -= 60;
                }
                string cName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1", tubeID);
                ICavity cSource = Workbench.Layout.GetCavityByName(cName); //样本位置：1-6号试管
                sccSource.Add(cSource);
            }

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i); //六联排第1-6孔
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 30;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(sccSource, sccDestination, 400);
        }

        /// <summary>
        /// 加样本
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        private void AddSample(int cupIndex, int[] cavities)
        {
            SCavityCollection sccSource = new SCavityCollection();
            int[] tubes = new int[cavities.Length];
            for (var m = 0; m < cavities.Length; m++)
            {
                tubes[m] = (cupIndex - 1)*6 + cavities[m];
            }

            for (var n = 0; n < tubes.Length; n++)
            {
                while (tubes[n] > 60)
                {
                    tubes[n] -= 60;
                }
                string cName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1", tubes[n]);
                ICavity cSource = Workbench.Layout.GetCavityByName(cName);
                sccSource.Add(cSource);
            }

            SCavityCollection sccDestination = new SCavityCollection();
            foreach (int c in cavities)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, c);
                //ICavity cavity = Workbench.Layout.GetCavityByName(cName);
                //sccDestination.Add(cavity);
                //20170317
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 30;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(sccSource, sccDestination, 400);
        }

        public void AddSample(int cupIndex, int i)
        {
            switch (i)
            {
                case 1:
                    int[] a = new[] {1, 2, 3};
                    AddSample(cupIndex, a);
                    break;
                case 2:
                    int[] b = new[] {4, 5, 6};
                    AddSample(cupIndex, b);
                    break;
                default:
                    break;
            }
        }

        //sdk used
        public void AddSample(SCavityCollection aspCavities, SCavityCollection dspCavities, double volume)
        {
            if (aspCavities.Count == 0 || dspCavities.Count == 0)
            {
                return;
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 30;
            }
            if (aspCavities[0].Name.Contains("PNBottole"))
            {
                SCavityCollection bufferCavities=new SCavityCollection();
                for (int i = 0; i < dspCavities.Count; i++)
                {
                    string cName = "RD_ReagentBox1:RD_ReagentBox_Cavity1";
                    ICavity c = Workbench.Layout.GetCavityByName(cName);
                    if (c != null)
                    {
                        bufferCavities.Add(c);
                    }
                }
                Transfer(bufferCavities,dspCavities,200,1000);
                SCavityCollection salineCavities = new SCavityCollection();
                for (int i = 0; i < dspCavities.Count; i++)
                {
                    string cName = "RD_ReagentBox2:RD_ReagentBox_Cavity1";
                    ICavity c = Workbench.Layout.GetCavityByName(cName);
                    if (c != null)
                    {
                        salineCavities.Add(c);
                    }
                }
                Transfer(salineCavities, dspCavities, 200, 1000);
                Transfer(aspCavities, dspCavities, 10, 300);
            }
            else
            {
                Transfer(aspCavities, dspCavities, volume, 1000);
            }
        }

        //sdk used
        public void AddAmp(SCavityCollection aspCavities, SCavityCollection dspCavities)
        {
            if (aspCavities.Count == 0 || dspCavities.Count == 0)
            {
                return;
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 10;
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity)cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(aspCavities, dspCavities, 50, 1000, "AMP");
        }

        /// <summary>
        /// 加扩增检测液
        /// </summary>
        /// <param name="cupIndex"></param>
        public void AddAMP(int cupIndex)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_AMPBottole1:RD_AMPBottole_Cavity1"); //扩增检测液孔名

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 10;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(cSource, sccDestination, 50, 1000, "AMP"); //加扩增检测液50ul
        }

        /// <summary>
        /// 加石蜡的量
        /// </summary>
        private const double ParaffinVolume = 200;

        //sdk used
        public void AddParaffin(SCavityCollection dspCavities)
        {
            if (dspCavities.Count == 0)
            {
                return;
            }
            ICavity c = Workbench.Layout.GetCavityByName("RD_ParaffinBox1:RD_ParaffinBox_Cavity1"); //石蜡孔名
            //SCavityCollection aspCavities = new SCavityCollection();
            //for (int i = 0; i < dspCavities.Count; i++)
            //{
            //    aspCavities.Add(c);
            //}
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(c, dspCavities, 200, 1000, "Oil");
        }

        /// <summary>
        /// 加石蜡
        /// </summary>
        /// <param name="cupIndex"></param>
        public void AddParaffin(int cupIndex)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_ParaffinBox1:RD_ParaffinBox_Cavity1"); //石蜡孔名

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 0;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(cSource, sccDestination, ParaffinVolume, 1000, "Oil"); //加石蜡200ul
        }

        //sdk used
        public void AddEz(SCavityCollection aspCavities, SCavityCollection dspCavities)
        {
            if (aspCavities.Count == 0 || dspCavities.Count == 0)
            {
                return;
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity) cavity).DispenseHeightRelative = 10;
            }
            Workbench.Transfer(aspCavities, dspCavities, 13, 300, "Enzyme");
        }

        /// <summary>
        /// 加酶
        /// </summary>
        /// <param name="cupIndex"></param>
        public void AddEnzyme(int cupIndex)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_EzBottole1:RD_Bottole_Cavity1"); //酶孔名

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 10;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(cSource, sccDestination, 13, 300, "Enzyme"); //加酶10ul
        }

        /// <summary>
        /// 吸废液
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        /// <param name="v">吸废液体积</param>
        /// <param name="isLastWaste">是否是最后一次吸废液</param>
        private void Waste(int cupIndex, double v, bool isLastWaste = false)
        {
            SCavityCollection sccSource = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                ICavity c = Workbench.Layout.GetCavityByName(cName);
                sccSource.Add(c);
            }

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 0; i < 3; i++)
            {
                for (var j = 1; j <= 2; j++)
                {
                    string cName = string.Format("RD_WashStation1:RD_WashStation_Cavity{0}", j); //废液槽孔名
                    Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                    cavity.DispenseHeightRelative = 0;
                    sccDestination.Add(cavity);
                }
            }
            const double tipType = 1000; //GetTipType(v);

            Workbench.Transfer(sccSource, sccDestination, v, tipType, isLastWaste ? "lastWaste" : "Waste");
        }

        public enum WasteType
        {
            FirstWaste,
            SecondWaste,
            LastWaste
        }

        //sdk used
        public void FirstWaste(int cupIndex, SCavityCollection aspCavities)
        {
            if (aspCavities.Count == 0)
            {
                return;
            }
            SCavityCollection dspCavities = new SCavityCollection();
            ICavity c = Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
            for (int i = 0; i < aspCavities.Count; i++)
            {
                dspCavities.Add(c);
            }

            Workbench.Transfer(aspCavities, dspCavities, 500, 1000, "firstWaste");
        }

        public void SecondWaste(int cupIndex, SCavityCollection aspCavities)
        {
            if (aspCavities.Count == 0)
            {
                return;
            }
            SCavityCollection dspCavities = new SCavityCollection();
            ICavity c = Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
            for (int i = 0; i < aspCavities.Count; i++)
            {
                dspCavities.Add(c);
            }

            Workbench.Transfer(aspCavities, dspCavities, 800, 1000, "secondWaste");
        }

        public void LastWaste(int cupIndex, SCavityCollection aspCavities)
        {
            if (aspCavities.Count == 0)
            {
                return;
            }
            SCavityCollection dspCavities = new SCavityCollection();
            ICavity c = Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
            for (int i = 0; i < aspCavities.Count; i++)
            {
                dspCavities.Add(c);
            }

            Workbench.Transfer(aspCavities, dspCavities, 800, 1000, "lastWaste");
        }

        /// <summary>
        /// 吸废液
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        /// <param name="v">吸废液体积</param>
        /// <param name="wasteType">第几次吸废液</param>
        private void Waste(int cupIndex, int[] cavities, double v, WasteType wasteType)
        {
            SCavityCollection sccSource = new SCavityCollection();

            foreach (int c in cavities)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, c);
                ICavity cSource = Workbench.Layout.GetCavityByName(cName);
                sccSource.Add(cSource);
            }

            SCavityCollection sccDestination = new SCavityCollection();

            for (var j = 1; j <= 3; j++)
            {
                string cName = string.Format("RD_WashStation2:RD_WashStation_Cavity1"); //WashStation2：打废液
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 5; //20170526
                sccDestination.Add(cavity);
            }

            const double tipType = 1000; //GetTipType(v);

            string category = "";
            switch (wasteType)
            {
                case WasteType.FirstWaste:
                    category = "firstWaste";
                    break;
                case WasteType.SecondWaste:
                    category = "secondWaste";
                    break;
                case WasteType.LastWaste:
                    category = "lastWaste";
                    break;
            }

            Workbench.Transfer(sccSource, sccDestination, v, tipType, category);
        }

        public void Waste(int cupIndex, int i, double v, WasteType wasteType)
        {
            switch (i)
            {
                case 1:
                    int[] a = new[] {1, 2, 3};
                    Workbench.Waste(cupIndex, a, v, wasteType);
                    break;
                case 2:
                    int[] b = new[] {4, 5, 6};
                    Workbench.Waste(cupIndex, b, v, wasteType);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 第一次排废液量
        /// </summary>
        private const double FirstWasteVolume = 500;

        /// <summary>
        /// 第二次排废液量
        /// </summary>
        private const double SecondWasteVolume = 800;

        /// <summary>
        /// 第三次排废液量
        /// </summary>
        private const double LastWasteVolume = 950;

        public void FirstWaste(int cupIndex, int i)
        {
            Waste(cupIndex, i, FirstWasteVolume, WasteType.FirstWaste);
        }

        public void SecondWaste(int cupIndex, int i)
        {
            Waste(cupIndex, i, SecondWasteVolume, WasteType.SecondWaste);
        }

        public void LastWaste(int cupIndex, int i)
        {
            Waste(cupIndex, i, LastWasteVolume, WasteType.LastWaste);
        }

        ///// <summary>
        ///// 返回使用的Tip头类型
        ///// </summary>
        ///// <param name="v">吸液量</param>
        ///// <returns>使用哪种类型的Tip头</returns>
        //private double GetTipType(double v)
        //{
        //    if (v > 300d) return 1000d;
        //    else return 300d;
        //}

        public void FirstWashing(SCavityCollection dspCavities)
        {
            if (dspCavities.Count == 0)
            {
                return;
            }
            ICavity c = Workbench.Layout.GetCavityByName("RD_ReagentBox4:RD_ReagentBox_Cavity1"); //洗涤液孔名
            SCavityCollection aspCavities = new SCavityCollection();
            for (int i = 0; i < dspCavities.Count; i++)
            {
                aspCavities.Add(c);
            }
            foreach (var cavity in dspCavities)
            {
                ((Cavity)cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(aspCavities, dspCavities, 800, 1000, "Washing"); //加洗涤液
        }

        public void SecondWashing(SCavityCollection dspCavities)
        {
            if (dspCavities.Count == 0)
            {
                return;
            }
            ICavity c = Workbench.Layout.GetCavityByName("RD_ReagentBox4:RD_ReagentBox_Cavity1"); //洗涤液孔名
            //SCavityCollection aspCavities = new SCavityCollection();
            //for (int i = 0; i < dspCavities.Count; i++)
            //{
            //    aspCavities.Add(c);
            //}
            foreach (var cavity in dspCavities)
            {
                ((Cavity)cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(c, dspCavities, 600, 1000, "Washing"); //加洗涤液
        }

        /// <summary>
        /// 加洗涤液
        /// </summary>
        /// <param name="cupIndex">六联排编号</param>
        /// <param name="v">加洗涤液的体积</param>
        public void Washing(int cupIndex, double v)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_ReagentBox3:RD_ReagentBox_Cavity1"); //洗涤液孔名

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 0;
                sccDestination.Add(cavity);
            }
            const double tipType = 1000; //GetTipType(v);
            Workbench.Transfer(cSource, sccDestination, v, tipType); //加洗涤液
        }

        private const double FirstWashingVolume = 800;
        private const double SecondWashingVolume = 600;

        public void FirstWashing(int cupIndex)
        {
            Washing(cupIndex, FirstWashingVolume);
        }

        public void SecondWashing(int cupIndex)
        {
            Washing(cupIndex, SecondWashingVolume);
        }

        /// <summary>
        /// 加入矿物油的量
        /// </summary>
        private const double OilVolume = 50;

        public void AddOil(int cupIndex, double v = OilVolume)
        {
            ICavity cSource = Workbench.Layout.GetCavityByName("RD_ReagentBox4:RD_ReagentBox_Cavity1"); //矿物油孔名

            SCavityCollection sccDestination = new SCavityCollection();
            for (var i = 1; i <= 6; i++)
            {
                string cName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", cupIndex, i);
                Cavity cavity = (Cavity) Workbench.Layout.GetCavityByName(cName);
                cavity.DispenseHeightRelative = 0;
                sccDestination.Add(cavity);
            }

            Workbench.Transfer(cSource, sccDestination, v, 1000, "Oil"); //加矿物油
        }

        //sdk used
        public void AddOil(SCavityCollection dspCavities)
        {
            if (dspCavities.Count == 0)
            {
                return;
            }
            ICavity c = Workbench.Layout.GetCavityByName("RD_ReagentBox3:RD_ReagentBox_Cavity1"); //矿物油孔名
            //SCavityCollection aspCavities = new SCavityCollection();
            //for (int i = 0; i < dspCavities.Count; i++)
            //{
            //    aspCavities.Add(c);
            //}
            foreach (var cavity in dspCavities)
            {
                ((Cavity)cavity).DispenseHeightRelative = 0;
            }
            Workbench.Transfer(c, dspCavities, 50, 1000, "Oil");
        }

        #endregion

        //        private Port _port = PortFactory.GetInstance();

        //        private bool IsPortOpen = false;




        //public void Shake30()
        //{
        //    Workbench.OpenShaker(30);
        //    Thread.Sleep(30000);
        //    Workbench.CloseShaker();
        //}

        //public void Shake60()
        //{
        //    Workbench.OpenShaker(60);
        //    Thread.Sleep(60000);
        //    Workbench.CloseShaker();
        //}

        public bool IsShaker1;
        public bool IsShaker2;
        public bool IsShaker3;
        public bool IsShakerPause;
        public bool StopShaker;
        public bool IsShakerOpened = false;



        public void Shake60(int cupIndex)
        {
            int time = 30; //顺时针转30秒
            ChangeNext(cupIndex, false);
            while (time > 0 && !StopShaker)
            {
                while (IsShakerPause)
                {
                    CloseShaker();
                    Thread.Sleep(1);
                }
                OpenShaker(1200, 0);
                time--;
                Thread.Sleep(1000);
            }
            CloseShaker();

            Thread.Sleep(500);
            time = 30; //逆时针转30秒
            while (time > 0 && !StopShaker)
            {
                while (IsShakerPause)
                {
                    CloseShaker();
                    Thread.Sleep(1);
                }
                OpenShaker(1200, 1);
                time--;
                Thread.Sleep(1000);
            }

            CloseShaker();

            ChangeNext(cupIndex, true);
        }

        public delegate void ShakeFinishedHandler(int napId);

        public ShakeFinishedHandler ShakeFinishedEvent;

        public delegate void WaitFinishedHandler(int napId);

        public WaitFinishedHandler WaitFinishedEvent;
        

        public void Wait(int index, int time)
        {
            Task.Factory.StartNew(() =>
            {
                ChangeNext(index, false);
                while (time > 0)
                {
                    Thread.Sleep(1000);
                    time--;
                }
                ChangeNext(index, true);
                WaitFinishedEvent(index);
            });
        }

        public bool DoCup1Next;
        public bool DoCup2Next;
        public bool DoCup3Next;
        public bool DoCup4Next;
        public bool DoCup5Next;
        public bool DoCup6Next;
        public bool DoCup7Next;
        public bool DoCup8Next;
        public bool DoCup9Next;
        public bool DoCup10Next;
        public bool DoCup11Next;
        public bool DoCup12Next;
        public bool DoCup13Next;
        public bool DoCup14Next;

        private void ChangeNext(int index, bool doNext)
        {
            switch (index)
            {
                case 1:
                    DoCup1Next = doNext;
                    break;
                case 2:
                    DoCup2Next = doNext;
                    break;
                case 3:
                    DoCup3Next = doNext;
                    break;
                case 4:
                    DoCup4Next = doNext;
                    break;
                case 5:
                    DoCup5Next = doNext;
                    break;
                case 6:
                    DoCup6Next = doNext;
                    break;
                case 7:
                    DoCup7Next = doNext;
                    break;
                case 8:
                    DoCup8Next = doNext;
                    break;
                case 9:
                    DoCup9Next = doNext;
                    break;
                case 10:
                    DoCup10Next = doNext;
                    break;
                case 11:
                    DoCup11Next = doNext;
                    break;
                case 12:
                    DoCup12Next = doNext;
                    break;
                case 13:
                    DoCup13Next = doNext;
                    break;
                case 14:
                    DoCup14Next = doNext;
                    break;
            }
        }

        /// <summary>
        /// 填充指定Tip载架
        /// </summary>
        /// <param name="tipRackIndex">载架编号1~6；1=300ulTip；2~5=1000ulTip</param>
        public void FillTips(int tipRackIndex)
        {
            string trName = string.Format("DTRack{0}", tipRackIndex);
            IItem tipRack = Workbench.Layout.GetItemByName(trName);
            Workbench.SetTipRackState(tipRack, STipPositionState.Pickable);
        }

        /// <summary>
        /// 填充1000ulTip头；TipRack=2~6；
        /// </summary>
        public void Fill1000Tips()
        {
            for (var i = 2; i <= 6; i++)
            {
                FillTips(i);
            }

        }

        /*
         * 0.正在搬运：参数1=起始位置Slot编号，参数2=目标位置Slot编号；Slot编号通过GetSlotPos(ISlot slot)获取
         * 1.搬运：参数1=起始位置Slot编号，参数2=目标位置Slot编号；Slot编号通过GetSlotPos(ISlot slot)获取
         * 2.吸液：参数1=List<吸液孔位>，参数2=Dictionary<孔位编号,所吸孔位的液量>；孔位集合通过GetCavityPos(SCavityCollection cavities)获取
         * 3.打液：参数1=六联排编号，参数2="111111"，用于表示六个孔位是有液体的状态，0：无液体； 1：有液体。
         * 4.抽废液：参数1=六联排编号（1~21），参数2="111111"，用于表示六个孔位是有液体的状态，0：无液体； 1：有液体。
         * 5.取Tip头：参数1=Dictionary<TipRack编号,TipRack的Tip头状态>，参数2=用不到>
         */

        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="action">动作，枚举类型</param>
        /// <param name="obj1">参数一</param>
        /// <param name="obj2">参数二</param>
        public delegate void RefreshUiHandler(RdAction action, object obj1, object obj2);

        public event RefreshUiHandler RefreshUiEvent;

        //报错
        public delegate void ReportMessageHandler(Enum.PopupType popupType, string message, Action[] actions);

        public static event ReportMessageHandler ReportMessageEvent;

        #region 刷新界面
        /// <summary>
        /// 获取孔位编号
        /// </summary>
        /// <param name="cavities"></param>
        /// <returns></returns>
        private List<int> GetCavityPos(SCavityCollection cavities)
        {
            List<int> pos = new List<int>();

            foreach (ICavity c in cavities)
            {
                int p = -1;
                switch (c.Name)
                {
                    case "RD_MBBottole1:RD_MBBottole_Cavity1":
                        p = 11;
                        break;
                    case "RD_MBBottole2:RD_MBBottole_Cavity1":
                        p = 12;
                        break;
                    case "RD_AMPBottole1:RD_AMPBottole_Cavity1":
                        p = 21;
                        break;
                    case "RD_AMPBottole2:RD_AMPBottole_Cavity1":
                        p = 22;
                        break;
                    case "RD_AMPBottole3:RD_AMPBottole_Cavity1":
                        p = 23;
                        break;
                    case "RD_AMPBottole4:RD_AMPBottole_Cavity1":
                        p = 24;
                        break;
                    case "RD_PNBottole1:RD_PNBottole_Cavity1":
                        p = 31;
                        break;
                    case "RD_PNBottole2:RD_PNBottole_Cavity1":
                        p = 32;
                        break;
                    case "RD_PNBottole3:RD_PNBottole_Cavity1":
                        p = 33;
                        break;
                    case "RD_PNBottole4:RD_PNBottole_Cavity1":
                        p = 34;
                        break;
                    case "RD_ISBottole1:RD_Bottole_Cavity1":
                        p = 41;
                        break;
                    case "RD_ISBottole2:RD_Bottole_Cavity1":
                        p = 42;
                        break;
                    case "RD_ISBottole3:RD_Bottole_Cavity1":
                        p = 43;
                        break;
                    case "RD_ISBottole4:RD_Bottole_Cavity1":
                        p = 44;
                        break;
                    case "RD_PNBottole5:RD_PNBottole_Cavity1":
                        p = 51;
                        break;
                    case "RD_PNBottole6:RD_PNBottole_Cavity1":
                        p = 52;
                        break;
                    case "RD_PNBottole7:RD_PNBottole_Cavity1":
                        p = 53;
                        break;
                    case "RD_PNBottole8:RD_PNBottole_Cavity1":
                        p = 54;
                        break;
                    case "RD_ReagentBox1:RD_ReagentBox_Cavity1":
                        p = 61;
                        break;
                    case "RD_ReagentBox2:RD_ReagentBox_Cavity1":
                        p = 62;
                        break;
                    case "RD_ReagentBox3:RD_ReagentBox_Cavity1":
                        p = 63;
                        break;
                    case "RD_ReagentBox4:RD_ReagentBox_Cavity1":
                        p = 64;
                        break;
                    case "RD_ReagentBox5:RD_ReagentBox_Cavity1":
                        p = 65;
                        break;
                    case "RD_ReagentBox6:RD_ReagentBox_Cavity1":
                        p = 66;
                        break;
                    case "RD_ReagentBox7:RD_ReagentBox_Cavity1":
                        p = 67;
                        break;
                    case "RD_ReagentBox8:RD_ReagentBox_Cavity1":
                        p = 68;
                        break;
                    case "RD_EzBottole1:RD_Bottole_Cavity1":
                        p = 71;
                        break;
                    case "RD_EzBottole2:RD_Bottole_Cavity1":
                        p = 72;
                        break;
                    case "RD_EzBottole3:RD_Bottole_Cavity1":
                        p = 73;
                        break;
                    case "RD_EzBottole4:RD_Bottole_Cavity1":
                        p = 74;
                        break;
                }
                pos.Add(p);
            }
            return pos;
        }

        private int GetCavityPos(ICavity cavity)
        {
            int p = -1;
            switch (cavity.Name)
            {
                case "RD_MBBottole1:RD_MBBottole_Cavity1":
                    p = 11;
                    break;
                case "RD_MBBottole2:RD_MBBottole_Cavity1":
                    p = 12;
                    break;
                case "RD_AMPBottole1:RD_AMPBottole_Cavity1":
                    p = 21;
                    break;
                case "RD_AMPBottole2:RD_AMPBottole_Cavity1":
                    p = 22;
                    break;
                case "RD_AMPBottole3:RD_AMPBottole_Cavity1":
                    p = 23;
                    break;
                case "RD_AMPBottole4:RD_AMPBottole_Cavity1":
                    p = 24;
                    break;
                case "RD_PNBottole1:RD_PNBottole_Cavity1":
                    p = 31;
                    break;
                case "RD_PNBottole2:RD_PNBottole_Cavity1":
                    p = 32;
                    break;
                case "RD_PNBottole3:RD_PNBottole_Cavity1":
                    p = 33;
                    break;
                case "RD_PNBottole4:RD_PNBottole_Cavity1":
                    p = 34;
                    break;
                case "RD_ISBottole1:RD_Bottole_Cavity1":
                    p = 41;
                    break;
                case "RD_ISBottole2:RD_Bottole_Cavity1":
                    p = 42;
                    break;
                case "RD_ISBottole3:RD_Bottole_Cavity1":
                    p = 43;
                    break;
                case "RD_ISBottole4:RD_Bottole_Cavity1":
                    p = 44;
                    break;
                case "RD_PNBottole5:RD_PNBottole_Cavity1":
                    p = 51;
                    break;
                case "RD_PNBottole6:RD_PNBottole_Cavity1":
                    p = 52;
                    break;
                case "RD_PNBottole7:RD_PNBottole_Cavity1":
                    p = 53;
                    break;
                case "RD_PNBottole8:RD_PNBottole_Cavity1":
                    p = 54;
                    break;
                case "RD_ReagentBox1:RD_ReagentBox_Cavity1":
                    p = 61;
                    break;
                case "RD_ReagentBox2:RD_ReagentBox_Cavity1":
                    p = 62;
                    break;
                case "RD_ReagentBox3:RD_ReagentBox_Cavity1":
                    p = 63;
                    break;
                case "RD_ReagentBox4:RD_ReagentBox_Cavity1":
                    p = 64;
                    break;
                case "RD_ReagentBox5:RD_ReagentBox_Cavity1":
                    p = 65;
                    break;
                case "RD_ReagentBox6:RD_ReagentBox_Cavity1":
                    p = 66;
                    break;
                case "RD_ReagentBox7:RD_ReagentBox_Cavity1":
                    p = 67;
                    break;
                case "RD_ReagentBox8:RD_ReagentBox_Cavity1":
                    p = 68;
                    break;
                case "RD_EzBottole1:RD_Bottole_Cavity1":
                    p = 71;
                    break;
                case "RD_EzBottole2:RD_Bottole_Cavity1":
                    p = 72;
                    break;
                case "RD_EzBottole3:RD_Bottole_Cavity1":
                    p = 73;
                    break;
                case "RD_EzBottole4:RD_Bottole_Cavity1":
                    p = 74;
                    break;
            }
            return p;
        }

        /// <summary>
        /// 获取Slot编号（0~38）
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetSlotPos(ISlot slot)
        {
            int pos = -1;
            switch (slot.Name)
            {
                case "RD_CupRack1:RD_CupRack_CupSlot1":
                    pos = 1;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot2":
                    pos = 2;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot3":
                    pos = 3;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot4":
                    pos = 4;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot5":
                    pos = 5;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot6":
                    pos = 6;
                    break;
                case "RD_CupRack1:RD_CupRack_CupSlot7":
                    pos = 7;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot1":
                    pos = 8;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot2":
                    pos = 9;
                    break;
                case "RD_CupRack2RD_CupRack_CupSlot3":
                    pos = 10;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot4":
                    pos = 11;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot5":
                    pos = 12;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot6":
                    pos = 13;
                    break;
                case "RD_CupRack2:RD_CupRack_CupSlot7":
                    pos = 14;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot1":
                    pos = 15;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot2":
                    pos = 16;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot3":
                    pos = 17;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot4":
                    pos = 18;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot5":
                    pos = 19;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot6":
                    pos = 20;
                    break;
                case "RD_CupRack3:RD_CupRack_CupSlot7":
                    pos = 21;
                    break;
                case "RD_Heating1:RD_Heating_CupSlot1":
                    pos = 22;
                    break;
                case "RD_Heating1:RD_Heating_CupSlot2":
                    pos = 23;
                    break;
                case "RD_Heating1:RD_Heating_CupSlot3":
                    pos = 24;
                    break;
                case "RD_Heating1:RD_Heating_CupSlot4":
                    pos = 25;
                    break;
                case "RD_ShakerRack1:RD_ShakerRack_CupSlot1":
                    pos = 26;
                    break;
                case "RD_ShakerRack1:RD_ShakerRack_CupSlot2":
                    pos = 27;
                    break;
                case "RD_ShakerRack1:RD_ShakerRack_CupSlot3":
                    pos = 28;
                    break;
                case "RD_Mag1:RD_Mag_CupSlot1":
                    pos = 29;
                    break;
                case "RD_Mag1:RD_Mag_CupSlot2":
                    pos = 30;
                    break;
                case "RD_Mag1:RD_Mag_CupSlot3":
                    pos = 31;
                    break;
                case "RD_Mag1:RD_Mag_CupSlot4":
                    pos = 32;
                    break;
                case "RD_Reader1:RD_Reader_CupSlot1":
                    pos = 33;
                    break;
                case "RD_Reader1:RD_Reader_CupSlot2":
                    pos = 34;
                    break;
                case "RD_Reader1:RD_Reader_CupSlot3":
                    pos = 35;
                    break;
                case "RD_Reader1:RD_Reader_CupSlot4":
                    pos = 36;
                    break;
                case "RD_Reader1:RD_Reader_CupSlot5":
                    pos = 37;
                    break;
                case "RD_Reader1:RD_Reader_CoolDownSlot1":
                    pos = 28;
                    break;
                case "DropStation1:RD_DropStation_CupSlot1":
                    pos = 0;
                    break;
            }
            return pos;
        }

        /// <summary>
        /// 获取孔位编号及液量
        /// </summary>
        /// <param name="cavities"></param>
        /// <returns></returns>
        public Dictionary<int, double> GetVolume(SCavityCollection cavities)
        {
            Dictionary<int, double> dicVolume = new Dictionary<int, double>();

            foreach (ICavity cavity in cavities)
            {
				if (dicVolume.ContainsKey(GetCavityPos(cavity)))
				{
					continue;
				}
                dicVolume.Add(GetCavityPos(cavity), cavity.Liquid.Volume);
            }

            return dicVolume;
        }

        /// <summary>
        /// 获取六联排编号
        /// </summary>
        /// <param name="cavities"></param>
        /// <returns></returns>
        public int GetNapId(SCavityCollection cavities)
        {
            int napId = 0;
            string napName = cavities[0].Name; //RD_Cup21:RD_Cup_Cavity6
            string strInt = System.Text.RegularExpressions.Regex.Replace(napName, @"[^0-9]+", ""); //去除非数字部分
            strInt = strInt.Substring(0, strInt.Length - 1); //去除最后一位
            try
            {
                napId = Convert.ToInt32(strInt); //字符串转整形
            }
            catch (Exception)
            {

            }
            return napId;
        }

        /// <summary>
        /// 获取六联排孔位状态
        /// </summary>
        /// <param name="cavities"></param>
        /// <returns>0：无液体    1：有液体</returns>
        public string GetDspCells(SCavityCollection cavities)
        {
            string s="ABCDEF";
            foreach (ICavity cavity in cavities)
            {
                int cellId = 0;
                string strCellId = cavity.Name.Substring(cavity.Name.Length - 1, 1);
                try
                {
                    cellId = Convert.ToInt32(strCellId);
                }
                catch (Exception)
                {

                }
                s=UpdateString(s,cellId,'1');
            }
            s = s.Replace('F', '0');
            s = s.Replace('E', '0');
            s = s.Replace('D', '0');
            s = s.Replace('C', '0');
            s = s.Replace('B', '0');
            s = s.Replace('A', '0');
            return s;
        }

        /// <summary>
        /// 获取抽废液孔位状态
        /// </summary>
        /// <param name="cavities"></param>
        /// <returns>0：无液体（已抽）      1：有液体（未抽）</returns>
        public string GetWasteCells(SCavityCollection cavities)
        {
            string s = "ABCDEF";
            foreach (ICavity cavity in cavities)
            {
                int cellId = 0;
                string strCellId = cavity.Name.Substring(cavity.Name.Length - 1, 1);
                try
                {
                    cellId = Convert.ToInt32(strCellId);
                }
                catch (Exception)
                {

                }
                s = UpdateString(s, cellId,'0');
            }
            s = s.Replace('F', '1');
            s = s.Replace('E', '1');
            s = s.Replace('D', '1');
            s = s.Replace('C', '1');
            s = s.Replace('B', '1');
            s = s.Replace('A', '1');
            return s;
        }

        private string UpdateString(string s,int i,char c)
        {
            switch (i)
            {
                case 1:
                    s = s.Replace('F', c);
                    break;
                case 2:
                    s = s.Replace('E', c);
                    break;
                case 3:
                    s = s.Replace('D', c);
                    break;
                case 4:
                    s = s.Replace('C', c);
                    break;
                case 5:
                    s = s.Replace('B', c);
                    break;
                case 6:
                    s = s.Replace('A', c);
                    break;
            }
            
            return s;
        }

        /// <summary>
        /// 获取Tip头状态
        /// </summary>
        /// <returns></returns>
        public Dictionary<int,string> GetTipStates()
        {
            Dictionary<int,string> dicState=new Dictionary<int, string>();
            for (int i = 1; i <= 9; i++)
            {
                string state = "";
                string tipRackName = string.Format("DTRack{0}", i);
                IItem tipRack = Layout.GetItemByName(tipRackName);
                TipStateSaveData.TipRackSaveData trsd = new TipStateSaveData.TipRackSaveData(tipRack);
                state = trsd.TipPosStates;
                dicState.Add(i,state);
            }
            return dicState;
        }

        #endregion 刷新界面

        #region 震荡
        /// <summary>
        /// 震荡
        /// </summary>
        /// <param name="index">六联排序号</param>
        /// <param name="time">震荡时间（秒）</param>
        /// <param name="direction">震荡方向（0：顺时针；1：逆时针）</param>
        public void Shake(int index, int time, int direction = 0)
        {
            Task.Factory.StartNew(() =>
            {
                ChangeNext(index, false);
                while (time > 0 && !StopShaker)
                {
                    while (IsShakerPause)
                    {
                        CloseShaker();
                        Thread.Sleep(1);
                    }
                    OpenShaker(time, direction);
                    time--;
                    Thread.Sleep(1000);
                }

                CloseShaker();
                ChangeNext(index, true);
                ShakeFinishedEvent(index);
            });
        }

        public ErrCode OpenShaker(int time, int direction = 0)
        {
            CRdsModule rdsModule = new CRdsModule();
            ErrCode errCode = ErrCode.EC_OK;
            object retObj = null; //接收到的对象;
            // 测试
            errCode = rdsModule.Shake(true, 1000, time, 0);//振动30秒
            return errCode;
        }

        public  ErrCode CloseShaker()
        {
            CRdsModule rdsModule = new CRdsModule();
            ErrCode errCode = ErrCode.EC_OK;
            // 测试
            errCode = rdsModule.Shake(false, 0, 0, 0);//振动30秒
            return errCode;
        }
        #endregion 震荡

        #region 移液

        /// <summary>
        /// 根据吸液孔，得到使用几个Tip去吸
        /// </summary>
        /// <param name="aspCavities">吸液孔</param>
        /// <returns></returns>
        private int GetUsedTipCount(SCavityCollection aspCavities)
        {
            ICavity c = aspCavities[0];
            return 1 + aspCavities.Cast<ICavity>().Count(cavity => cavity.Name != c.Name);
            //int usedTipCount = 1;
            //ICavity c = aspCavities[0];
            //foreach (ICavity cavity in aspCavities)
            //{
            //    if (cavity.Name != c.Name)
            //    {
            //        usedTipCount++;
            //    }
            //}
            //return usedTipCount;
        }

        /// <summary>
        /// 得到吸液量数组
        /// </summary>
        /// <param name="aspCavities"></param>
        /// <param name="dspCavities"></param>
        /// <param name="volume"></param>
        /// <param name="tipV"></param>
        /// <returns></returns>
        private double[] GetAspVolumes(SCavityCollection aspCavities, SCavityCollection dspCavities, double volume, double tipV)
        {
            double[] aspVolume=new double[3];

            if (aspCavities[0].Name == aspCavities[1].Name&&aspCavities[0].Name==aspCavities[2].Name)
            {
                aspVolume[0] = volume*3;
                aspVolume[1] = 0;
                aspVolume[2] = 0;
            }
            else if (aspCavities[0].Name == aspCavities[1].Name)
            {
                aspVolume[0] = volume*2;
                aspVolume[1] = volume;
                aspVolume[2] = 0;
            }
            else if(aspCavities[1].Name == aspCavities[2].Name)
            {
                aspVolume[0] = volume;
                aspVolume[1] = volume*2;
                aspVolume[2] = 0;
            }
            else if (aspCavities[0].Name == aspCavities[2].Name)
            {
                aspVolume[0] = volume*2;
                aspVolume[1] = volume;
                aspVolume[2] = 0;
            }
            else
            {
                aspVolume[0] = volume;
                aspVolume[1] = volume;
                aspVolume[2] = volume;
            }
            return aspVolume;
        }

        private bool IsSame(SCavityCollection cavities)
        {
            bool isSame = true;
            ICavity c = cavities[0];
            if (cavities.Cast<ICavity>().Any(cavity => cavity.Name != c.Name))
            {
                isSame = false;
                return isSame;
            }
            //foreach (ICavity cavity in cavities)
            //{
            //    if (cavity.Name != c.Name)
            //    {
            //        isSame = false;
            //        return isSame;
            //    }
            //}
            return isSame;
        }

        //public void Transfer(SCavityCollection aspCavities, SCavityCollection dspCavities, double volume, double tipV = 1000, string category = "Default")
        //{
        //    //如果源位置与目标位置个数不符，则提示
        //    if (aspCavities.Count != dspCavities.Count)
        //    {
        //        //System.Windows.MessageBox.Show("Count of source cavities does not match number of destination cavities","Warning");
        //        string message = "Count of source cavities does not match number of destination cavities";
        //        ReportMessageEvent(PopupType.ShowMessage, message, null);
        //        return;
        //    }

        //    #region 准备数据

        //    SVolumeBand vb = new SVolumeBand();
        //    switch (category)
        //    {
        //        case "Mag6":
        //            vb = Workbench.GetVolumeBand("Default", "Mag6", volume, tipV);
        //            break;
        //        case "Washing":
        //            vb = Workbench.GetVolumeBand("Default", "Washing", volume, tipV);
        //            break;
        //        case "AMP":
        //            vb = Workbench.GetVolumeBand("Default", "AMP", volume, tipV);
        //            break;
        //        case "firstWaste":
        //        case "secondWaste":
        //        case "lastWaste":
        //            vb = Workbench.GetVolumeBand("Default", "Waste", volume, tipV);
        //            break;
        //        case "Oil":
        //            vb = Workbench.GetVolumeBand("Default", "Oil", volume, tipV);
        //            break;
        //        case "Enzyme":
        //            vb = Workbench.GetVolumeBand("Default", "Enzyme", volume, tipV);
        //            break;
        //        case "Urine":
        //            vb = Workbench.GetVolumeBand("Default", "Urine", volume, tipV);
        //            break;
        //        case "Default":
        //        default:
        //            vb = Workbench.GetVolumeBand("Default", "Default", volume, tipV);
        //            break;
        //    }

        //    //Y方向最外侧
        //    double[] maxY = { 371.4, 380.2, 389 }; //389=ADP Y方向最大值;最大值取388，余量1；
        //    double destinationX;
        //    double[] destinationY = new double[3];
        //    double tempy = Workbench.Layout.GetSlotByName("RD_CupRack1:RD_CupRack_CupSlot1").Position.Y - 2;
        //    double[] tempY = new double[3];

        //    double[] aspVolume = new double[] {0,0,0,0,0,0};
        //    double[] dspVolume=new double[] {0,0,0,0,0,0};
        //    //本次加样需要吸取的孔位
        //    SCavityCollection usedAspCavities = new SCavityCollection();
        //    int usedTipCount = 3;

        //    switch (aspCavities.Count)
        //    {
        //        case 6:
        //            if (IsSame(aspCavities))
        //            {
        //                if (aspCavities[0].Name.Contains("ReagentBox3") || aspCavities[0].Name.Contains("ParaffinBox"))
        //                {
        //                    if (volume*2 < tipV)
        //                    {
        //                        aspVolume[0] = volume*2;
        //                        aspVolume[1] = volume*2;
        //                        aspVolume[2] = volume*2;
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        usedAspCavities.Add(aspCavities[0]);
        //                    }
        //                    else
        //                    {
        //                        for (int i = 0; i < aspCavities.Count; i++)
        //                        {
        //                            aspVolume[i] = volume;
        //                            usedAspCavities.Add(aspCavities[0]);
        //                        }
        //                    }

        //                    usedTipCount = 3;
        //                }
        //                else
        //                {
        //                    aspVolume[0] = volume * 6;
        //                    aspVolume[1] = 0;
        //                    aspVolume[2] = 0;
        //                    usedAspCavities.Add(aspCavities[0]);
        //                    usedTipCount = 1;
        //                }
        //            }
        //            break;
        //        case 5:
        //            if (IsSame(aspCavities))
        //            {
        //                if (aspCavities[0].Name.Contains("ReagentBox3") || aspCavities[0].Name.Contains("ParaffinBox"))
        //                {
        //                    if (volume*2 < tipV)
        //                    {
        //                        aspVolume[0] = volume*2;
        //                        aspVolume[1] = volume*2;
        //                        aspVolume[2] = volume;
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        usedAspCavities.Add(aspCavities[0]);
        //                    }
        //                    else
        //                    {
        //                        for (int i = 0; i < aspCavities.Count; i++)
        //                        {
        //                            aspVolume[i] = volume;
        //                            usedAspCavities.Add(aspCavities[0]);
        //                        }
        //                    }
        //                    usedTipCount = 3;
        //                }
        //                else
        //                {
        //                    aspVolume[0] = volume*5;
        //                    aspVolume[1] = 0;
        //                    aspVolume[2] = 0;
        //                    usedAspCavities.Add(aspCavities[0]);
        //                    usedTipCount = 1;
        //                }
        //            }
        //            break;
        //        case 4:
        //            if (IsSame(aspCavities))
        //            {
        //                if (aspCavities[0].Name.Contains("ReagentBox3") || aspCavities[0].Name.Contains("ParaffinBox"))
        //                {
        //                    if (volume*2 < tipV)
        //                    {
        //                        aspVolume[0] = volume*2;
        //                        aspVolume[1] = volume*2;
        //                       // aspVolume[2] = volume;
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        usedAspCavities.Add(aspCavities[0]);
        //                        //usedAspCavities.Add(aspCavities[0]);
        //                    }
        //                    else
        //                    {
        //                        for (int i = 0; i < aspCavities.Count; i++)
        //                        {
        //                            aspVolume[i] = volume;
        //                            usedAspCavities.Add(aspCavities[0]);
        //                        }
        //                    }
        //                    usedTipCount = 2;
        //                }
        //                else
        //                {
        //                    aspVolume[0] = volume*4;
        //                    aspVolume[1] = 0;
        //                    aspVolume[2] = 0;
        //                    usedAspCavities.Add(aspCavities[0]);
        //                    usedTipCount = 1;
        //                }
        //            }
        //            break;
        //        case 3:
        //            if (aspCavities[0].Name == aspCavities[1].Name && aspCavities[0].Name == aspCavities[2].Name)
        //            {
        //                aspVolume[0] = volume * 3;
        //                aspVolume[1] = 0;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedTipCount = 1;
        //            }
        //            else if (aspCavities[0].Name == aspCavities[1].Name)
        //            {
        //                aspVolume[0] = volume * 2;
        //                aspVolume[1] = volume;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[2]);
        //            }
        //            else if (aspCavities[0].Name == aspCavities[2].Name)
        //            {
        //                aspVolume[0] = volume * 2;
        //                aspVolume[1] = volume;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[1]);
        //                usedTipCount = 2;
        //            }
        //            else if (aspCavities[1].Name == aspCavities[2].Name)
        //            {
        //                aspVolume[0] = volume;
        //                aspVolume[1] = volume * 2;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[1]);
        //                usedTipCount = 2;
        //            }
        //            else
        //            {
        //                aspVolume[0] = volume;
        //                aspVolume[1] = volume;
        //                aspVolume[2] = volume;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[1]);
        //                usedAspCavities.Add(aspCavities[2]);
        //                usedTipCount = 3;
        //            }
        //            break;
        //        case 2:
        //            if (aspCavities[0].Name == aspCavities[1].Name)
        //            {
        //                aspVolume[0] = volume * 2;
        //                aspVolume[1] = 0;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[1]);
        //                usedTipCount = 1;
        //            }
        //            else
        //            {
        //                aspVolume[0] = volume;
        //                aspVolume[1] = volume;
        //                aspVolume[2] = 0;
        //                usedAspCavities.Add(aspCavities[0]);
        //                usedAspCavities.Add(aspCavities[1]);
        //                usedTipCount = 2;
        //            }
        //            break;
        //        case 1:
        //            aspVolume[0] = volume;
        //            usedAspCavities.Add(aspCavities[0]);
        //            usedTipCount = 1;
        //            break;
        //    }
        //    for (int n = 0; n < dspCavities.Count;n++)
        //    {
        //        dspVolume[n] = volume;
        //    }

        //    int addTimes = dspCavities.Count/usedTipCount;
        //    if (dspCavities.Count%usedTipCount > 0)
        //    {
        //        addTimes++;
        //    }

        //    STipMap usedTips = new STipMap(0, usedTipCount - 1);
        //    SPipettingMethods pipMethods = Workbench.PipMethods;
        //    // make sure the robot is connected
        //    Workbench.StartSequence(PipMethods.AllTips); // start sequence using all tips
        //    STipMap usedDt = Workbench.DTAdapter.Combine(usedTips); // used DT adapters
        //    SPipettingPar aspLiquid = new SPipettingPar(3);
        //    SPipettingPar sysAir = Workbench.GetAirPar(vb.SystemAirgap);
        //    SPipettingPar transAir = Workbench.GetAirPar(vb.TransportationAirgap);
        //    SDetectionClass dc = Workbench.DetectionManager.GetDetectionClass("Default");
        //    SDetectionPar detPar = dc.DetectionParameters;
        //    SPipettingPar spit = Workbench.GetAspVolumePar(vb.SpitBack, vb);
        //    SMixPar mixPar = new SMixPar();
        //    detPar.DisplayErrors = true;
           
            

        //    #endregion 准备数据

        //    #region // Get disposable tip (optional)

        //    //Flush system liquid before get tips.
        //    Workbench.FlushADP(400);

        //    if (usedDt.AnyTipUsed)
        //    {
        //        // We have used disposable tip on the sequence
        //        // Important: Please note that Tip check is not working yet!
        //        SGetTipsPar gtp = new SGetTipsPar
        //        {
        //            DisplayHasTipsError = true,
        //            DisplayBadTipsError = true
        //        };

        //        //2017-03-09 zhangjing
        //        //Tip头用完后，提示用户填充Tip头；
        //        if (tipV == 300)
        //        {
        //            ITipType tt = new STipType("Rainin Universal 300");
        //            IItem tipRack = Workbench.Layout.GetItemByName("DTRack1");
        //            int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
        //            if (tipNum < usedDt.NoOfUsedTips)
        //            {
        //                //System.Windows.MessageBox.Show("300ul Tip头不足，请补充后点击确定按钮！");
        //                string message = "300ul Tip头不足，请补充后点击确定按钮！";
        //                ReportMessageEvent(PopupType.ShowMessage, message, null);
        //                FillTips(1);
        //            }
        //            PipMethods.GetTips(usedDt, tt, gtp);
        //            //界面更新Tip头状态
        //            RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
        //        }
        //        else if (tipV == 1000)
        //        {
        //            ITipType tt = new STipType("Rainin Universal 1000");
        //            IItem tipRack = Workbench.Layout.GetItemByName("DTRack9");
        //            int tipNum = Workbench.GetNumberOfTips(tipRack, STipPositionState.Pickable);
        //            if (tipNum < usedDt.NoOfUsedTips)
        //            {
        //                //System.Windows.MessageBox.Show("1000ul Tip头不足，请补充后点击确定按钮！");
        //                string message = "1000ul Tip头不足，请补充后点击确定按钮！";
        //                ReportMessageEvent(PopupType.ShowMessage, message, null);
        //                Fill1000Tips();
        //            }
        //            PipMethods.GetTips(usedDt, tt, gtp);
        //            //界面更新Tip头状态
        //            RefreshUiEvent(RdAction.GetTip, GetTipStates(), null);
        //        }
        //    }

        //    #endregion

        //    for (int i = 0; i < addTimes; i++)
        //    {
        //        #region // Aspirate liquid

        //        for (int tipIndex = 0; tipIndex < usedTips.NoOfUsedTips; tipIndex++)
        //        {
        //            usedTips[tipIndex] = true;
        //            aspLiquid[tipIndex].Volume = aspVolume[tipIndex+addTimes*usedTips.NoOfUsedTips] + vb.WasteVolume + vb.SpitBack;//吸液体积+WasteVolume+SpitBack
        //        }

        //        //2017-03-09 zhangjing
        //        //取完Tip头后，如果是去震荡模块或者吸磁模块，需避让读数模块
        //        if (usedAspCavities[0].ParentItem.CurrentSlot.Name.Contains("RD_ShakerRack") ||
        //            usedAspCavities[0].ParentItem.CurrentSlot.Name.Contains("RD_Mag"))
        //        {
        //            //Z轴上升到安全距离
        //            PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);
        //            Item paraffinBox = (Item) Workbench.Layout.GetItemByName("RD_ParaffinBox1");
        //            double x = paraffinBox.Position.X + 19.4; //19.4=ParaffinBox长度
        //            double y0 = paraffinBox.Position.Y - 50;
        //            double y1 = paraffinBox.Position.Y - 40;
        //            double y2 = paraffinBox.Position.Y - 30;
        //            double[] y = new double[] {y0, y1, y2};
        //            PipMethods.MoveXY(x, y);
        //        }

        //        if (category == "Oil")
        //        {
        //            detPar.UseLiquidDetection = false;
        //            detPar.UseTracking = false;
        //            detPar.LimitVolumeToPickable = false;

        //            // Aspirate
        //            PipMethods.Aspirate(usedTips, usedAspCavities, sysAir, aspLiquid, transAir, spit, mixPar, detPar);
        //            //吸液后刷新界面
        //            RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedAspCavities), GetVolume(usedAspCavities));
        //        }
        //        else if (category == "firstWaste" || category == "secondWaste")
        //        {
        //            detPar.UseLiquidDetection = false;
        //            detPar.UseTracking = true;
        //            detPar.LimitVolumeToPickable = false;
        //            aspLiquid.ZMode = SPipettingPar.ModeConst.RelativeToLiquidPosition;

        //            for (var k = 0; k < usedTips.MaxTip(usedAspCavities); k++)
        //            {
        //                //usedSource[k].Liquid.Volume = aspLiquid.VolArr[k];
        //                usedAspCavities[k].Liquid = new SLiquid("sample", aspLiquid.VolArr[k], new SLiquidState());
        //                aspLiquid.ZOffset[k] = -2;
        //            }


        //            //                    SErrorManager.AddQuietHandler(MyErrorHandler);
        //            // Aspirate
        //            PipMethods.Aspirate(usedTips, usedAspCavities, sysAir, aspLiquid, transAir, spit, mixPar, detPar);
        //            //                   SErrorManager.RemoveQuietHandler(MyErrorHandler);
        //            //刷新界面
        //            RefreshUiEvent(RdAction.Waste, GetNapId(usedAspCavities), GetWasteCells(usedAspCavities));
        //        }
        //        else if (category == "lastWaste")
        //        {
        //            SPipettingPar aspLiquid2 = Workbench.GetAspVolumePar(800, vb);

        //            detPar.UseLiquidDetection = false;
        //            detPar.UseTracking = true;
        //            detPar.LimitVolumeToPickable = false;
        //            aspLiquid2.ZMode = SPipettingPar.ModeConst.RelativeToLiquidPosition;

        //            for (var k = 0; k < usedTips.MaxTip(usedAspCavities); k++)
        //            {
        //                //usedSource[k].Liquid.Volume = aspLiquid.VolArr[k];
        //                usedAspCavities[k].Liquid = new SLiquid("sample", aspLiquid2.VolArr[k], new SLiquidState());
        //                aspLiquid2.ZOffset[k] = -2;
        //            }

        //            PipMethods.Aspirate(usedTips, usedAspCavities, sysAir, aspLiquid2, transAir, spit, mixPar, detPar);

        //            //DetPar.UseLiquidDetection = false;
        //            //DetPar.UseTracking = false;
        //            //DetPar.LimitVolumeToPickable = false;
        //            //SPipettingPar aspLiquid3 = Workbench.GetAspVolumePar(150, vb);
        //            //PipMethods.Aspirate(UsedTips, usedSource, SysAir, aspLiquid3, TransAir, Spit, MixPar, DetPar);
        //            //刷新界面
        //            RefreshUiEvent(RdAction.Waste, GetNapId(usedAspCavities), GetWasteCells(usedAspCavities));
        //        }
        //        // }
        //        else
        //        {
        //            detPar.UseLiquidDetection = true;
        //            detPar.UseTracking = true;
        //            detPar.LimitVolumeToPickable = false;

        //            // Aspirate
        //            PipMethods.Aspirate(usedTips, usedAspCavities, sysAir, aspLiquid, transAir, spit, mixPar, detPar);

        //            RefreshUiEvent(RdAction.Aspirate, GetCavityPos(usedAspCavities), GetVolume(usedAspCavities));
        //                //吸液后刷新界面
        //        }

        //        #endregion

        //        #region 走折线

        //        //Z轴上升到安全距离
        //        PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

                
        //        switch (usedTips.NoOfUsedTips)
        //        {
        //            case 3:
        //                tempY[0] = tempy - 17.6;
        //                tempY[1] = tempy - 8.8;
        //                tempY[2] = tempy;
        //                break;
        //            case 2:
        //                tempY[0] = tempy - 17.6;
        //                tempY[1] = tempy - 8.8;
        //                tempY[2] = maxY[2];
        //                break;
        //            case 1:
        //                tempY[0] = tempy - 17.6;
        //                tempY[1] = maxY[1];
        //                tempY[2] = maxY[2];
        //                break;
        //        }

        //        if (dspCavities[0].Name == "RD_WashStation2:RD_WashStation_Cavity1")
        //        {
        //            if (usedAspCavities[0].ParentItem.CurrentSlot.Name == "RD_Mag1:RD_Mag_CupSlot4")
        //            {
        //                //TODO
        //            }
        //            else
        //            {
        //                //全都向右走
        //                double x = PipMethods.Arm.XWorktablePosition + 17.0/2; //当前位置+半个六联排的宽度；17=一个六联排的宽度

        //                double y0 = PipMethods.Arm.YZDevices[0].YWorktablePosition;
        //                double y1 = PipMethods.Arm.YZDevices[1].YWorktablePosition;
        //                double y2 = PipMethods.Arm.YZDevices[2].YWorktablePosition;

        //                double[] y = new double[] {y0, y1, y2};
        //                Robot.GetArm(0).NoTravelMove = true;
        //                PipMethods.MoveXY(x, y);

        //                double z = aspCavities[0].FirstPos.Z - CavityTravelHeight - 3;
        //                PipMethods.BlockMoveZ(usedTips, z, 0, 0);
        //                PipMethods.MoveXY(x, maxY); //移动至六联排左侧Y方向最大值位置

        //                IItem washStation2 = Workbench.Layout.GetItemByName("RD_WashStation2"); //洗站为WashStation2；
        //                x = washStation2.Position.X;
        //                PipMethods.MoveXY(x, maxY); //移动至洗站左侧Y方向最大值位置

        //                //目标位置Y
        //                //ICavity washStationCavity =
        //                //    Workbench.Layout.GetCavityByName("RD_WashStation2:RD_WashStation_Cavity1");
        //                //destinationY[0] = (washStationCavity.FirstPos.Y + washStationCavity.LastPos.Y)/2 - 10;
        //                //destinationY[1] = destinationY[0] + 10;
        //                //destinationY[2] = destinationY[0] + 20;

        //                //PipMethods.MoveXY(x, destinationY);
        //                Robot.GetArm(0).NoTravelMove = false;
        //            }
        //        }
        //        else
        //        {
        //            //样本左侧间隙
        //            //double x = (usedSource[(i - 1) * ADP_COUNT].FirstPos.X + usedSource[(i - 1) * ADP_COUNT].LastPos.X) / 2 - 19.85 / 2;//19.85=一排样本架的宽度
        //            double x = PipMethods.Arm.XWorktablePosition - 19.85/2; //19.85=一排样本架的宽度
        //            //移动至样本左侧间隙，Y方向最大值
        //            PipMethods.MoveXY(x, maxY);

        //            //移动至第四个样本架与第一个六联排载架之间，Y方向最大值
        //            double tempX = Workbench.Layout.GetItemByName("RD_CupRack1").Position.X;
        //            PipMethods.MoveXY(tempX, maxY);

        //            if (dspCavities[0].ParentItem.CurrentSlot.Name == "RD_CupRack1:RD_CupRack_CupSlot1")
        //            {
        //                //目标六联排左侧间隙
        //                destinationX = (dspCavities[0].FirstPos.X + dspCavities[0].LastPos.X)/2 - 8.5;
        //            }
        //            else
        //            {
        //                //移动至第四个样本架与第一个六联排载架之间，Y方向在第三个Tip盒和第一个六联排载架之间
        //                PipMethods.MoveXY(tempX, tempY);

        //                //目标六联排左侧间隙
        //                destinationX = (dspCavities[0].FirstPos.X + dspCavities[0].LastPos.X)/2 - 8.5;
        //                //8.5=六联排宽度的一半
        //                //移动至目标六联排左侧，Y方向在第三个Tip盒和第一个六联排载架之间
        //                PipMethods.MoveXY(destinationX, tempY);
        //            }

        //            switch (usedTips.NoOfUsedTips)
        //            {
        //                case 3:
        //                    //目标位置Y
        //                    destinationY[0] = (dspCavities[0].FirstPos.Y + dspCavities[0].LastPos.Y)/2;
        //                    destinationY[1] = (dspCavities[1].FirstPos.Y + dspCavities[1].LastPos.Y)/2;
        //                    destinationY[2] = (dspCavities[2].FirstPos.Y + dspCavities[2].LastPos.Y)/2;
        //                    break;
        //                case 2:
        //                    destinationY[0] = (dspCavities[0].FirstPos.Y + dspCavities[0].LastPos.Y)/2;
        //                    destinationY[1] = (dspCavities[2].FirstPos.Y + dspCavities[1].LastPos.Y)/2;
        //                    destinationY[2] = maxY[2];
        //                    break;
        //                case 1:
        //                    destinationY[0] = (dspCavities[0].FirstPos.Y + dspCavities[0].LastPos.Y)/2;
        //                    destinationY[1] = maxY[1];
        //                    destinationY[2] = maxY[2];
        //                    break;
        //            }
        //            //移动至目标位置左侧
        //            PipMethods.MoveXY(destinationX, destinationY);
        //        }

        //        #endregion

        //        #region // Dispense liquid

        //        // prepare dispense parameters
        //        SPipettingPar dispenseLiquid = Workbench.GetDispVolumePar(volume, vb);

        //        //20170316 zhangjing
        //        //第一次打废液450
        //        if (category == "firstWaste") //20170316
        //        {
        //            dispenseLiquid = Workbench.GetDispVolumePar(350, vb);
        //        }
        //        //第二次打废液700
        //        if (category == "secondWaste") //20170316
        //        {
        //            dispenseLiquid = Workbench.GetDispVolumePar(500, vb);
        //        }
        //        //最后一次打废液600
        //        if (category == "lastWaste") //20170316
        //        {
        //            dispenseLiquid = Workbench.GetDispVolumePar(550, vb);
        //        }

        //        //20170310 zhangjing 
        //        dispenseLiquid += transAir;
        //        // on example we used fix detection/tracking parameters
        //        //DetPar = new SDetectionPar((int) SDetectionPar.ModeMask.UseTracking);
        //        //20170317
        //        detPar.UseLiquidDetection = false;
        //        detPar.UseTracking = false;
        //        detPar.LimitVolumeToPickable = false;
        //        if ((usedTips.NoOfUsedTips != 1) && (usedTips.NoOfUsedTips < dspCavities.Count))
        //        {

        //        }
        //        // Dispense
        //        PipMethods.Dispense(usedTips, dspCavities, dispenseLiquid, mixPar, detPar, transAir);
        //        //如果不是抽废液，打液后更新界面
        //        if (!category.Contains("Waste"))
        //        {
        //            RefreshUiEvent(RdAction.Dispense, GetNapId(dspCavities), GetDspCells(dspCavities));
        //        }

        //        #endregion
        //    }

        //    #region // drop tips

        //    if (usedDt.AnyTipUsed)
        //    {
        //        // We have used disposable tip on the sequence
        //        //SDropStation ds = Workbench.Robot.GetDevice(typeof(SDropStation)) as SDropStation;
        //        SDropStation ds = Workbench.DropStation;
        //        if (ds != null)
        //        {
        //            // valid drop station found 
        //            SDropTipsPar DropTipsPar = new SDropTipsPar();
        //            DropTipsPar.DisplayDropTipsError = true;

        //            #region 走折线

        //            //Z轴上升到安全距离
        //            PipMethods.BlockMoveZ(Workbench.PipMethods.AllTips, SAFE_Z, BLOCK_MODE, ADP_MOVE_SPEED);

        //            double x = PipMethods.Arm.XWorktablePosition + 8.5; //8.5=六联排宽度的一半；
        //            double[] minY = { 0, 9, 18 };

        //            double[] desY = new double[3];
        //            desY[0] = dspCavities[0].ParentItem.CurrentSlot.Position.Y + 101.32 + 50;
        //            //101.32=cupslot长度，50=cupslot向外延伸距离
        //            desY[1] = desY[0] + 9;
        //            desY[2] = desY[1] + 9;
        //            double desX = Workbench.Layout.GetLayoutDeviceByName("DropStation1").Position.X;

        //            if (dspCavities[0].ParentItem.CurrentSlot.Name.Contains("RD_Reader1"))
        //            {
        //                PipMethods.MoveXY(x, minY); //移动至最内侧
        //                PipMethods.MoveXY(desX, minY);
        //            }
        //            else if (dspCavities[0].Name == "RD_WashStation2:RD_WashStation_Cavity1")
        //            {
        //                //TODO
        //            }
        //            else
        //            {
        //                double tempX = Workbench.Layout.GetItemByName("RD_Heating1").Position.X;
        //                double tempy2 = Workbench.Layout.GetItemByName("RD_Heating1").Position.Y;
        //                double[] tempY2 = new double[3];
        //                switch (usedTips.NoOfUsedTips)
        //                {
        //                    case 3:
        //                        tempY2[0] = tempy2 - 17.6;
        //                        tempY2[1] = tempy2 - 8.8;
        //                        tempY2[2] = tempy2;
        //                        break;
        //                    case 2:
        //                        tempY2[0] = tempy2 - 17.6;
        //                        tempY2[1] = tempy2 - 8.8;
        //                        tempY2[2] = maxY[2];
        //                        break;
        //                    case 1:
        //                        tempY2[0] = tempy2 - 17.6;
        //                        tempY2[1] = maxY[1];
        //                        tempY2[2] = maxY[2];
        //                        break;
        //                }

        //                PipMethods.MoveXY(x, tempY); //X移动至六联排右侧，Y移动至六联排载架与Tip头载架之间
        //                PipMethods.MoveXY(tempX, tempY); //X移动至加热模块左侧，Y移动至六联排载架与Tip头载架之间
        //                PipMethods.MoveXY(tempX, tempY2); //X移动至加热模块左侧，Y移动至Reader外侧；
        //                PipMethods.MoveXY(desX, tempY2); //X移动至DropStation左侧，Y移动至Reader外侧；
        //            }

        //            #endregion

        //            PipMethods.DropTips(usedDt, DropTipsPar, ds);
        //        }
        //        else
        //        {
        //            //System.Windows.MessageBox.Show("No valid drop station on DT robot. Tips are not dropped!");
        //            string message = "No valid drop station on DT robot. Tips are not dropped!";
        //            ReportMessageEvent(PopupType.ShowMessage, message, null);
        //        }
        //    }
        //    GC.Collect();

        //    #endregion


        //    //};
        //    //if (Sequence != null)
        //    //{
        //    //    RunSequence.ExecuteSequence(Sequence);
        //    //}
        //}

        #endregion 移液

        //test
    }
}
