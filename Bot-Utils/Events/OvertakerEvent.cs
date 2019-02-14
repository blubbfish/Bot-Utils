using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class OvertakerEvent : ModulEventArgs {

    public OvertakerEvent() {
    }

    public OvertakerEvent(String addr, String prop, String value) {
      this.Address = addr;
      this.Property = prop;
      this.Value = value;
      this.Source = "Overtaker";
    }
  }
}
