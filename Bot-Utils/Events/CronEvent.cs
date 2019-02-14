using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class CronEvent : ModulEventArgs {

    public CronEvent() {
    }

    public CronEvent(String addr, String prop, String value) {
      this.Address = addr;
      this.Property = prop;
      this.Value = value;
      this.Source = "Cronjob";
    }
  }
}
