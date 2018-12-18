using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
* Notice
* This code is free ,you can use it in you project,but dont remove my declaration 
* author --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService.OpensslWrapper
{
    class OpenSslServerStream : Base
    {
        const int stream_buf_len = 4096; //max ip package is 4096,so...
        private Socket tcpSocket;
        private SslContext sslCtx;
        private Ssl ssl;
        private byte[] streamBuf;
        internal OpenSslServerStream(ref Socket tcpSocket) : base(IntPtr.Zero, false)
        {
            this.tcpSocket = tcpSocket;
            this.sslCtx = SslContext.GetInst(); 
            streamBuf = new byte[stream_buf_len];
            try
            {
                InitSSL();
            }catch(Exception e)
            {
                System.Diagnostics.Debug.Write("OpenSslServerStream fail:" + e.Message + "\n");
                Log.Logger.Instance.WriteException(e);
                Close();
            }
        }

        public int Read(byte [] buf)
        {
            //int nRead = ssl.Read(buf, buf.Length);
            return ssl.Read(buf, buf.Length); 
        }

        public string Read()
        {
            if (tcpSocket == null ||ssl == null)
                return "";
           
            int nRead = 0;
            string str = "";
            while ((nRead =  ssl.Read(streamBuf, stream_buf_len))> 0)
            {
                System.Diagnostics.Debug.Write(string.Format("***nRead :{0} \n",nRead));
                str += System.Text.Encoding.Default.GetString(streamBuf);
            }
            return str;
        }
      
        public void WriteString(string strData)
        {
            byte [] bytes = System.Text.Encoding.Default.GetBytes(strData);
            Write(bytes, bytes.Length);
            bytes = null;
        }

        public void Write(byte[] Data ,int len)
        {
            //throw new NotImplementedException();
            int ret =  ssl.Write(Data, len);
            if (ret != len)
                throw new Exception("ssl write data failed!\n");
        }

        public void Close()
        {
            if(tcpSocket != null)
                tcpSocket.Close();
          
            if(ssl!=null)
                 ssl.Shutdown();

            ssl = null;
            tcpSocket = null;
        }

        private void InitSSL()
        {
            
            //建立ssl对象，设定fd
            ssl = new Ssl(this.sslCtx);
            int ret = ssl.SetFd(tcpSocket.Handle);
            if (ret != 1)
                throw new Exception("SSL_set_fd failed!\n");
            
            // begin to accept...
            ssl.SetAcceptState();

            ret = ssl.Accept();
            int ntry = 10;
            while (ret != 1 && --ntry >= 0)
            {
                System.Diagnostics.Debug.WriteLine("***try to accept...",ntry);
                Thread.Sleep(500);
                ret = ssl.Accept();
            }
            if (ret != 1)
                throw new Exception("SSL_accept failed!\n");

           
        }


        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}
