using System;
using System.Collections.Generic;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class Overtaker<T> : AModul<T>, IDisposable {
    protected readonly Dictionary<String, Dictionary<String, String>> events = new Dictionary<String, Dictionary<String, String>>();

    #region Constructor
    public Overtaker(T lib, InIReader settings) : base(lib, settings) {
      this.ParseIni();
    }
    #endregion

    #region Overtakerfunctions
    protected void ParseIni() {
      this.RemoveLibraryUpdateHooks();
      foreach (KeyValuePair<String, Dictionary<String, String>> item in this.config) {
        if (item.Value.ContainsKey("from")) {
          String from = item.Value["from"];
          String[] source = from.Split(':');
          this.events.Add(source[0], item.Value);
          this.AddLibraryUpdateHook(source[0]);
        }
      }
    }

    protected void SetValues(Object sender, String name, Dictionary<String, String> dictionary) {
      String from = dictionary["from"];
      String[] source = from.Split(':');
      if (source.Length != 2) {
        return;
      }
      String source_value;
      if (sender.HasProperty(source[1])) {
        source_value = sender.GetProperty(source[1]).ToString();
      } else {
        return;
      }
      if (dictionary.ContainsKey("convert")) {
        foreach (String tuple in dictionary["convert"].Split(';')) {
          String[] item = tuple.Split('-');
          if (source_value == item[0]) {
            source_value = item[1];
          }
        }
      }
      if (dictionary.ContainsKey("to")) {
        foreach (String to in dictionary["to"].Split(';')) {
          String[] target = to.Split(':');
          if (target.Length == 2) {
            this.SetValueHook(target[0], target[1], source_value);
          }
        }
      }
    }

    protected abstract void AddLibraryUpdateHook(String id);

    protected abstract void RemoveLibraryUpdateHooks();

    protected abstract void SetValueHook(String id, String prop, String value);
    #endregion

    #region AModul
    public override void Interconnect(Dictionary<String, AModul<T>> moduls) { }
    protected override void UpdateConfig() {
      this.ParseIni();
    }
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
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion   
  }
}
