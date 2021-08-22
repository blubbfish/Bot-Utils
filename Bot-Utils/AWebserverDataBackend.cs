using System;
using System.Collections.Generic;

using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class AWebserverDataBackend : AWebserver {
    protected ABackend databackend;
    protected AWebserverDataBackend(ABackend backend, Dictionary<String, String> settings) : base(settings) => this.databackend = backend;

    protected void StartDataBackend() => this.databackend.MessageIncomming += this.Backend_MessageIncomming;

    protected abstract void Backend_MessageIncomming(Object sender, BackendEvent e);

    public override void Dispose() {
      if(this.databackend != null) {
        this.databackend.Dispose();
      }
      base.Dispose();
    }
  }
}
