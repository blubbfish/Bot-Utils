﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;
using LitJson;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class Webserver
  {
    protected Dictionary<String, String> config;
    protected static InIReader requests;
    protected HttpListener httplistener;

    public Webserver(ABackend backend, Dictionary<String, String> settings, InIReader requestslookup) {
      this.config = settings;
      requests = requestslookup;
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
                this.SendWebserverResponse(httplistenercontext);
              } catch { } finally {
                httplistenercontext.Response.OutputStream.Close();
              }
            }, this.httplistener.GetContext());
          }
        } catch { };
      });
    }

    public static Boolean SendFileResponse(HttpListenerContext cont, String folder = "resources") {
      String restr = cont.Request.Url.PathAndQuery;
      if(restr.StartsWith("/")) {
        if(restr.IndexOf("?") != -1) {
          restr = restr.Substring(1, restr.IndexOf("?") - 1);
        } else {
          restr = restr.Substring(1);
        }
        if(Directory.Exists(folder + "/" + restr)) {
          restr += "/index.html";
        }
        String end = restr.IndexOf('.') != -1 ? restr.Substring(restr.IndexOf('.') + 1) : "";
        if(File.Exists(folder + "/" + restr)) {
          try {
            if(end == "png" || end == "jpg" || end == "jpeg" || end == "ico" || end == "woff") {
              Byte[] output = File.ReadAllBytes(folder + "/" + restr);
              switch(end) {
                case "ico":
                  cont.Response.ContentType = "image/x-ico";
                  break;
                case "woff":
                  cont.Response.ContentType = "font/woff";
                  break;
              }
              cont.Response.OutputStream.Write(output, 0, output.Length);
              return true;
            } else {
              String file = File.ReadAllText(folder + "/" + restr);
              if(requests.GetSections(false).Contains(restr)) {
                Dictionary<String, String> vars = requests.GetSection(restr);
                foreach(KeyValuePair<String, String> item in vars) {
                  file = file.Replace("\"{%" + item.Key.ToUpper() + "%}\"", item.Value);
                }
              }
              file = file.Replace("{%REQUEST_URL_HOST%}", cont.Request.Url.Host);
              Byte[] buf = Encoding.UTF8.GetBytes(file);
              cont.Response.ContentLength64 = buf.Length;
              switch(end) {
                case "css":
                  cont.Response.ContentType = "text/css";
                  break;
              }
              cont.Response.OutputStream.Write(buf, 0, buf.Length);
              Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
              return true;
            }
          } catch(Exception e) {
            Helper.WriteError("500 - " + e.Message);
            cont.Response.StatusCode = 500;
            return false;
          }
        }
      }
      Helper.WriteError("404 - " + cont.Request.Url.PathAndQuery + " not found!");
      cont.Response.StatusCode = 404;
      return false;
    }

    public static Boolean SendJsonResponse(Object data, HttpListenerContext cont) {
      try {
        Byte[] buf = Encoding.UTF8.GetBytes(JsonMapper.ToJson(data));
        cont.Response.ContentLength64 = buf.Length;
        cont.Response.OutputStream.Write(buf, 0, buf.Length);
        Console.WriteLine("200 - " + cont.Request.Url.PathAndQuery);
        return true;
      } catch { }
      return false;
    }

    public static Dictionary<String, String> GetPostParams(HttpListenerRequest req) {
      if(req.HttpMethod == "POST") {
        if(req.HasEntityBody) {
          StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding);
          String rawData = reader.ReadToEnd();
          req.InputStream.Close();
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

    public void Dispose() {
      this.httplistener.Stop();
      this.httplistener.Close();
    }

    protected abstract void Backend_MessageIncomming(Object sender, BackendEvent e);
    protected abstract Boolean SendWebserverResponse(HttpListenerContext cont);
  }
}
