using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using BlubbFish.Utils.IoT.Bots.Interfaces;

namespace BlubbFish.Utils.IoT.Bots.Moduls {
  public abstract class CronJob<T> : AModul<T>, IDisposable, IForceLoad {
    protected readonly List<Tuple<String, Action<Object>, Object>> internalCron = new List<Tuple<String, Action<Object>, Object>>();
    protected Thread thread;
    protected DateTime crontime;

    protected readonly Dictionary<String, String> cron_named = new Dictionary<String, String> {
      { "@yearly",   "0 0 1 1 *" },
      { "@annually", "0 0 1 1 *" },
      { "@monthly",  "0 0 1 * *" },
      { "@weekly",   "0 0 * * 0" },
      { "@daily",    "0 0 * * *" },
      { "@hourly",   "0 * * * *" }
    };

    #region Constructor
    public CronJob(T lib, InIReader settings) : base(lib, settings) {
      this.crontime = DateTime.Now;
      this.thread = new Thread(this.Runner);
      this.thread.Start();
    }
    #endregion

    #region Cronjobrunner
    protected void Runner() {
      Thread.Sleep(DateTime.Now.AddMinutes(1).AddSeconds(DateTime.Now.Second * -1).AddMilliseconds(DateTime.Now.Millisecond * -1) - DateTime.Now);
      while (true) {
        if (this.crontime.Minute != DateTime.Now.Minute) {
          this.crontime = DateTime.Now;
          if (this.config.Count != 0) {
            foreach (KeyValuePair<String, Dictionary<String, String>> item in this.config) {
              if (item.Value.ContainsKey("cron") && item.Value.ContainsKey("set") && this.ParseCronString(item.Value["cron"])) {
                this.SetValues(item.Value["set"]);
              }
            }
          }
          foreach (Tuple<String, Action<Object>, Object> item in this.internalCron) {
            if (this.ParseCronString(item.Item1)) {
              item.Item2?.Invoke(item.Item3);
            }
          }
        }
        Thread.Sleep(100);
      }
    }

    protected abstract void SetValues(String value);
    #endregion

    #region CronFunctions
    protected Boolean ParseCronString(String cronstring) {
      cronstring = cronstring.Trim();
      if (this.cron_named.ContainsKey(cronstring)) {
        cronstring = this.cron_named[cronstring];
      }
      String[] value = cronstring.Split(' ');
      if (value.Length != 5) {
        return false;
      }
      if (!this.CheckDateStr(this.crontime.ToString("mm"), value[0], "0-59")) {
        return false;
      }
      if (!this.CheckDateStr(this.crontime.ToString("HH"), value[1], "0-23")) {
        return false;
      }
      if (!this.CheckDateStr(this.crontime.ToString("MM"), value[3], "1-12")) {
        return false;
      }
      if (value[2] != "*" && value[4] != "*") {
        if (!this.CheckDateStr(this.crontime.ToString("dd"), value[2], "1-31") && !this.CheckDateStr(((Int32)this.crontime.DayOfWeek).ToString(), value[4], "0-7")) {
          return false;
        }
      } else {
        if (!this.CheckDateStr(this.crontime.ToString("dd"), value[2], "1-31")) {
          return false;
        }
        if (!this.CheckDateStr(((Int32)this.crontime.DayOfWeek).ToString(), value[4], "0-7")) {
          return false;
        }
      }
      return true;
    }
    protected Boolean CheckDateStr(String date, String cron, String limit) {
      cron = cron.ToLower();
      for (Int32 i = 0; i <= 6; i++) {
        cron = cron.Replace(DateTime.Parse("2015-01-" + (4 + i) + "T00:00:00").ToString("ddd", CultureInfo.CreateSpecificCulture("en-US")), i.ToString());
        cron = cron.Replace(DateTime.Parse("2015-01-" + (4 + i) + "T00:00:00").ToString("dddd", CultureInfo.CreateSpecificCulture("en-US")), i.ToString());
      }
      for (Int32 i = 1; i <= 12; i++) {
        cron = cron.Replace(DateTime.Parse("2015-" + i + "-01T00:00:00").ToString("MMM", CultureInfo.CreateSpecificCulture("en-US")), i.ToString());
        cron = cron.Replace(DateTime.Parse("2015-" + i + "-01T00:00:00").ToString("MMMM", CultureInfo.CreateSpecificCulture("en-US")), i.ToString());
      }
      if (cron.Contains("*")) {
        cron = cron.Replace("*", limit);
      }
      if (cron.Contains("-")) {
        MatchCollection m = new Regex("(\\d+)-(\\d+)").Matches(cron);
        foreach (Match p in m) {
          List<String> s = new List<String>();
          for (Int32 i = Math.Min(Int32.Parse(p.Groups[1].Value), Int32.Parse(p.Groups[2].Value)); i <= Math.Max(Int32.Parse(p.Groups[1].Value), Int32.Parse(p.Groups[2].Value)); i++) {
            s.Add(i.ToString());
          }
          cron = cron.Replace(p.Groups[0].Value, String.Join(",", s));
        }
      }
      Int32 match = 0;
      if (cron.Contains("/")) {
        Match m = new Regex("/(\\d+)").Match(cron);
        cron = cron.Replace(m.Groups[0].Value, "");
        match = Int32.Parse(m.Groups[1].Value);
      }
      Dictionary<Int32, String> ret = new Dictionary<Int32, String>();
      if (!cron.Contains(",")) {
        ret.Add(Int32.Parse(cron), "");
      } else {
        foreach (String item in cron.Split(',')) {
          if (!ret.ContainsKey(Int32.Parse(item))) {
            ret.Add(Int32.Parse(item), "");
          }
        }
      }
      if (match != 0) {
        Dictionary<Int32, String> r = new Dictionary<Int32, String>();
        foreach (KeyValuePair<Int32, String> item in ret) {
          if (item.Key % match == 0) {
            r.Add(item.Key, "");
          }
        }
        ret = r;
      }
      return ret.ContainsKey(Int32.Parse(date));
    }
    #endregion

    #region AModul
    public override void SetInterconnection(String cron, Action<Object> hook, Object data) => this.internalCron.Add(new Tuple<String, Action<Object>, Object>(cron, hook, data));

    protected override void UpdateConfig() { }
    #endregion

    #region IDisposable Support
    private Boolean disposedValue = false;

    protected virtual void Dispose(Boolean disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          if (this.thread != null) {
            this.thread.Abort();
            while (this.thread.ThreadState == ThreadState.Running) { Thread.Sleep(100); }
          }
        }
        this.thread = null;
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
