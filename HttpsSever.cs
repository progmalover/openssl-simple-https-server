using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using VEWebUrlRedirectService.OpensslWrapper;
using System.Reflection;
using VEWebUrlRedirectService.https;
/*
* Notice
* This code is free ,you can use it in you project,but dont remove my declaration 
* author --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService
{

    class HttpsServer : HttpsBaseServer
    {
        protected LocalResourceManager LocalResource;
        public HttpsServer(int port):base(port)
        {
            LocalResource = new LocalResourceManager();
        }
        public override void OnResponse(HttpResponse response,HttpRequest request)
        {
             System.Diagnostics.Debug.Write("HttpsServer:OnResponse!\n");
            TcpDataSender.GetInst().PushUrl(request.mFullURL);

            String reqStrURlRaw = request.mRawURL.ToString();
            bool bSourceExist = LocalResource.LocalFileExist(reqStrURlRaw);
            if (!bSourceExist && request.mReferer == null)
            {
                reqStrURlRaw = "/";
            }
            else
            if (request.mReferer != null)
            {
                string strRef = "";
                if (request.mReferer.StartsWith("https:"))
                    strRef = request.mReferer.Substring(8);
                if (request.mReferer.StartsWith("http:"))
                    strRef = request.mReferer.Substring(7);
                reqStrURlRaw = LocalResource.UrlToLocalRef(request.mFullURL, strRef);
            }

            if (!LocalResource.LocalFileExist(reqStrURlRaw))
                reqStrURlRaw = "/";
            bool bBlockFile = false;
            FileStream fr = LocalResource.GetFStream(reqStrURlRaw, ref bBlockFile);
            if (null == fr)
            {
                //request.();
                response.SetStatus(404);
                try
                {
                    response.WriteString("<html><body>文件未找到！</body></html>");
                }
                catch {}
                return;
            }

            try
            {
                byte[] datas = new byte[4096];
                int rlen = 0;
                while ((rlen = fr.Read(datas, 0, datas.Length)) > 0)
                {
                    if (bBlockFile)
                    {
                        try
                        {
                            response.Write(datas, rlen);
                        }
                        catch {
                            break;
                        }
                    }
                    else
                    {
                        Encoding utf8 = Encoding.UTF8;
                        Encoding unicode = Encoding.Unicode;
                        byte[] dataPtr = datas;
                        if (rlen < 4096)
                        {
                            dataPtr = new byte[rlen];
                            Array.Copy(datas, dataPtr, rlen);
                        }
                        byte[] uniBytes = Encoding.Convert(utf8, unicode, dataPtr);
                        Char[] uniChars = new Char[unicode.GetCharCount(uniBytes, 0, uniBytes.Length)];
                        unicode.GetChars(uniBytes, 0, uniBytes.Length, uniChars, 0);

                        try
                        {
                            response.WriteString(new string(uniChars));
                        }
                        catch {
                            break;
                        }
                        dataPtr = null;
                    }
                }
                
                datas = null;
            }
            catch { }

            System.Diagnostics.Debug.Write("HttpsServer:OnResponse end!\n");
        }
    }

}