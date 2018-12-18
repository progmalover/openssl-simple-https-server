using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
* Notice
* This code is free ,you can use it in you project,but dont remove my declaration 
* author --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService.https
{
    abstract class ServerListener
    {
        public abstract void OnRequest(HttpRequest request, HttpResponse response);
    }
}
