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
                ImGui.Text($"特殊配给货箱（红莲）：{BlindBoxData.MaterielContainer40.Intersect(configuration.AcquiredItems).Count()}/{BlindBoxData.MaterielContainer40.Count}");
                ImGui.Text($"特殊配给货箱（重生/苍穹）：{BlindBoxData.MaterielContainer30.Intersect(configuration.AcquiredItems).Count()}/{BlindBoxData.MaterielContainer30.Count}");

                var displayModes = Enum.GetNames<DisplayMode>();
                var displayModeIndex = (int)configuration.DisplayMode;
                if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
                {
                    configuration.DisplayMode = (DisplayMode)displayModeIndex;
                    configuration.Save();
                }

                if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
                {
                    DrawBlindBoxItemTab("特殊配给货箱（红莲）", BlindBoxData.MaterielContainer40);
                    DrawBlindBoxItemTab("特殊配给货箱（重生/苍穹）", BlindBoxData.MaterielContainer30);

                    ImGui.EndTabBar();
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

            ImGui.SetNextWindowSize(new Vector2(230, 75), ImGuiCond.Always);
            if (ImGui.Begin("盲盒设置", ref settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var autoUpdate = configuration.AutoUpdate;
                if (ImGui.Checkbox("自动更新", ref autoUpdate))
                {
                    configuration.AutoUpdate = autoUpdate;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    configuration.Save();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("是否在打开信息界面时自动更新盲盒数据。");
                }
            }
            ImGui.End();
        }

        private void DrawBlindBoxItemTab(string label, List<string> blindbox)
        {
            if (ImGui.BeginTabItem(label))
            {
                ImGui.BeginChild("items", new Vector2(-1, -1), false);
                switch (configuration.DisplayMode)
                {
                    case DisplayMode.All:
                        foreach (var item in blindbox)
                        {
                            ImGui.Text(item);
                        }
                        break;
                    case DisplayMode.Acquired:
                        foreach (var item in blindbox.Intersect(configuration.AcquiredItems))
                        {
                            ImGui.Text(item);
                        }
                        break;
                    case DisplayMode.Missing:
                        foreach (var item in blindbox.Except(configuration.AcquiredItems))
                        {
                            ImGui.Text(item);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
        }
    }
}
