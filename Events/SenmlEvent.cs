using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class SenmlEvent : ModulEventArgs {
    public SenmlEvent() {
    }
    public SenmlEvent(String topic, String text) {
      this.Address = topic;
      this.Value = text;
      this.Source = "Senml";
    }
    public override String ToString() {
      return this.Source + ": on " + this.Address + " set " + this.Value;
    }
  }
}
