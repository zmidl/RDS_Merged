using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//----------------------
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Rendu.Entity.Reader;
using System.Reflection;
namespace Rendu.Serialize
{

    public class CSerialize
    {

        //定义有哪些类需要进行字符串到类的转换
        static public Dictionary<string, Type> mapNameType;
        static public void   InitNameType()
        {
            //获取命令空间中所有类,
            // Assembly assembly = Assembly.Load("Rendu.Module.Reader")// 外部DLL中
            // Assembly assembly =Assembly.GetExecutingAssembly();//当前程序中
            //Type[] types =assembly.GetTypes();
            //没必要获取所有类,对上位机,就这几个
            if (mapNameType == null)
            {
                mapNameType = new Dictionary<string, Type>();
                mapNameType.Add("Return", typeof(CReturn)); // 返回类
                mapNameType.Add("Temprature", typeof(CTemprature));//温度相关类
                mapNameType.Add("FluroData", typeof(CFluroData));//读数结束返回类
                mapNameType.Add("ThyData", typeof(CThyData));//温湿度的数据 
                mapNameType.Add("NapReadCompleted", typeof(CNapReadCompleted));//六联排读完后的事件,为及时搬运,因计算结果再返回会有有延时,时间不定  
                mapNameType.Add("Shaker", typeof(CShaker));//控制振动模块类
                mapNameType.Add("SetNaps", typeof(CSetNaps));//设置六联排孔中已有加酶时间,可以读数
                mapNameType.Add("CloseApp", typeof(CCloseApp));//关闭对象程序
                mapNameType.Add("Shell", typeof(CShell));//仪器外罩     
                
            } 
             
        }
        //要生成什么类
        public static Type FindType(string xmlString)
        {
            Type type = null;
            if (mapNameType == null)
            {
                InitNameType();
            }
            foreach (var dic in mapNameType)
            {
                if (xmlString.IndexOf(dic.Key) >= 0)
                {
                    type = dic.Value;
                    break;
                }
            }
            return type;
        }
        // 从XmlElement中生成object
        public static object XmlDeserialize(XmlElement xmlElement)
        {
            //要生成什么类
            Type type = FindType(xmlElement.Name);
            if (type == null)
            {
                string strMsg = string.Format("FindType({0})没找到这种类研", xmlElement.Name);
                System.Diagnostics.Debug.WriteLine(strMsg);
                return null;
            }
            Object obj = null;//返回对象
            //----------------------------------
            try
            {
                XmlNodeReader nodeRead = new XmlNodeReader(xmlElement);
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                obj = xmlSerializer.Deserialize(nodeRead);
            }
            catch (Exception e)
            {
                // 输出误信息,以便于调试
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return obj;
        }
        // 从string中生成object
        public static object XmlDeserialize(string xmlString)
        {
  
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                
                xmlDoc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                // 输出误信息,以便于调试
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
            return XmlDeserialize(xmlDoc.DocumentElement); // 调用从XmlElement中生成object函数
 
        }

 
        // 从object中生成utf8格式的string, 
        public static string XmlSerialize(System.Object obj)
        {
            // 去掉长长的xmlns:xsi,xmlns:xsd
            // xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            string xmlString = string.Empty;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;// //添加开头的<?xml version="1.0" encoding="utf-8"?>
            settings.OmitXmlDeclaration = false; //去除开头的<?xml version="1.0" encoding="utf-8"?>
            settings.Indent = true;//换行，缩进            
            //settings.IndentChars = "\t";

            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriter writer = XmlWriter.Create(ms, settings);
                //Type type = obj.GetType();
                XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                //--------------------------------          
                xmlSerializer.Serialize(ms, obj, ns);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
                xmlString = xmlString.Replace("<?xml version=\"1.0\"?>\r\n", "");
            }

            return xmlString;
        }

        /// <summary>  
        /// XML String 反序列化成对象  
        /// </summary>  
        public static T XmlDeserialize<T>(string xmlString)
        {
            T t = default(T);
            try
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
                //{
                XmlReader xmlReader = XmlReader.Create(xmlString);
                //  XmlReader xmlReader = XmlReader.Create(xmlStream);
                // {
                Object obj = xmlSerializer.Deserialize(xmlReader);
                t = (T)obj;
                //  }
                //}
            }
            catch (Exception e)
            {

                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return t;
        }
   
  
        //public static object XmlDeserialize(string xmlString)
        //{

        //    //要生成什么类
        //    Type type = FindType(ref xmlString);          
        //    if (type == null) return null;
        //    // Type type = Type.GetType("EntityModule.Module_Reader.Return");
        //    //返回对象
        //    Object obj = null;
        //    XmlSerializer xmlSerializer = new XmlSerializer(type);


        //    byte [] byteUtf8 =   Encoding.Unicode.GetBytes(xmlString);
        //   using (Stream xmlStream = new MemoryStream(byteUtf8))
        //    {

        //       XmlReader xmlReader = XmlReader.Create(xmlStream);
        //        try
        //        {
        //            obj = xmlSerializer.Deserialize(xmlReader);
        //        }
        //        catch (Exception e)
        //        {
        //            // 输出误信息,以便于调试
        //            System.Diagnostics.Debug.WriteLine(e.ToString());   
        //        }
        //    }
        //    return obj;
        //}
        //public static string GetMethodInfo()
        //{
        //    string str = "";  
        //    //取得当前方法命名空间
        //    str += "命名空间名:"+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "\n";
        //    //取得当前方法类全名 包括命名空间
        //    str += "类名:"+System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + "\n";
        //    //取得当前方法名
        //    str += "方法名:"+System.Reflection.MethodBase.GetCurrentMethod().Name + "\n";
        //    str += "\n";     
 
        //    System.Diagnostics.StackTrace ss = new System.Diagnostics.StackTrace(true);
        //    MethodBase mb = ss.GetFrame(1).GetMethod();
        //    //取得父方法命名空间
        //    str += mb.DeclaringType.Namespace + "\n";
        //    //取得父方法类名
        //    str += mb.DeclaringType.Name + "\n";
        //    //取得父方法类全名
        //    str += mb.DeclaringType.FullName + "\n";
        //    //取得父方法名
        //    str += mb.Name + "\n";

        //    //System.Diagnostics.StackFrame sf = ss.GetFrame(i);                    
        //    //sf.GetMethod() ;//得到错误的方法
        //    //sf.GetFileName();  //得到错误的文件名
        //    //sf.GetFileLineNumber();//得到文件错误的行号
        //    //sf.GetFileColumnNumber();//得到错误的列
                   
        //    return str;
        //}
    }
 

}
