using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class MqttEvent : ModulEventArgs {
    public MqttEvent() {
    }
    public MqttEvent(String topic, String text) {
      this.Address = topic;
      this.Value = text;
      this.Source = "MQTT";
    }
    public override String ToString() => this.Source + ": on " + this.Address + " set " + this.Value;
  }
}
