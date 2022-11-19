using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHelper.Utils {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    namespace Stas.GA {
        /// <summary>
        /// thread safe perform checker, based on ui.w8 const
        /// </summary>
        internal class SW : Stopwatch {
            /// <summary>
            /// for reset max_ft after use cleare the log
            /// </summary>
            public static Dictionary<string, SW> registred = new();
            string name { get; }
            List<double> elapsed = new List<double>();
            /// <summary>
            /// max frame time this session
            /// </summary>
            double max_ft = 0;
            public void Restart(bool full = false) {
                base.Restart();
                elapsed.Clear();
                if (full)
                    max_ft = 0;
            }
            public SW(string _name) {
                name = _name;
                registred[name] = this;
            }
            int error_count = 0;
            public void Print(string add = null) {
                var plus = string.IsNullOrEmpty(add) ? "" : "=>" + add + " ";
                var elaps = Elapsed.TotalMilliseconds;

                lock (elapsed) {
                    elapsed.Add(elaps);
                    if (elapsed.Count > 60)
                        elapsed.RemoveAt(0);
                    var ft = elapsed.Sum() / elapsed.Count;//frame time

                    var fps = (1000f / ft).ToRoundStr(0);
                    if (ft > max_ft) {
                        max_ft = ft;
                    }
                    if (ft > Core.w8) {
                        error_count += 1;
                        //ui.AddToLog(name + " to slow", MessType.Error);
                        Core.AddToLog(name + plus + "max=[" + max_ft.ToRoundStr(3) + "]ms curr=[" + ft.ToRoundStr(3) + "]ms fps=[" + fps + "] count=[" + error_count + "]", MessType.Error);
                    }
                    else {
                        Core.AddToLog(name + plus + "max=[" + max_ft.ToRoundStr(3) + "]ms curr=[" + ft.ToRoundStr(2) + "]ms fps=[" + fps + "]");
                    }
                }
            }
        }
    }

}
