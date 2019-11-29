using System;
using System.Runtime.Loader;
using System.Threading;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class ABot {
    #if !NETCOREAPP
    private Thread sig_thread;
    #endif
    private Boolean RunningProcess = true;

    protected ProgramLogger logger = new ProgramLogger();

    private void SetupShutdown(Object sender, ConsoleCancelEventArgs e) {
      e.Cancel = true;
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.SetupShutdown: Signalhandler Windows INT recieved.");
      this.RunningProcess = false;
    }
    private void Default_Unloading(AssemblyLoadContext obj) {
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.SetupShutdown: Signalhandler Windows INT recieved.");
      this.RunningProcess = false;
    }

    protected void WaitForShutdown() {
      if(Type.GetType("Mono.Runtime") != null) {
        #if !NETCOREAPP
        this.sig_thread = new Thread(delegate () {
          Mono.Unix.UnixSignal[] signals = new Mono.Unix.UnixSignal[] {
            new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
            new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGINT)
          };
          Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Mono attached.");
          while(true) {
            Int32 i = Mono.Unix.UnixSignal.WaitAny(signals, -1);
            Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Mono INT recieved " + i + ".");
            this.RunningProcess = false;
            break;
          }
        });
        this.sig_thread.Start();
        #endif
      } else {
        AssemblyLoadContext.Default.Unloading += this.Default_Unloading;
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Netcore attached.");
        Console.CancelKeyPress += new ConsoleCancelEventHandler(this.SetupShutdown);
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Windows attached.");
      }
      while(this.RunningProcess) {
        Thread.Sleep(100);
      }
    }

    

    public virtual void Dispose() {
      #if !NETCOREAPP
      if(this.sig_thread != null && this.sig_thread.IsAlive) {
        this.sig_thread.Abort();
      }
      #endif
    }
  }
}
