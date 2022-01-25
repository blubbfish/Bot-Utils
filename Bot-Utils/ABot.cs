using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class ABot {
    private Boolean RunningProcess = true;
    private readonly ProgramLogger logger = null;

    public Boolean DebugLogging {
      get;
    }

    public ABot(String[] _, Boolean fileLogging, String configSearchPath) {
      InIReader.SetSearchPath(new List<String>() { "/etc/"+ configSearchPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\"+ configSearchPath });
      if(fileLogging) {
        this.logger = new ProgramLogger(InIReader.GetInstance("settings").GetValue("logging", "path", Assembly.GetEntryAssembly().GetName().Name + ".log"));
      }
      if(Boolean.TryParse(InIReader.GetInstance("settings").GetValue("logging", "debug", "true"), out Boolean debuglog)) {
        this.DebugLogging = debuglog;
      }
    }

    private void ConsoleCancelEvent(Object sender, ConsoleCancelEventArgs e) {
      e.Cancel = true;
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ConsoleCancelEvent()");
      this.RunningProcess = false;
    }

    #if NETCOREAPP
    private void Unloading(AssemblyLoadContext obj) => this.RunningProcess = false;

    private void ProcessExit(Object sender, EventArgs e) => this.RunningProcess = false;
    #endif

    protected void WaitForShutdown() {
      #if NETCOREAPP
      AssemblyLoadContext.Default.Unloading += this.Unloading;
      AppDomain.CurrentDomain.ProcessExit += this.ProcessExit;
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Attach Unloading and ProcessExit.");
      #endif
      Console.CancelKeyPress += new ConsoleCancelEventHandler(this.ConsoleCancelEvent);
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Attach ConsoleCancelEvent.");
      while(this.RunningProcess) {
        Thread.Sleep(100);
      }
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Shutdown.");
    }

    public virtual void Dispose() {
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.Dispose: Shutdown.");
      this.RunningProcess = false;
      this.logger?.Dispose();
    }
  }
}
