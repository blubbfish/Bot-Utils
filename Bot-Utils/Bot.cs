using System;
using System.Collections.Generic;
using System.Reflection;
using BlubbFish.Utils.IoT.Bots.Events;
using BlubbFish.Utils.IoT.Bots.Interfaces;
using BlubbFish.Utils.IoT.Bots.Moduls;

namespace BlubbFish.Utils.IoT.Bots {
  public abstract class Bot<T> : ABot {
    protected readonly Dictionary<String, AModul<T>> moduls = new Dictionary<String, AModul<T>>();

    protected void ModulDispose() {
      foreach (KeyValuePair<String, AModul<T>> item in this.moduls) {
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

    protected void ModulUpdate(Object sender, ModulEventArgs e) => Console.WriteLine(e.ToString());
  }
}
