using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//----------------------
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace Rendu.Entity.Reader
{

    #region  上位机与读数模块的通信类
    //Module_RDS与Module_Reader通讯协议
    //主程序端
    //Module_RDS_Client
    //Module_RDS_Service
  
    //读数端
    //Module_Reader_Client
    //Module_Reader_Service
    //XML编码格式:UTF8

    // 基本上都用到bMustReturn
    public class CCommBase
    {
        [XmlAttribute("bMustReturn")]
        public int bMustReturn { get; set; }
        public CCommBase()
        {
            bMustReturn = 1;
        }
    }

    //<Return strName="Shaker"   nResult="0"  />
    [XmlRoot("Return")]
    public class CReturn : CCommBase
    {
        [XmlAttribute("strName")]
        public string strName { get; set; }
        [XmlAttribute("nResult")]
        public int nResult { get; set; }
        public CReturn()
        {
            bMustReturn = 0;
        }
    }
    // 六联排读完后的事件,为及时搬运,因计算结果再返回会有有延时,时间不定
    //<NapReadCompleted  bMustReturn="1" >	
    //    <Nap nID="1"   nPos="1" / >		
    //    <Nap nID="2"   nPos="1" / >
    // </NapReadCompleted>
    [XmlRoot("NapReadCompleted")]
    public class CNapReadCompleted : CFluroData
    {

    }
    //  设置六联排已加酶,可以多个六联排
    //<SetNaps   bMustReturn="1" >	
    //    <Nap nID="1"   nCurrentPos="1"  >		
    //       <Cell  nPos="1" strItemName="UU" strEnzymeTime="2017-05-09 23:05:57" / >
    // </Nap>
    //</SetNaps>
    //结构与FluroData一样,可以直接继续承
    [XmlRoot("SetNaps")]
    public class CSetNaps : CFluroData
    {

    }
    //仪器外壳设备,以nType定义枚常量,区分是什么设备,
    //<Shell  strCommand="Active" nType="16" nChannel="0" bSet="1" />

    [XmlRoot("Shell")]
    public class CShell : CCommBase
    {
        [XmlAttribute("strCommand")]
        public string strCommand { get; set; }
        [XmlAttribute("nType")]
        public int nType { get; set; }
        [XmlAttribute("nChannel")]
        public int nChannel { get; set; }
        [XmlAttribute("bSet")]
        public int bSet { get; set; }
        public CShell()
        {
            strCommand = "Active";
            bSet = 1;
        }
    }
    //关闭对象程序
    //<CloseApp    /> 
    [XmlRoot("CloseApp")]
    public class CCloseApp : CCommBase 
    {
    }
 
    //振动模块
 
    //<Shaker  bMustReturn="1" strCommand="Active" bSet="1" nSpeed="1000" nDelay="50" nDirection="0" />
    [XmlRoot("Shaker")]
    public class CShaker : CCommBase 
    {

        [XmlAttribute("strCommand")]
        public string strCommand { get; set; }
        [XmlAttribute("bSet")]
        public int bSet { get; set; }
        [XmlAttribute("nSpeed")]
        public int nSpeed { get; set; }
        [XmlAttribute("nDelay")]
        public int nDelay { get; set; }
        [XmlAttribute("nDirection")]
        public int nDirection { get; set; }
        public CShaker()
        {
            strCommand = "Active";
            bSet = 1;
        }
    }
    //以nDeviceID定义枚常量,区分是什么模块,
    //<Temprature  nDeviceID="129" nTempr="420"  nTargetTempr="420" nInTempr="300" /> // 发命令读读数模块温度时返回
    //<Temprature  nDeviceID="129" nTempr="420"  nInTempr="300" />  // 返回荧光数据时返回
    //<Temprature  nDeviceID="130" nTempr="420"  nInTempr="300" />  // 加热模块返回
 
    [XmlRoot("Temprature")]
    public class CTemprature : CCommBase 
    {
        [XmlAttribute("nDeviceID")]
        public int nDeviceID { get; set; }// CanID:0x81读数模块, CanID:0x82温浴模块温度
        [XmlAttribute("nTempr")]
        public int nTempr { get; set; }// 读数模块/温浴模块温度,如果是发送则是设置温度
        [XmlAttribute("nTargetTempr")]
        public int nTargetTempr { get; set; }// 设置或返回读数模块的目标温度        
        [XmlAttribute("nInTempr")]
        public int nInTempr { get; set; } // 读数模块环境温度
 
    }



   ////读数模板返回读数结果,加酶时间为时间字符串
    //<FluroData  bMustReturn="1">
   //  <Nap nID="1"   nCurrentPos="1"  >		
   //   <Cell  nPos="1" strItemName="UU" strEnzymeTime="2017-05-09 23:05:57" >
   //       <Result nCycleCount="40"  fCt="23.5" fConc="8976516978"  nResult="" >
   //          <Channel1  strTime="" strRaw="" strValue="" />
   //         <Channel2  strTime="" strRaw="" strValue="" />      
   //      </Result>  
   //     </Cell>
   // </Nap>	
    // <Nap strMemo="很多个" />	 
   //</Event>
    [XmlRoot("FluroData")]
    public class CFluroData : CCommBase 
    {
        [XmlElement("Nap")]
        public List<CNap> Naps ;
        public CFluroData()
        {
            Naps = new List<CNap>(); 
        }
    }

    //温湿度返回数据Hygrothermograph Data
    //<ThyData  bMustReturn="0" nChannel="1" nTemprature="2505"  nHumidity="7821">
    [XmlRoot("ThyData")]
    public class CThyData : CCommBase
    {
        [XmlAttribute("nChannel")]
        public int nChannel { get; set; }
        [XmlAttribute("nTemprature")] //温度值单位0.01℃
        public int nTemprature { get; set; }
        [XmlAttribute("nHumidity")]
        public int nHumidity { get; set; } // 湿度值单位0.01%RH

    }

    #region   FluroData的子类
    //[XmlRoot("Nap")]
    public class CNap
    {

        [XmlAttribute("nID")]
        public int nID { get; set; }
        [XmlAttribute("nPos")]
        public int nPos { get; set; }
        [XmlElement("Cell")]
        public List<CCell> Cells { get; set; }
        public CNap()
        {
            Cells = new List<CCell>(); 
        }
    }
    public class CCell
    {

        [XmlAttribute("nPos")]
        public int nPos { get; set; }
        [XmlAttribute("strItemName")]
        public string strItemName { get; set; }
        [XmlAttribute("strEnzymeTime")]
        public string strEnzymeTime { get; set; } //加酶时间为时间字符串,如果为DateTime,转类时需单独处理会增加代码量
        [XmlElement("Result")]
        public CResult Result { get; set; } 
    }


    public class CResult
    {
        [XmlAttribute("nCycleCount")]
        public int nCycleCount { get; set; }
        [XmlAttribute("dCt")]
        public double dCt { get; set; }
        [XmlAttribute("dConc")] 
        public double dConc { get; set; }
        [XmlAttribute("nResult")]
        public int nResult { get; set; }

        [XmlElement("Channel")]
        public List<CChannel> Channels { get; set; }
        public CResult()
        {
            Channels = new List<CChannel>(); 
        }

        //[XmlElement("Channel1")]
        //public CChannel Channel1 { get; set; }
        //[XmlElement("Channel2")]
        //public CChannel Channel2 { get; set; }

 
    }
    public class CChannel
    {
        [XmlAttribute("nPos")]
        public int nPos { get; set; }
       [XmlAttribute("strTime")]
        public string strTime { get; set; }
        [XmlAttribute("strRaw")]
        public string strRaw { get; set; }
        [XmlAttribute("strValue")]
        public string strValue { get; set; }
    }
        #endregion  
        #endregion  
}


/*
 <!-- 
Module_RDS与Module_Reader通讯协议
主程序端
Module_RDS_Client
Module_RDS_Service
  
读数端
Module_Reader_Client
Module_Reader_Service
XML编码格式:UTF8
 -->
 <ModuleComm>
   <!-- 设置六联排已加酶,可以多个六联排 -->
   <SetNaps  bMustReturn="1" >	
       <Nap nID="1"   nCurrentPos="1"  >		
          <Cell  nPos="1" strItemName="UU" strEnzymeTime="2017-05-09 23:05:57" />
       </Nap>
       <Nap />
   </SetNaps>	
   <Return strName="SetNaps" nResult="0"  />
   <!-- 读数模板返回读数结果 -->
   <FluroData bMustReturn="1">
      <Nap nID="1"   nCurrentPos="1"  >		
      <Cell  nPos="1" strItemName="UU" strEnzymeTime="2017-05-09 23:05:57" >
          <Result nCycleCount="40"  fCt="23.5" fConc="8976516978"  nResult="0" >
               <Channel1 
                 strTime="36,76,116,157,197,238,278,318,359,399,440,480,520,561,601,642,682,722,763,803,844,884,924,965,1005,1046,1086,1126,1167,1207,1248,1288,1328,1369,1409,1450,1490,1531,1571,1611,1652,1692,1733,1773,1813,1854,1894,1935,1975,2015,2056,2096,2137,2177,2217,2258,2298,2339,2379,2419,2460,2500"
                 strRaw="596,586,617,604,632,580,597,678,760,956,1102,1281,1439,1530,1626,1728,1790,1854,1908,1947,1964,2037,2060,2089,2088,2096,2122,2131,2154,2152,2153,2187,2231,2231,2230,2272,2323,2372,2376,2376"
                 strValue="596,586,617,604,632,580,597,678,760,956,1102,1281,1439,1530,1626,1728,1790,1854,1908,1947,1964,2037,2060,2089,2088,2096,2122,2131,2154,2152,2153,2187,2231,2231,2230,2272,2323,2372,2376,2376"
               />
             <Channel2 
                 strTime="36,76,116,157,197,238,278,318,359,399,440,480,520,561,601,642,682,722,763,803,844,884,924,965,1005,1046,1086,1126,1167,1207,1248,1288,1328,1369,1409,1450,1490,1531,1571,1611,1652,1692,1733,1773,1813,1854,1894,1935,1975,2015,2056,2096,2137,2177,2217,2258,2298,2339,2379,2419,2460,2500"
                 strRaw="591,567,579,603,576,592,572,620,648,796,936,1127,1286,1374,1522,1618,1688,1768,1845,1917,1929,1991,2021,2033,2062,2072,2110,2104,2158,2150,2176,2166,2165,2155,2176,2197,2222,2256,2289,2334"
                 strValue="591,567,579,603,576,592,572,620,648,796,936,1127,1286,1374,1522,1618,1688,1768,1845,1917,1929,1991,2021,2033,2062,2072,2110,2104,2158,2150,2176,2166,2165,2155,2176,2197,2222,2256,2289,2334"
               />           
         </Result>  
        </Cell>
       </Nap>	
       <Nap />       
   </FluroData>
   <Return strName="FluroData" nResult="0"  />
	<!-- 振动 -->
	<Shaker strName ="Shaker"  bMustReturn="1" strCommand="Active" nSpeed="1000" nDelay="50" nDirection="0" />
	<Return strName="Shaker"   nResult="0"  />
	<!-- 温度	 -->	
	
	<Temprature  bMustReturn="1"    nReaderTempr="420" nReaderInTempr="300" nHeatingTempr="650"/>
	<Return strName="Temprature"   nResult="0"  />
	<!-- 锁 -->
	<DoorLock   nActive="0" />
	<!-- 关闭对象程序 -->
	<CloseApp    />     
 	<!-- 温湿度返回数据Hygrothermograph ,温湿度的单位0.01-->
    <Thermohygrograph nChannel="1" nTemprature="2560" nHumidity="7643" />
</ModuleComm>
*/