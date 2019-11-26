using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class StatusPollingEvent : ModulEventArgs {
    public StatusPollingEvent() {
    }

    public StatusPollingEvent(String text, String node) {
      this.Value = text;
      this.Address = node;
      this.Source = "POLLING";
    }

    public override String ToString() => this.Source + ": " + this.Value + " on " + this.Address;
  }
}
