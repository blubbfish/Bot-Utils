using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlubbFish.Utils.IoT.Connector;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class MultiSourceBot : ABot {
    protected Dictionary<String, ABackend> sources;
    protected Dictionary<String, String> settings;

    protected MultiSourceBot(Dictionary<String, ABackend> sources, Dictionary<String, String> settings) {
      this.sources = sources;
      this.settings = settings;
    }
  }
}
