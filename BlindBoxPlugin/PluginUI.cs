using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BlindBoxPlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose() { }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(600, 400), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(600, 400), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("盲盒信息", ref visible,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                List<string> AcquiredItems = new();
                AcquiredItems.AddRange(configuration.Minions);
                AcquiredItems.AddRange(configuration.Mounts);

                ImGui.Text($"特殊配给货箱（红莲）：{BlindBoxData.MaterielContainer40.Except(AcquiredItems).Count()}/{BlindBoxData.MaterielContainer40.Count}");
                ImGui.Text($"特殊配给货箱（重生/苍穹）：{BlindBoxData.MaterielContainer30.Except(AcquiredItems).Count()}/{BlindBoxData.MaterielContainer30.Count}");

                if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
                {
                    if (ImGui.BeginTabItem("特殊配给货箱（红莲）"))
                    {
                        ImGui.BeginChild("物品", new Vector2(-1, -1), false);
                        foreach (var item in BlindBoxData.MaterielContainer40.Except(AcquiredItems))
                        {
                            ImGui.Text(item);
                        }
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("特殊配给货箱（重生/苍穹）"))
                    {
                        ImGui.BeginChild("物品", new Vector2(-1, -1), false);
                        foreach (var item in BlindBoxData.MaterielContainer30.Except(AcquiredItems))
                        {
                            ImGui.Text(item);
                        }
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                }
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("设置", ref settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var autoUpdate = configuration.AutoUpdate;
                if (ImGui.Checkbox("打开页面时自动更新", ref autoUpdate))
                {
                    configuration.AutoUpdate = autoUpdate;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
