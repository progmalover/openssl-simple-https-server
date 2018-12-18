using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using VEWebUrlRedirectService.OpensslWrapper;
/*
* Notice
* This code is free ,you can use it in you project,but dont remove my declaration 
* author --maxwell.z
* 2018.12
* ref:https://www.w3.org/Protocols/rfc2616/rfc2616-sec6.html#sec6
*/

namespace VEWebUrlRedirectService.https
{
    class HttpResponse
    {
        public HttpResponse(OpenSslServerStream stream,ref HttpRequest Request)
        {
            mStream = stream;
            mStatus = 0;
            mRefRequest = Request;
           // bWriteWithHeader = true;
            bDataIsUtf8Text = false;
            //建立缓冲文件buffer
            //1000k缓冲
            string tempFile = Path.GetTempFileName();
            mBufSream = new BufferedStream(new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite), 1024000);
        }

        public OpenSslServerStream GetStream()
        {
            return mStream;
        }
        
        public void Write(byte []bytes,int len)
        {
            mBufSream.Write(bytes,0, len);
        }
      
        //string 类型默认使用utf8
        public void WriteString(string strData)
        {
            bDataIsUtf8Text = true;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strData);
            Write(bytes, bytes.Length);
        }
        private string CreateHeader()
        {
            mStrHeader = CreateStatusLine();
            mStrHeader += AppendHeader();
            mStrHeader += AppendMessageBody();

            return mStrHeader;
        }

        private string AppendMessageBody()
        {
            if (mBufSream == null || mBufSream.Length <= 0)
                return "Content-length:0\r\n\r\n";
            else
                return string.Format("Content-length:{0}\r\n\r\n", mBufSream.Length);
        }

        private string AppendHeader()
        {
            string strHeader = "";
            strHeader += "Server:VEVRHttpsServer\r\n";
            strHeader += "Data:" + DateTime.Now.ToLongDateString() + "\r\n";

            //i think the webbrowser knows what type of file that it wanted...so just add simple text/html head.
            if(bDataIsUtf8Text)
                 strHeader += "Content-Type:text/html;charset=utf-8\r\n";  // "Content-Type:" + this.mRefRequest.mHeaders["Accept"];
            
            if (mRefRequest.mIsKeepAlive)
                strHeader += "Connection:keep-alive\r\n";
            strHeader += "Pragma:no-cache\r\n";
            if(mRefRequest.mCookie!= null && mRefRequest.mCookie.Length > 0)
                strHeader += "Cookie:" + mRefRequest.mCookie + "\r\n";
            return strHeader;
        }

        private string CreateStatusLine()
        {
            //throw new NotImplementedException();
            if (mStatus == 0)
                return "http1.1 200 ok\r\n";
            string strErr = string.Format("http1.1 {0} Internal-Server-Error\r\n", mStatus);
            return strErr;
        }

        public void SetStatus(int nStatus)
        {
            mStatus = nStatus;
        }

        public void CommitData()
        {
            byte[] buf = new byte[4096];
       
            CreateHeader();
            byte[] bytes = System.Text.Encoding.Default.GetBytes(mStrHeader);
            //fill buf
            Array.Copy(bytes, buf, bytes.Length);
            int nBRemain = buf.Length - bytes.Length;

            mBufSream.Seek(0,SeekOrigin.Begin);
            if (mBufSream.Length > 0)
            {
                int nRead = mBufSream.Read(buf, bytes.Length, nBRemain);
                mStream.Write(buf, bytes.Length + nRead);
            }
            else
                return;
            //
            int bCount = 0;
            while((bCount = mBufSream.Read(buf, 0, buf.Length))> 0)
            {
                mStream.Write(buf, bCount);
            }
            
        }
      
        private int mStatus;
        private OpenSslServerStream mStream;
        private HttpRequest mRefRequest;
        private string mStrHeader;
        private BufferedStream mBufSream; 
        private bool bDataIsUtf8Text;

      
    }
}
