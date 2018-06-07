using System;

namespace BlubbFish.Utils.IoT.Bots.Events {
  public class ModulEventArgs : EventArgs {
    public ModulEventArgs() {
    }
    public String Address { get; protected set; }
    public String Property { get; protected set; }
    public String Value { get; protected set; }
    public String Source { get; protected set; }
    public override String ToString() {
      return this.Source + ": " + this.Address + " set " + this.Property + " to " + this.Value;
    }
  }
}
