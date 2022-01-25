using System;
using System.Collections.Generic;
using System.Reflection;
using BlubbFish.Utils.IoT.Bots.Events;
using BlubbFish.Utils.IoT.Bots.Interfaces;
using BlubbFish.Utils.IoT.Bots.Moduls;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class Bot<T> : ABot {
    protected readonly Dictionary<String, AModul<T>> moduls = new Dictionary<String, AModul<T>>();

    public Bot(String[] args, Boolean fileLogging, String configSearchPath) : base(args, fileLogging, configSearchPath) { }

    protected void ModulDispose() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulDispose: Entlade Modul: " + item.Key);
        item.Value.Dispose();
        Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulDispose: Modul entladen: " + item.Key);
      }
      this.Dispose();
    }

    protected void ModulLoader(String @namespace, Object library) {
      Assembly asm = Assembly.GetEntryAssembly();
      foreach (Type item in asm.GetTypes()) {
        if (item.Namespace == @namespace) {
          Type t = item;
          String name = t.Name;
          try {
            if (InIReader.ConfigExist(name.ToLower())) {
              Dictionary<String, String> modulconfig = InIReader.GetInstance(name.ToLower()).GetSection("modul");
              if(!(modulconfig.ContainsKey("enabled") && modulconfig["enabled"].ToLower() == "false")) {
                Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Load Modul " + name);
                this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, InIReader.GetInstance(name.ToLower()) }));
                Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Loaded Modul " + name);
                continue;
              }
            } 
            if (t.HasInterface(typeof(IForceLoad))) {
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Forced Load Modul " + name);
              this.moduls.Add(name, (AModul<T>)t.GetConstructor(new Type[] { typeof(T), typeof(InIReader) }).Invoke(new Object[] { library, null }));
              Console.WriteLine("BlubbFish.Utils.IoT.Bots.Bot.ModulLoader: Forced Loaded Modul " + name);
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
      if(this.DebugLogging) {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
