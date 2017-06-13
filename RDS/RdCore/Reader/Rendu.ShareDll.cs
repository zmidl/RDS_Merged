using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//----------------------
using System.Security;
using System.Security.Permissions; 
using System.Runtime.InteropServices;  // DllImport,MarshalAs,GCHandle
namespace Rendu.ShareDll
{
    public enum ErrCode
    {
        EC_OK = 0,			// 0.正常,正确
        EC_NoFind = 1,		// 1.没找到对象,没有注册对象
        EC_NoActive = 2,	// 2.有对象,但对象已正常退出
        EC_Abandoned = 3,	// 3.有对象但被废弃,对象已非正常退出,非调用UnRegister
        EC_TimeOut = 4,		// 4.发送或接收数据超时,超过参数dwTimeout指定的时间ms
        EC_RegisteErr = 5,	// 5.注册时错误
        EC_ShareMemoryErr = 6,// 6.共享数据复制时有错,
        EC_UNKNOW = 7		// 7.不知道是啥错误
    }

    public class CShareDll
    {
        static public List<GCHandle> gcHandleLis = new List<GCHandle>();
        public static void  FreeGCHadle()
        {   // 用完在释放gch.Free();
            foreach (GCHandle element in gcHandleLis) element.Free(); 
            gcHandleLis.Clear();
        }
        //注册客户端
        public static ErrCode RegisterClient(string strName)
        {
            IntPtr intPtr = IntPtr.Zero;
            return UnsafeNativeMethods.Register(strName, intPtr, intPtr);
        }
        //注册服务端
        public static ErrCode RegisterService(string strName, ReceiveCallback OnReceive ,object objCallback)
        {
           


            // 使用GCHandleType.Pinned会报错,改用GCHandleType.Normal就不出错了
            // System.ArgumentException”类型的未经处理的异常在 mscorlib.dll 中发生 
            // 其他信息: Object 包含非基元或非直接复制到本机结构中的数据。
            //GC.SuppressFinalize(OnReceive);这个方法告诉CLR不要再调用obj对象的终结器了. 试过但依然会被终结.
            
            //针对委托，可以使用Marshal.GetFunctionPointerForDelegate从delegate封送到非托管函数，或者使用Marshal.GetDelegateForFunctionPointer从非托管函数里面取出。
            //在封送到非托管函数时需要确保GC不要回收委托(需要保持引用但不需要Pinned)。
 
            //C#的对象不能在Native中进行处理，只能保存以待之后再传回给C#调用，保存的方式为C#用GCHandle引用对象，然后把GCHandle转成IntPtr，传给Native作为指针保存
            GCHandle gcHandle = GCHandle.Alloc(OnReceive, GCHandleType.Normal);
            gcHandleLis.Add(gcHandle);

            IntPtr funPtr = Marshal.GetFunctionPointerForDelegate(OnReceive); // 锁定委托函数的指针
            // ReceiveCallback funReceive = (ReceiveCallback) Marshal.GetDelegateForFunctionPointer(funPtr,typeof( ReceiveCallback));这是从函数指针中转换为委托

            IntPtr objIntPtr = IntPtr.Zero;
            if (objCallback != null)
            {                 
                gcHandle = GCHandle.Alloc(objCallback, GCHandleType.Normal);
                objIntPtr = (IntPtr)gcHandle;
                //gcHandle = (GCHandle)objIntPtr;
            }
            return UnsafeNativeMethods.Register(strName, funPtr, objIntPtr);
        }
        // 取消注册
        public static ErrCode UnRegister(string strName)
        {
            return UnsafeNativeMethods.UnRegister(strName);
        }

        // 发送数据
        public static ErrCode SendData(string strData, UInt32 nUserFlag, string pwszForm, string pwszTo, UInt32 dwTimeout)
        {
            Byte[] pData = Encoding.UTF8.GetBytes(strData); // 转成UTF8格式的Byte数组
            UInt32 nLen = (UInt32)pData.GetLength(0); // 
            //  UInt32 nLen = (UInt32 ) strData.Length + 1;
            ErrCode nRet = UnsafeNativeMethods.SendData(pData, nLen, nUserFlag, pwszForm, pwszTo, dwTimeout);

            return nRet;
        }

        public static ErrCode WaitData(out string strData, out UInt32 nUserFlag, out string strForm, string pwszTo, UInt32 dwTimeout)
        {
            ErrCode retCode = ErrCode.EC_UNKNOW;
            nUserFlag = 0;
            strData = string.Empty;
            strForm = string.Empty;

            try
            {

                IntPtr pData = IntPtr.Zero;
                IntPtr pwszForm = IntPtr.Zero;
                UInt32 nLen = 0;
                retCode = UnsafeNativeMethods.WaitData(out pData, out nLen, out nUserFlag, out pwszForm, pwszTo, dwTimeout);
                if (retCode != ErrCode.EC_OK) return retCode;
                strForm = Marshal.PtrToStringUni(pwszForm);// Unicode字符串
                //strData = Marshal.PtrToStringAnsi(pData); // 这种方法没有UTF8转成格挡String
                //------------------------------
                // UTF8转成Uniode格式的Byte数组
                byte[] bufRecive = new byte[nLen];
                Marshal.Copy(pData, bufRecive, 0, (int)nLen);
                strData = Encoding.UTF8.GetString(bufRecive); //按照UTF8的编码方式得到字符串,原发送的数据已不含字符串尾的'\0'
                //strData = Encoding.UTF8.GetString(bufRecive).TrimEnd('\0'); //按照UTF8的编码方式得到字符串,并去掉末尾的'\0'
                UnsafeNativeMethods.BufferFree(pData, pwszForm);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return retCode;
        }
    }

    #region   ShareDLL动态库的引用
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    //public delegate int ReceiveCallback(IntPtr pData, UInt32 nLen, UInt32 nUserFlag,string form, string to, IntPtr obj);
    public delegate int ReceiveCallback(IntPtr pData, UInt32 nLen, UInt32 nUserFlag, [MarshalAs(UnmanagedType.LPWStr)]string form, [MarshalAs(UnmanagedType.LPWStr)] string to, [MarshalAs(UnmanagedType.SysInt)]IntPtr obj);

    /*

    typedef int  (CALLBACK* FUN_CallBack)(LPBYTE pData,DWORD nLen,DWORD nUserFlag,LPCTSTR pwszForm,LPCTSTR pwszTo,LPVOID pParam) ;  //服务端收数据后的回高通知
    EXPORT_DLL int  CALLBACK SendData(LPBYTE pData,DWORD nLen,DWORD nUserFlag,LPCTSTR pwszForm,LPCTSTR pwszTo ,DWORD dwTimeout);//发送数据pData,从pwszFrom,发给pwszTo
    EXPORT_DLL int  CALLBACK WaitData(LPBYTE *ppData,DWORD *pnLen,DWORD *pnUserFlag,LPTSTR *ppwszForm,LPCTSTR pwszTo,DWORD dwTimeout); // 等待数据,从pwszFrom,发给pwszTo
    EXPORT_DLL int  CALLBACK Register(LPCTSTR pwszName,FUN_CallBack  ReciveDataCallback,LPVOID pParam);//注册对象
    EXPORT_DLL int  CALLBACK UnRegister(LPCTSTR pwszName);//反注册对象,只有自己创建的对象才需要反注册,
    EXPORT_DLL int  CALLBACK IsActive(LPCTSTR pwszName);//是否是活动的对象,已在SendData,WaitData中增加判断,可以不关注,监控模块的程序才需要用到.
    EXPORT_DLL void CALLBACK BufferFree(LPBYTE ppData,LPCTSTR ppwszForm);//只有客户端收到数据用完后才需释放内存 
     */
    //回调函数声明
    [SuppressUnmanagedCodeSecurityAttribute]
    internal static class UnsafeNativeMethods
    {

#if DEBUG
        public const string ShareDLL_PATH = @"\Apps\Dlls\ShareDll.dll";
#else
                public const string ShareDLL_PATH = @"ShareDll.dll";//@"\Apps\Dlls\ShareDll.dll";
#endif

        //注册对象
        [DllImport(ShareDLL_PATH, EntryPoint = "Register", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U4)]
        public extern static ErrCode Register([MarshalAs(UnmanagedType.LPWStr)]string pwszName, IntPtr OnReceive, [MarshalAs(UnmanagedType.SysInt)]IntPtr hObj);

        //public extern static ErrCode Register([MarshalAs(UnmanagedType.LPWStr)]string pwszName,
        //    [MarshalAs(UnmanagedType.FunctionPtr)]ReceiveCallback OnReceive,
        //    [MarshalAs(UnmanagedType.SysInt)]IntPtr hObj);

        //发送数据pData,从pwszFrom,发给pwszTo
        [DllImport(ShareDLL_PATH, EntryPoint = "SendData", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U4)]
        public extern static ErrCode SendData(Byte[] pData,//[MarshalAs(UnmanagedType.LPStr)]string pData,
            UInt32 nLen, UInt32 nUserFlag,
            [MarshalAs(UnmanagedType.LPWStr)]string pwszForm,
            [MarshalAs(UnmanagedType.LPWStr)]string pwszTo,
             UInt32 dwTimeout);

        // 等待数据,从pwszFrom,发给pwszTo
        // 因字符串是从DLL中生成的,用string, StringBuilder等会报错误.{"尝试读取或写入受保护的内存。这通常指示其他内存已损坏。"  
        // 现改为IntPtr,  使用 Marshal.PtrToStringAnsi(IntPtr pData);
        // [DllImport(ShareDLL_PATH, EntryPoint = "WaitData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //public extern static int WaitData([MarshalAs(UnmanagedType.LPStr)]out string pData,
        //     out UInt32 nLen, out UInt32 nUserFlag,
        //     [MarshalAs(UnmanagedType.LPWStr)]  out string pwszForm,
        //     [MarshalAs(UnmanagedType.LPWStr)]string pwszTo,
        //      UInt32 dwTimeout);

        [DllImport(ShareDLL_PATH, EntryPoint = "WaitData", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U4)]
        public extern static ErrCode WaitData(out IntPtr pData,
             out UInt32 nLen, out UInt32 nUserFlag,
             out IntPtr pwszForm,
             [MarshalAs(UnmanagedType.LPWStr)]string pwszTo,
              UInt32 dwTimeout);
        //反注册对象,只有自己创建的对象才需要反注册,
        [DllImport(ShareDLL_PATH, EntryPoint = "UnRegister", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U4)]
        public extern static ErrCode UnRegister([MarshalAs(UnmanagedType.LPWStr)]string pwszName);

        //是否是活动的对象,已在SendData,WaitData中增加判断,可以不关注,监控模块的程序才需要用到.
        [DllImport(ShareDLL_PATH, EntryPoint = "IsActive", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public extern static bool IsActive([MarshalAs(UnmanagedType.LPWStr)]string pwszName);

        ////只有客户端收到数据用完后才需释放内存 
        //[DllImport(ShareDLL_PATH, EntryPoint = "BufferFree", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        //public extern static int BufferFree([MarshalAs(UnmanagedType.LPStr)]string pData, [MarshalAs(UnmanagedType.LPWStr)]string pwszName);
        //只有客户端收到数据用完后才需释放内存 
        [DllImport(ShareDLL_PATH, EntryPoint = "BufferFree", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U4)]
        public extern static void BufferFree(IntPtr pData, IntPtr pwszName);
    }
     #endregion   

    public static class StructPtrConverter
    {
        /// <summary>  
        /// 由结构体转换为byte数组  
        /// </summary>  
        public static byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>  
        /// 由byte数组转换为结构体  
        /// </summary>  
        public static T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }
    }  
}