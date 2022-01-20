using System;
using System.Runtime.Loader;
using System.Threading;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class ABot {
    private Boolean RunningProcess = true;

    protected ProgramLogger logger = new ProgramLogger();

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
    }
  }
}
