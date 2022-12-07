using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameHelper.Utils; 
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
        if (full) {
            elapsed.Clear();
            max_ft = 0;
        }
    }
    int max_coolect = 100;
    public SW(string _name, int max_coolect = 100) {
        name = _name;
        this.max_coolect = max_coolect;
        registred[name] = this;
    }
    public (string, MessType) GetRes => res;
    int error_count = 0;
    public void Print(string add = null) {
        MakeRes(add);
        Core.AddToLog(res.Item1, res.Item2);
    }
    (string, MessType) res;
    public void MakeRes(string add = null) {
        var plus = string.IsNullOrEmpty(add) ? " " : "=>" + add + " ";
        var elaps = Elapsed.TotalMilliseconds;

        lock (elapsed) {
            elapsed.Add(elaps);
            if (elapsed.Count > max_coolect)
                elapsed.RemoveAt(0);
            var ft = elapsed.Sum() / elapsed.Count;//frame time

            var fps = (1000f / ft).ToRoundStr(0);
            if (ft > max_ft) {
                max_ft = ft;
            }
            if (ft > Core.w8) {
                error_count += 1;
                //ui.AddToLog(name + " to slow", MessType.Error);
                res = (name + plus + "max=[" + max_ft.ToRoundStr(3) + "]ms curr=[" + ft.ToRoundStr(3) + "]ms fps=[" + fps + "] count=[" + error_count + "]", MessType.Error);

            }
            else {
                res = (name + plus + "max=[" + max_ft.ToRoundStr(3) + "]ms curr=[" + ft.ToRoundStr(2) + "]ms fps=[" + fps + "]", MessType.Ok);
            }
        }
    }
}
