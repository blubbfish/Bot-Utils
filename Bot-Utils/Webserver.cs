using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlubbFish.Utils.IoT.Bots
{
  public abstract class Webserver
  {
    protected Dictionary<String, String> config;
    protected InIReader requests;
    protected HttpListener httplistener;

    public Webserver(ABackend backend, Dictionary<String, String> settings, InIReader requests) {
      this.config = settings;
      this.requests = requests;
      backend.MessageIncomming += this.Backend_MessageIncomming;
      this.httplistener = new HttpListener();
      this.httplistener.Prefixes.Add(this.config["prefix"]);
      this.httplistener.Start();
      ThreadPool.QueueUserWorkItem((o) => {
        Console.WriteLine("Webserver is Running...");
        try {
          while (this.httplistener.IsListening) {
            ThreadPool.QueueUserWorkItem((state) => {
              HttpListenerContext httplistenercontext = state as HttpListenerContext;
              try {
                this.SendResponse(httplistenercontext);
              } catch { } finally {
                httplistenercontext.Response.OutputStream.Close();
              }
            }, this.httplistener.GetContext());
          }
        } catch { };
      });
    }

    protected virtual void SendResponse(HttpListenerContext cont) {
      String restr = cont.Request.Url.PathAndQuery;
      if (restr.StartsWith("/")) {
        if(restr.IndexOf("?") != -1) {
          restr = restr.Substring(1, restr.IndexOf("?")-1);
        } else {
          restr = restr.Substring(1);
        }
        if(restr == "") {
          restr = "index.html";
        }
        String end = restr.IndexOf('.') != -1 ? restr.Substring(restr.IndexOf('.')+1) : "";
        if (File.Exists("resources/"+ restr)) {
          try {
            if (end  == "png" || end == "jpg" || end == "jpeg" || end == "ico" || end == "woff") {
              Byte[] output = File.ReadAllBytes("resources/" + restr);
              switch(end) {
                case "ico": cont.Response.ContentType = "image/x-ico"; break;
                case "woff": cont.Response.ContentType = "font/woff"; break;
              }
              cont.Response.OutputStream.Write(output, 0, output.Length);
              return;
            } else {
              String file = File.ReadAllText("resources/" + restr);
              if (this.requests.GetSections(false).Contains(restr)) {
                Dictionary<String, String> vars = this.requests.GetSection(restr);
                foreach (KeyValuePair<String, String> item in vars) {
                  file = file.Replace("\"{%" + item.Key.ToUpper() + "%}\"", item.Value);
                }
              }
              file = file.Replace("{%REQUEST_URL_HOST%}", cont.Request.Url.Host);
              Byte[] buf = Encoding.UTF8.GetBytes(file);
              cont.Response.ContentLength64 = buf.Length;
              switch(end) {
                case "css": cont.Response.ContentType = "text/css"; break;
              }
              cont.Response.OutputStream.Write(buf, 0, buf.Length);
              Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
              return;
            }
          } catch(Exception e) {
            Helper.WriteError("500 - " + e.Message);
            cont.Response.StatusCode = 500;
            return;
          }
        }
        Helper.WriteError("404 - " + cont.Request.Url.PathAndQuery + " not found!");
        cont.Response.StatusCode = 404;
        return;
      }
      return;
    }

    public void Dispose() {
      this.httplistener.Stop();
      this.httplistener.Close();
    }

    protected abstract void Backend_MessageIncomming(Object sender, BackendEvent e);
  }
}
