using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BlubbFish.Utils.IoT.Bots.Moduls;
using BlubbFish.Utils.IoT.Bots.Events;
using BlubbFish.Utils.IoT.Bots.Interfaces;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class Bot<T> {
    private Thread sig_thread;
    private Boolean RunningProcess = true;
    protected ProgramLogger logger = new ProgramLogger();
    protected readonly Dictionary<String, AModul<T>> moduls = new Dictionary<String, AModul<T>>();

    protected void WaitForShutdown() {
      if (Type.GetType("Mono.Runtime") != null) {
        this.sig_thread = new Thread(delegate () {
          Mono.Unix.UnixSignal[] signals = new Mono.Unix.UnixSignal[] {
            new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
            new Mono.Unix.UnixSignal(Mono.Unix.Native.Signum.SIGINT)
          };
          Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Mono attached.");
          while (true) {
            Int32 i = Mono.Unix.UnixSignal.WaitAny(signals, -1);
            Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Mono INT recieved " + i + ".");
            this.RunningProcess = false;
            break;
          }
        });
        this.sig_thread.Start();
      } else {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(this.SetupShutdown);
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.WaitForShutdown: Signalhandler Windows attached.");
      }
      while (this.RunningProcess) {
        Thread.Sleep(100);
      }
    }

    private void SetupShutdown(Object sender, ConsoleCancelEventArgs e) {
      e.Cancel = true;
      Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.SetupShutdown: Signalhandler Windows INT recieved.");
      this.RunningProcess = false;
    }

    protected void ModulDispose() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.Dispose();
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulDispose: Modul entladen: " + item.Key);
      }
      if (this.sig_thread != null && this.sig_thread.IsAlive) {
        this.sig_thread.Abort();
      }
    }

    protected void ModulLoader(String @namespace, Object library) {
      Assembly asm = Assembly.GetEntryAssembly();
      foreach (Type item in asm.GetTypes()) {
        if (item.Namespace == @namespace) {
          Type t = item;
          String name = t.Name;
          try {
            if (InIReader.ConfigExist(name.ToLower())) {
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Load Modul " + name);
              this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, InIReader.GetInstance(name.ToLower()) }));
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Loaded Modul " + name);
            } else if (t.HasInterface(typeof(IForceLoad))) {
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Load Modul Forced " + name);
              this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, null }));
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Loaded Modul Forced " + name);
            }
          } catch(Exception e) {
            Helper.WriteError(e.InnerException.Message);
          }
        }
      }
    }

    protected void ModulInterconnect() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.Interconnect(this.moduls);
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulInterconnect: Interconnect Module " + item.Key);
      }
    }

    protected void ModulEvents() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.EventLibSetter();
        item.Value.Update += this.ModulUpdate;
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulEvents: Attach Event " + item.Key);
      }
    }

    protected void ModulUpdate(Object sender, ModulEventArgs e) {
      Console.WriteLine(e.ToString());
    }
  }
}
