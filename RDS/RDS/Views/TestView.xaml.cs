using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RdCore;
using Rendu.RdsModule;
using Rendu.Entity.Reader;
using Rendu.ShareDll;
using Sias.Core;

namespace RDS.Views
{
    /// <summary>
    /// TestView.xaml 的交互逻辑
    /// </summary>
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
        }

        public CRdsModule rdsModule;
        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            if (rdsModule == null)
            {
                rdsModule = new CRdsModule();
            }
            if (!rdsModule.IsInit)
            {
                rdsModule.Init(this);
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!rdsModule.IsInit)
            {
                MessageBox.Show("Please initialize before test.");
                return;
            }
            object retObj = null; //接收到的对象;
            string strFrom = string.Empty;
            Rendu.ShareDll.ErrCode ec = ErrCode.EC_OK;
            // 测试
            ec = rdsModule.Shake(true, 1000, 30, 0);//振动30秒
            retObj = CRdsModule.WaitObject(out strFrom); //接收到的对象;
            ec = rdsModule.Shake(true, 1000, 30, 0);//振动30秒
            retObj = CRdsModule.WaitObject(out strFrom); //接收到的对象;
            ec = rdsModule.ShellSwitch(ShellTYPE.Lamp, 0, true); // 仪器外置上的灯打开
            retObj = CRdsModule.WaitObject(out strFrom); //接收到的对象;

            // 用类的方式,振动30秒

            CShaker objSend = new CShaker(); //发送的对象;
            objSend.bSet = 1;
            objSend.nSpeed = 1000;
            objSend.nDelay = 3;
            objSend.bMustReturn = 1;

            // 接收返回的发命令后的应答CReturn
            ec = CRdsModule.SendObject(objSend, CRdsModule.strRdCanServiceName);
            retObj = CRdsModule.WaitObject(out strFrom); //接收到的对象;
            if (retObj != null)
            {
                System.Diagnostics.Debug.WriteLine(retObj.ToString());
            }
        }
        private Sdk sdk = new Sdk();
        private void BtnCalcNext_Click(object sender, RoutedEventArgs e)
        {
            //sdk.Test();
        }

        private void BtnInit_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessWorkbench.Workbench is ProcessWorkbench)
            {
                ProcessWorkbench.Workbench.RobotConnected = false;
                ProcessWorkbench.Workbench.RobotName = SConfigurationManager.DevicePath + "\\Rendu.Robot.xml";
                ProcessWorkbench.Workbench.RobotConnected = true;
            }
            sdk.Init();
        }

        private void BtnOpenShaker_Click(object sender, RoutedEventArgs e)
        {
            int time = Convert.ToInt32(TxtBoxShakeTime.Text);
            sdk.OpenShaker(time);
        }

        private void BtnCloseShaker_Click(object sender, RoutedEventArgs e)
        {
            sdk.CloseShaker();
        }

        private void BtnSetNaps_Click(object sender, RoutedEventArgs e)
        {
            CSetNaps setNaps=new CSetNaps();
            CNap nap=new CNap();
            CCell cell=new CCell();
            for (int i = 0; i < 6; i++)
            {
                cell.nPos = i;
                cell.strItemName = "UU";
                cell.strEnzymeTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            }
            nap.Cells.Add(cell);
            nap.nID = 1;
            nap.nPos = 1;
            setNaps.Naps.Add(nap);

            object retObj = null; //接收到的对象;
            string strFrom = string.Empty;
            Rendu.ShareDll.ErrCode ec = ErrCode.EC_OK;
            // 接收返回的发命令后的应答CReturn
            ec = CRdsModule.SendObject(setNaps, CRdsModule.strReaderServiceName);
            retObj = CRdsModule.WaitObject(out strFrom); //接收到的对象;
            if (retObj != null)
            {
                System.Diagnostics.Debug.WriteLine(retObj.ToString());
            }
        }
    }
}
