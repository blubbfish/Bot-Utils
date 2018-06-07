using System;
using System.Collections.Generic;
using BlubbFish.Utils.IoT.Bots.Events;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class AModul<T> {
    protected T library;
    private readonly InIReader settings;
    protected Dictionary<String, Dictionary<String, String>> config = new Dictionary<String, Dictionary<String, String>>();

    public Boolean HasConfig { get; private set; }
    public Boolean ConfigPublic { get; private set; }

    public delegate void ModulEvent(Object sender, ModulEventArgs e);
    public abstract event ModulEvent Update;

    public AModul(T lib, InIReader settings) {
      this.HasConfig = false;
      this.ConfigPublic = false;
      this.library = lib;
      this.settings = settings;
      this.ParseConfig();
    }

    private void ParseConfig() {
      if (this.settings != null) {
        this.HasConfig = true;
        foreach (String item in this.settings.GetSections(false)) {
          this.config.Add(item, this.settings.GetSection(item));
        }
        if (this.config.ContainsKey("modul")) {
          this.ConfigPublic = this.config["modul"].ContainsKey("config") && this.config["modul"]["config"].ToLower() == "public";
        }
      }
    }

    public Dictionary<String, Dictionary<String, String>> GetConfig() {
      if (this.HasConfig && this.ConfigPublic) {
        Dictionary<String, Dictionary<String, String>> ret = new Dictionary<String, Dictionary<String, String>>(this.config);
        if (ret.ContainsKey("modul")) {
          ret.Remove("modul");
        }
        return ret;
      }
      return new Dictionary<String, Dictionary<String, String>>();
    }

    public virtual void Interconnect(Dictionary<String, Object> moduls) { }

    public virtual void SetInterconnection(String param, Action<Object> hook, Object data) { }

    public abstract void Dispose();

    public void SetConfig(Dictionary<String, Dictionary<String, String>> newconf) {
      if (this.HasConfig && this.ConfigPublic) {
        if (newconf.ContainsKey("modul")) {
          newconf.Remove("modul");
        }
        if (this.config.ContainsKey("modul")) {
          newconf.Add("modul", this.config["modul"]);
        }
        this.config = newconf;
        this.settings.SetSections(this.config);
        this.UpdateConfig();
      }
    }

    protected abstract void UpdateConfig();
  }
}
