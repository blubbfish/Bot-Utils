using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using BlubbFish.Utils.IoT.Bots.Events;
using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;
using LitJson;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class Mqtt<T> : AModul<T>, IDisposable {
    protected readonly Thread connectionWatcher;
    protected ABackend mqtt;
    protected Dictionary<String, Object> modules;

    #region Constructor
    public Mqtt(T lib, InIReader settings) : base(lib, settings) {
      if (this.config.ContainsKey("settings")) {
        this.connectionWatcher = new Thread(this.ConnectionWatcherRunner);
        this.connectionWatcher.Start();
      }
    }
    #endregion

    #region Watcher
    protected void ConnectionWatcherRunner() {
      while (true) {
        try {
          if (this.mqtt == null || !this.mqtt.IsConnected) {
            this.Reconnect();
          }
          Thread.Sleep(10000);
        } catch (Exception) { }
      }
    }

    protected void Reconnect() {
      this.Disconnect();
      this.Connect();
    }

    protected abstract void Connect();

    protected abstract void Disconnect();
    #endregion

    #region AModul
    public override void Interconnect(Dictionary<String, Object> moduls) {
      this.modules = moduls;
    }

    protected override void UpdateConfig() {
      this.Reconnect();
    }
    #endregion

    protected Tuple<Boolean, MqttEvent> ChangeConfig(BackendEvent e, String topic) {
      if (e.From.ToString().StartsWith(topic) && (e.From.ToString().EndsWith("/set") || e.From.ToString().EndsWith("/get"))) {
        Match m = new Regex("^"+ topic + "(\\w+)/[gs]et$|").Match(e.From.ToString());
        if (!m.Groups[1].Success) {
          return new Tuple<Boolean, MqttEvent>(false, null);
        }
        AModul<T> modul = null;
        foreach (KeyValuePair<String, Object> item in this.modules) {
          if (item.Key.ToLower() == m.Groups[1].Value) {
            modul = ((AModul<T>)item.Value);
          }
        }
        if (modul == null) {
          return new Tuple<Boolean, MqttEvent>(false, null);
        }
        if (e.From.ToString().EndsWith("/get") && modul.HasConfig && modul.ConfigPublic) {
          String t = topic + m.Groups[1].Value;
          String d = JsonMapper.ToJson(modul.GetConfig()).ToString();
          ((ADataBackend)this.mqtt).Send(t, d);
          return new Tuple<Boolean, MqttEvent>(true, new MqttEvent(t, d));
        } else if (e.From.ToString().EndsWith("/set") && modul.HasConfig && modul.ConfigPublic) {
          try {
            JsonData a = JsonMapper.ToObject(e.Message);
            Dictionary<String, Dictionary<String, String>> newconf = new Dictionary<String, Dictionary<String, String>>();
            foreach (String section in a.Keys) {
              Dictionary<String, String> sectiondata = new Dictionary<String, String>();
              foreach (String item in a[section].Keys) {
                sectiondata.Add(item, a[section][item].ToString());
              }
              newconf.Add(section, sectiondata);
            }
            modul.SetConfig(newconf);
            return new Tuple<Boolean, MqttEvent>(true, new MqttEvent("New Config", "Write"));
          } catch (Exception) { }
        }
      }
      return new Tuple<Boolean, MqttEvent>(false, null);
    }

    #region IDisposable Support
    private Boolean disposedValue = false;

    protected void Dispose(Boolean disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this.connectionWatcher.Abort();
          while (this.connectionWatcher.ThreadState == ThreadState.Running) { Thread.Sleep(10); }
          this.Disconnect();
        }
        this.disposedValue = true;
      }
    }

    public override void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
