using System;
using System.Text;
using System.Xml;
//---------------------------------------
using Rendu.Entity.Reader;
using Rendu.Serialize;
using Rendu.ShareDll;
 

using System.Runtime.InteropServices;  // DllImport,MarshalAs,GCHandle
namespace Rendu.RdsModule
{

    // 仪器外置上的开关
    // Enum Shell Switch Type:灯,锁,温湿计通道,温湿计的采样隔
    public enum ShellTYPE { Lamp = 0x10, Lock = 0x11, ThyChannel = 0x12, ThyInterval = 0x13 };
    public class CRdsModule
    {
        public static  string strRdsClientName = "Module_RDS_Client";
        public static  string strRdsServiceName = "Module_RDS_Service"; //主程序的服务接收端 ,收荧光数据,温度等
        //  string strReaderClientName = "Module_Reader_Client";
        public static  string strReaderServiceName = "Module_Reader_Service";  //读数模块的服务接收端 
        public static  string strRdCanServiceName = "Module_RdCan_Service";
        public static UInt32 nSendTimeout = 10000;
        public static UInt32 nWaitTimeout = 10000;
        //---------------------------------
    
        private static bool _IsInit = false;// 是否已初始化过
         public   bool IsInit 
        { 
            get   {  return _IsInit; }
        }

        public CRdsModule() // 构造函数
        {
   
        }
        ~CRdsModule() // 析构函数
        {
            UnInit();
        }
        public  void UnInit()
        {
            if (_IsInit)
            {
                ErrCode ret = ErrCode.EC_OK;
                ret = CShareDll.UnRegister(strRdsClientName);
                ret = CShareDll.UnRegister(strRdsServiceName);
                _IsInit = false;
            }
        }

        public bool Init(object objCallback)
        {
            if (_IsInit) return _IsInit;
            //-----------------------------------
            IntPtr intPtr = IntPtr.Zero;
            //GCHandle gcHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
            //intPtr = gcHandle.AddrOfPinnedObject();
            object obj = IntPtr.Zero;
            ErrCode retCode = ErrCode.EC_OK;

            retCode = CShareDll.RegisterClient(strRdsClientName);
            if (retCode != ErrCode.EC_OK) return false;


            retCode = CShareDll.RegisterService(strRdsServiceName, new ReceiveCallback(OnRecive),objCallback);//
        
            if (retCode != ErrCode.EC_OK) return false;
            _IsInit = true;
            return true;

        }

        // 处理收到的数据
        public static int DoRecive(CCommBase objCommRecive, string strReciveName, string strForm, string strTo, Object objCallback)
        {
            //--------------------------------------------
            //根据不同类型处理
            string strText = String.Empty;

            switch (strReciveName)
            {
                case "NapReadCompleted":
                    break;
                case "Return": //Return // 返回类
                    break;
                case "Temprature"://Temprature//温度相关类
                    break;
                case "ThyData": //温湿度返回数据Hygrothermograph
                    break;
                //case "FluroData":  //FluroData//读数结束返回类
                //    break;
                //case "Shaker"://Shaker//控制振动模块类
                //    break;
                //case "SetNaps"://SetNaps//设置六联排孔中已有加酶时间,可以读数
                //    break;
                //case "CloseApp": //CloseApp//关闭对象程序
                //    break;
                //case "Shell": //DoorLock//门锁  
                //    break;
                default:
                    break;
            }
       

            return  0;
        }
        // 需使用静态函数才可以,否则在DLL中回调些函数时,随机几次后,就使用失败了
         public  static int OnRecive(IntPtr pData, UInt32 nLen, UInt32 nUserFlag, string strForm, string strTo, IntPtr  objCallbackIntPtr)
        {
            Object objCallback = null;
            if (objCallbackIntPtr != IntPtr.Zero)
            {
                GCHandle gch = (GCHandle)objCallbackIntPtr; 
                objCallback = gch.Target;
            }

            int nRet = 0;
            // 收到数据,由UT8转为字符串
            byte[] bufRecive = new byte[nLen];
            Marshal.Copy(pData, bufRecive, 0, (int)nLen);
            string strRecive = Encoding.UTF8.GetString(bufRecive) ;
            // string strRecive = Encoding.UTF8.GetString(bufRecive).TrimEnd('\0');

            //收到的数据生成类
            object objRecive = null;

            objRecive = CSerialize.XmlDeserialize(strRecive);
            if (objRecive == null) return 0;
            Type typeRecive = objRecive.GetType();
            bool bIsSub = typeRecive.IsSubclassOf(typeof(CCommBase));//是否为CCommBase子类
            if (!bIsSub) return 0;

            string strType = typeRecive.ToString();
            string strReciveName = strType.Substring(strType.LastIndexOf(".C") + 2);
            //--------------------------------------------
            CCommBase objCommRecive = (CCommBase)objRecive;

             // 在此是静态函数,也可以在些new一个处理方法类,以方便引用实例对象

            nRet = DoRecive(objCommRecive, strReciveName, strForm, strTo, objCallback); 
            //---------------------
            // 返回
            // 如果需要应答
            if (strReciveName == "Return" || objCommRecive.bMustReturn == 0) return nRet;
            CReturn objReturn = new CReturn();         
            objReturn.nResult = 0;
            objReturn.strName = strReciveName;
            ErrCode retCode = SendObject(objReturn, strForm);
            return nRet;
        }
        
        //设置振动模块的开关
        public ErrCode Shake(bool bActive,int nSpeed, int nDelay, int nDirect)
        {
            ErrCode retCode = ErrCode.EC_OK;
            string strTo = strRdCanServiceName;
            //<Shaker  bMustReturn="1"  strCommand="Active" bSet="1" nSpeed="1000" nDelay="50" nDirection="0" />
            //"<Shaker bMustReturn=\"1\"  strCommand=\"Active\" nSpeed=\"1000\" nDelay=\"5\" nDirection=\"0\" />",
            string strSend = String.Format("<Shaker  bMustReturn=\"1\"  strCommand=\"Active\" bSet=\"{0}\"  nSpeed=\"{1}\" nDelay=\"{2}\" nDirection=\"{3}\" />",
                bActive?1:0, nSpeed, nDelay, nDirect);
             UInt32 nUserFlag = 0;
             retCode = CShareDll.SendData(strSend, nUserFlag, strRdsClientName, strTo, nSendTimeout);
             return retCode;           
        }
        // 处置上的开关
        public ErrCode ShellSwitch(ShellTYPE Type , int nChannel, bool bSet)
        {
            ErrCode retCode = ErrCode.EC_OK;
            string strTo = strRdCanServiceName;
            //<Shell  bMustReturn="1"  strCommand="Active" nType="16" bSet="1"  nChannel="0" bSet="1" />
            string strSend = String.Format("<Shell  bMustReturn=\"1\"  strCommand=\"Active\" nType=\"{0}\" nChannel=\"{1}\" bSet=\"{2}\" />",
               (int)Type, nChannel, bSet ? 1 : 0);
            UInt32 nUserFlag = 0;
            retCode = CShareDll.SendData(strSend, nUserFlag, strRdsClientName, strTo, nSendTimeout);
            return retCode;
        }
        // 联接CAN设备:
        //0:CANalyst-II,第1口
        //1:CANalyst-II,第2口
        //2:第1个PeakCan
        //3:第2个PeakCan
        public ErrCode CanIOConnect(bool bConnect,int nCanID = 3)
        {
            ErrCode retCode = ErrCode.EC_OK;
            string strTo = strRdCanServiceName;
            //<CanIO     strCommand="Active" bSet="1"  nID="3" strMemo="第2个PeakCan"/>"
            string strSend = String.Format("<Shell  bMustReturn=\"1\"  strCommand=\"Active\"  bSet=\"{0}\"  nID=\"{1}\" />", bConnect, nCanID);
            UInt32 nUserFlag = 0;
            retCode = CShareDll.SendData(strSend, nUserFlag, strRdsClientName, strTo, nSendTimeout);
            return retCode;
        }


        ////例如设置读数模块的上六联排信息
        //public ErrCode SetNapsToReader()
        //{
        //    ErrCode retCode = ErrCode.EC_OK;
        //    string strTo = strReaderServiceName;
        //    CSetNaps objSend = new CSetNaps();
        //    //objSend.Naps.Add(new CNap());
        //    retCode = SendObject(objSend, strTo);
        //    return retCode;
        //}

         // 发送object到服务端 
         public static ErrCode SendObject(object objSend, string strTo)
        {
             ErrCode retCode = ErrCode.EC_OK;
             string strSend = CSerialize.XmlSerialize(objSend);
                     
             UInt32 nUserFlag = 0;
             retCode = CShareDll.SendData(strSend, nUserFlag, strRdsClientName, strTo, nSendTimeout);

             return retCode;
        }
         // 发送XmlElement到服务端 
         public static ErrCode SendXmlElement(XmlElement xmlElement, string strTo)
         {
             ErrCode retCode = ErrCode.EC_OK;
             string strSend = xmlElement.OuterXml;         
             UInt32 nUserFlag = 0;
             retCode = CShareDll.SendData(strSend, nUserFlag, strRdsClientName, strTo, nSendTimeout);
             return retCode;
         }
         // 等待服务端返回object
         public static object WaitObject(out string strFrom)
        {
            object retObj = null;
            ErrCode retCode = ErrCode.EC_OK;
            string strRecve = string.Empty;
            strFrom = string.Empty;
            UInt32 nUserFlag = 0;
            retCode = CShareDll.WaitData(out strRecve, out nUserFlag, out strFrom, strRdsClientName, nWaitTimeout);
            if (retCode != ErrCode.EC_OK) return retObj;
           retObj =  CSerialize.XmlDeserialize(strRecve);
            return retObj;
        }

        // 等待服务端返回XmlElement
         public static XmlElement WaitXmlElement(out string strFrom)
        {
            XmlElement retXmlElement = null;
            ErrCode retCode = ErrCode.EC_OK;
            string strRecve = string.Empty;
            strFrom = string.Empty;
            UInt32 nUserFlag = 0;
            retCode = CShareDll.WaitData(out strRecve, out nUserFlag, out strFrom, strRdsClientName, nWaitTimeout);
            if (retCode != ErrCode.EC_OK) return retXmlElement;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strRecve);
            retXmlElement = xmlDoc.DocumentElement;
            return retXmlElement;
        }
    }

 
}