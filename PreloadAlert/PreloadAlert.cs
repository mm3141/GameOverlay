using System;
using GameHelper.Plugin;
using ImGuiNET;

namespace PreloadAlert
{
    public sealed class PreloadAlert : PluginBase<PreloadSettings>
    {
        public override void OnDisable()
        {
            Console.WriteLine("I am now Disabled.");
        }

        public override void OnEnable()
        {
            Console.WriteLine("I am now Enabled.");
        }

        public override void DrawSettings()
        {
            ImGui.InputText("testing", ref this.Settings.DummySettings, 100);
            ImGui.Checkbox("Enable##PreloadAlert", ref this.Settings.Enable);
        }

        public override void OnLoad()
        {
            System.Console.WriteLine($"I am loaded and my directory is {this.DllDirectory}");
        }

        public override void DrawUI()
        {
            ImGui.Begin("Preload Alert");
            ImGui.Text("Hey There!!!!");
            ImGui.End();
        }
    }
}
