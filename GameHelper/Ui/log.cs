using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameHelper {
    public class Mess {
        public string info;
        public int count;
        public MessType mtype;
    }
    public enum MessType { Ok = 0, Error = 1, Warning = 2, OnTop,
        Critical
    }

    public class FixedSizedLog : ConcurrentQueue<Mess> {
        private readonly object locker = new object();

        public int Size { get; private set; }
        public FixedSizedLog(int size) {
            Size = size;
        }
        public new void Clear() {
            lock (locker) {
                Mess outObj;
                while (base.TryDequeue(out outObj)) {
                    // do nothing
                }
            }
        }

        /// <summary>
        /// Adds an object to the end of the System.Collections.Concurrent.ConcurrentQueue`1.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(string str, MessType _mt = MessType.Ok) {
            var sampl = @"\[(?<asd>[^\[\]]*)\]";
            lock (locker) {
                while (base.Count > Size) {
                    base.TryDequeue(out _);
                }
                var ci = 0;
                var index = -1;
                //looking for an old line, the same as the one we add,
                //discarding the changeable one inside the square brackets
                foreach (var c in this) { //
                    var curr = Regex.Replace(c.info, sampl, "");
                    var nstr = Regex.Replace(str, sampl, "");
                    if (curr == nstr) {
                        index = ci;
                        break;
                    }
                    ci++;
                }
                if (index != -1) { //if found old string...
                    Debug.Assert(index < Size);
                    var old = base.ToArray();
                    if (str.Contains("[") && str.Contains("]"))
                        old[index] = new Mess() { info = str, count = 0, mtype = _mt };
                    else
                        old[index] = new Mess() { info = old[index].info, count = old[index].count + 1, mtype = _mt };

                    Clear();
                    foreach (var v in old)
                        base.Enqueue(v);
                } else { //add a new value to the end of the list and crop at the top

                    if (_mt != MessType.OnTop) {
                        base.Enqueue(new Mess() { info = str, count = 0, mtype = _mt });
                    } else {
                        var clist = this.ToArray();
                        Clear();
                        base.Enqueue(new Mess() { info = str, count = 0, mtype = _mt });
                        foreach (var s in clist)
                            base.Enqueue(s);
                    }
                }
            }
        }
    }
}
