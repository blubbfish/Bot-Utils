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
          Console.WriteLine("Signalhandler Mono attached.");
          while (true) {
            Int32 i = Mono.Unix.UnixSignal.WaitAny(signals, -1);
            Console.WriteLine("Signalhandler Mono INT recieved " + i + ".");
            this.RunningProcess = false;
            break;
          }
        });
        this.sig_thread.Start();
      } else {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(this.SetupShutdown);
        Console.WriteLine("Signalhandler Windows attached.");
      }
      while (this.RunningProcess) {
        Thread.Sleep(100);
      }
    }

    private void SetupShutdown(Object sender, ConsoleCancelEventArgs e) {
      e.Cancel = true;
      Console.WriteLine("Signalhandler Windows INT recieved.");
      this.RunningProcess = false;
    }

    protected void ModulDispose() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.Dispose();
        Console.WriteLine("Modul entladen: " + item.Key);
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
          if (InIReader.ConfigExist(name.ToLower())) {
            this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, InIReader.GetInstance(name.ToLower()) }));
            Console.WriteLine("Load Modul " + name);
          } else if (t.HasInterface(typeof(IForceLoad))) {
            this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, null }));
            Console.WriteLine("Load Modul Forced " + name);
          }
        }
      }
    }

    protected void ModulInterconnect() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.Interconnect(this.moduls);
      }
    }

    protected void ModulEvents() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        item.Value.Update += this.ModulUpdate;
      }
    }

    protected void ModulUpdate(Object sender, ModulEventArgs e) {
      Console.WriteLine(e.ToString());
    }
  }
}
