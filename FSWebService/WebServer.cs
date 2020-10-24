using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FSWebService {
    /// <summary>
    /// Based on code from:
    /// https://www.codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server.aspx
    /// 
    /// The MIT License (MIT)
    /// 
    /// Copyright (c) 2013 David's Blog (www.codehosting.net)     
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    /// associated documentation files (the "Software"), to deal in the Software without restriction, 
    /// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    /// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in all copies or 
    /// substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
    /// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    /// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
    /// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
    /// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
    /// DEALINGS IN THE SOFTWARE.
    /// </summary>
    class WebServer : IDisposable {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;
        private bool disposed = false;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method) {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method) { }

        public void Run() {
            System.Threading.ThreadPool.QueueUserWorkItem((o) => {
                System.Diagnostics.Debug.WriteLine("Webserver running...");
                try {
                    while (_listener.IsListening) {
                        System.Threading.ThreadPool.QueueUserWorkItem((c) => {
                            var ctx = c as HttpListenerContext;
                            try {
                                string rstr = _responderMethod(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "https://skyvector.com");
                                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET");
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (HttpListenerException) { } // suppress any exceptions
                            finally {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop() {
            _listener.Stop();
            _listener.Close();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;
            if (disposing) {
                Stop();
            }
            disposed = true;
        }
    }
}
