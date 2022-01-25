using System;
using System.Collections.Generic;

using BlubbFish.Utils.IoT.Connector;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class MultiSourceBot : ABot {
    protected Dictionary<String, ABackend> sources;
    protected Dictionary<String, String> settings;

    protected MultiSourceBot(String[] args, Boolean fileLogging, String configSearchPath, Dictionary<String, ABackend> sources, Dictionary<String, String> settings) : base(args, fileLogging, configSearchPath) {
      this.sources = sources;
      this.settings = settings;
    }
  }
}
