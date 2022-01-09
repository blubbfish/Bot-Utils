using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlubbFish.Utils.IoT.Bots.Events;
using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;
using LitJson;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class Mqtt<T> : AModul<T>, IDisposable {
    protected ABackend mqtt;
    protected Dictionary<String, AModul<T>> modules;

    #region Constructor
    public Mqtt(T lib, InIReader settings) : base(lib, settings) => this.Connect();
    #endregion

    #region Connection
    protected void Reconnect() {
      if(!this.config.ContainsKey("settings")) {
        throw new ArgumentException("Setting section [settings] is missing!");
      } else {
        this.Disconnect();
        this.Connect();
      }
    }

    protected void Connect() => this.mqtt = !this.config.ContainsKey("settings") ? throw new ArgumentException("Setting section [settings] is missing!") : ABackend.GetInstance(this.config["settings"], ABackend.BackendType.Data);

    protected void Disconnect() => this.mqtt.Dispose();
    #endregion

    #region AModul
    public override void Interconnect(Dictionary<String, AModul<T>> moduls) => this.modules = moduls;

    protected override void UpdateConfig() => this.Reconnect();
    #endregion

    protected Tuple<Boolean, MqttEvent> ChangeConfig(BackendEvent e, String topic) {
      if (e.From.ToString().StartsWith(topic) && (e.From.ToString().EndsWith("/set") || e.From.ToString().EndsWith("/get"))) {
        Match m = new Regex("^"+ topic + "(\\w+)/[gs]et$|").Match(e.From.ToString());
        if (!m.Groups[1].Success) {
          return new Tuple<Boolean, MqttEvent>(false, null);
        }
        AModul<T> modul = null;
        foreach (KeyValuePair<String, AModul<T>> item in this.modules) {
          if (item.Key.ToLower() == m.Groups[1].Value) {
            modul = item.Value;
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
          } catch { }
        }
      }
      return new Tuple<Boolean, MqttEvent>(false, null);
    }

    #region IDisposable Support
    private Boolean disposedValue = false;

    protected void Dispose(Boolean disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          this.Disconnect();
        }
        this.disposedValue = true;
      }
    }

    public override void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
