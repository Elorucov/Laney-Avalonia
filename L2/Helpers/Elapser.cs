using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ELOR.Laney.Helpers {
    public class Elapser<T> {
        private Dictionary<T, Timer> registeredObjects = new Dictionary<T, Timer>();
        public IReadOnlyList<T> RegisteredObjects { get { return registeredObjects.Keys.ToList(); } }

        public event EventHandler<T> Elapsed;

        public void Add(T obj, double milliseconds) {
            Timer timer = new Timer(TimeSpan.FromMilliseconds(milliseconds));
            timer.Elapsed += (a, b) => {
                Elapsed?.Invoke(this, obj);
                registeredObjects.Remove(obj);
            };
            timer.Start();

            if (registeredObjects.ContainsKey(obj)) {
                registeredObjects[obj].Stop();
                registeredObjects[obj] = timer;
            } else {
                try {
                    registeredObjects.Add(obj, timer);
                } catch (IndexOutOfRangeException oex) { // He-he...
                    timer.Stop();
                    Log.Error(oex, $"Elapser.Add: \"Classic\" out-of-range error when adding something to Dictionary...");
                    Clear();
                }
            }
        }

        public void Remove(T obj) {
            if (registeredObjects.ContainsKey(obj)) {
                registeredObjects[obj].Stop();
                registeredObjects.Remove(obj);
                Elapsed?.Invoke(this, obj);
            }
        }

        public void Clear() {
            foreach (var obj in registeredObjects) {
                obj.Value.Stop();
                Elapsed?.Invoke(this, obj.Key);
            }
            registeredObjects.Clear();
        }
    }
}