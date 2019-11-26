using System;
using System.Collections.Generic;
using BlubbFish.Utils.IoT.Bots.Interfaces;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class Statuspolling<T> : AModul<T>, IDisposable, IForceLoad {

    #region Constructor
    public Statuspolling(T lib, InIReader settings) : base(lib, settings) { }
    #endregion

    #region Statuspollingfunctions
    protected abstract void PollSpecific(Object obj);

    protected abstract void PollAll(Object obj);
    #endregion

    #region AModul
    public override void Interconnect(Dictionary<String, AModul<T>> moduls) {
      foreach (KeyValuePair<String, AModul<T>> item in moduls) {
        if (item.Value is CronJob<T>) {
          item.Value.SetInterconnection("0 0 * * *", new Action<Object>(this.PollAll), null);
          if (this.config.Count != 0) {
            foreach (KeyValuePair<String, Dictionary<String, String>> section in this.config) {
              if (section.Value.ContainsKey("cron") && section.Value.ContainsKey("devices")) {
                item.Value.SetInterconnection(section.Value["cron"], new Action<Object>(this.PollSpecific), section.Value["devices"]);
              }
            }
          }
        }
      }
    }
    protected override void UpdateConfig() { }
    #endregion

    #region IDisposable Support
    private Boolean disposedValue = false;

    protected virtual void Dispose(Boolean disposing) {
      if (!this.disposedValue) {
        if (disposing) {
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
