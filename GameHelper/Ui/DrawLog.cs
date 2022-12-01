using GameHelper.Utils;
using ImGuiNET;
using System.Drawing;

namespace GameHelper.Ui; 

public class DrawLog {
    
    public void Draw() {
        ImGui.Begin("Trader", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
        var draw_ptr = ImGui.GetWindowDrawList();
        foreach (var l in Core.log) {
            var ac = Color.FromArgb(255, 10, 10, 10).ToImgui();
            if (l.mtype == MessType.Warning)
                ac = Color.FromArgb(100, Color.Orange).ToImgui();
            if (l.mtype == MessType.Error)
                ac = Color.FromArgb(100, Color.Red).ToImgui();
            if (l.mtype == MessType.Critical)
                ac = Color.FromArgb(100, Color.Purple).ToImgui();
            var text = l.info + "\n";
            if (l.count != 0)
                text = l.info + " (" + l.count + ")\n";
            var sp = ImGui.GetCursorScreenPos();
            var ts = ImGui.CalcTextSize(text);
            var lt = sp;
            var rt = sp.Increase(ts.X, 0);
            var rb = sp.Increase(ts.X, ts.Y);
            var lb = sp.Increase(0, ts.Y);
            draw_ptr.AddQuadFilled(lt, rt, rb, lb, ac);
            ImGui.Text(text);
        }
        if (ImGui.Button("Clear")) {
            Core.log.Clear();
            foreach(var s in SW.registred)
                s.Value.Restart(true);
        }
        ImGuiHelper.ToolTip("cleare log, reset max frame_time for debugger");
        ImGui.End();
    }
}
