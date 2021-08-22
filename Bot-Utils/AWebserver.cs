using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class AWebserver : ABot {
    protected Dictionary<String, String> config;
    
    protected HttpListener httplistener;

    public AWebserver(Dictionary<String, String> settings) => this.config = settings;

    protected void StartListen() {
      this.httplistener = new HttpListener();
      this.httplistener.Prefixes.Add(this.config["prefix"]);
      this.httplistener.Start();
      _ = ThreadPool.QueueUserWorkItem((o) => {
        Console.WriteLine("Webserver is Running...");
        try {
          while(this.httplistener.IsListening) {
            _ = ThreadPool.QueueUserWorkItem((state) => {
              HttpListenerContext httplistenercontext = state as HttpListenerContext;
              try {
                _ = this.SendWebserverResponse(httplistenercontext);
              } catch { } finally {
                httplistenercontext.Response.OutputStream.Close();
              }
            }, this.httplistener.GetContext());
          }
        } catch { };
      });
    }

    public override void Dispose() {
      if(this.httplistener.IsListening) {
        this.httplistener.Stop();
      }
      this.httplistener.Close();
      base.Dispose();
    }

    protected abstract Boolean SendWebserverResponse(HttpListenerContext cont);
  }

  #region HttpListener* Extensions
  public static class HttpListenerHelper {
    private static InIReader requests;

    public static Dictionary<String, String> GetPostParams(this HttpListenerRequest request) {
      if(request.HttpMethod == "POST") {
        if(request.HasEntityBody) {
          StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
          String rawData = reader.ReadToEnd();
          request.InputStream.Close();
          reader.Close();
          Dictionary<String, String> ret = new Dictionary<String, String>();
          foreach(String param in rawData.Split('&')) {
            String[] kvPair = param.Split('=');
            if(!ret.ContainsKey(kvPair[0])) {
              ret.Add(kvPair[0], HttpUtility.UrlDecode(kvPair[1]));
            }
          }
          return ret;
        }
      }
      return new Dictionary<String, String>();
    }

    public static Boolean SendStringResponse(this HttpListenerContext cont, String obj) => cont.SendBinaryResponse(Encoding.UTF8.GetBytes(obj));

    public static Boolean SendBinaryResponse(this HttpListenerContext cont, Byte[] buf) {
      try {
        cont.Response.ContentLength64 = buf.Length;
        cont.Response.OutputStream.Write(buf, 0, buf.Length);
        Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
        return true;
      } catch(Exception e) {
        Helper.WriteError("500 - " + e.Message + "\n\n" + e.StackTrace);
        cont.Response.StatusCode = 500;
      }
      return false;
    }

    public static Boolean SendFileResponse(this HttpListenerContext cont, String folder = "resources", Boolean printOutput = true) {
      String restr = cont.Request.Url.PathAndQuery;
      if(restr.StartsWith("/")) {
        restr = restr.IndexOf("?") != -1 ? restr[1..restr.IndexOf("?")] : restr[1..];
        if(Directory.Exists(folder + "/" + restr)) {
          restr += "/index.html";
        }
        String end = restr.IndexOf('.') != -1 ? restr[(restr.IndexOf('.') + 1)..] : "";
        if(File.Exists(folder + "/" + restr)) {
          try {
            if(end == "png" || end == "jpg" || end == "jpeg" || end == "ico" || end == "woff" || end == "mp4") {
              Byte[] output = File.ReadAllBytes(folder + "/" + restr);
              switch(end) {
                case "ico":
                  cont.Response.ContentType = "image/x-ico";
                  break;
                case "woff":
                  cont.Response.ContentType = "font/woff";
                  break;
                case "mp4":
                  cont.Response.ContentType = "video/mpeg";
                  break;
              }
              cont.Response.OutputStream.Write(output, 0, output.Length);
              if(printOutput) {
                Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
              }
              return true;
            } else {
              String file = File.ReadAllText(folder + "/" + restr);
              if(requests != null && requests.GetSections(false).Contains(restr)) {
                Dictionary<String, String> vars = requests.GetSection(restr);
                foreach(KeyValuePair<String, String> item in vars) {
                  file = file.Replace("\"{%" + item.Key.ToUpper() + "%}\"", item.Value);
                }
              }
              file = file.Replace("{%REQUEST_URL_HOST%}", cont.Request.Url.Host + ":" + cont.Request.Url.Port);
              Byte[] buf = Encoding.UTF8.GetBytes(file);
              cont.Response.ContentLength64 = buf.Length;
              switch(end) {
                case "css":
                  cont.Response.ContentType = "text/css";
                  break;
              }
              cont.Response.OutputStream.Write(buf, 0, buf.Length);
              if(printOutput) {
                Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
              }
              return true;
            }
          } catch(Exception e) {
            Helper.WriteError("500 - " + e.Message + "\n\n" + e.StackTrace);
            cont.Response.StatusCode = 500;
            return false;
          }
        }
      }
      if(printOutput) {
        Helper.WriteError("404 - " + cont.Request.Url.PathAndQuery + " not found!");
      }
      cont.Response.StatusCode = 404;
      return false;
    }

    public static void SetRequestsOverride(InIReader requestslookup) => requests = requestslookup;
  }
  #endregion
}
