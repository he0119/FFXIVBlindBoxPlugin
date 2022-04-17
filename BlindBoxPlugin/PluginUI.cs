using ImGuiNET;
using System;
using System.Collections.Generic;
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
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

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
                ImGui.Text($"拥有的宠物数量 {configuration.Minions.Count}");
                ImGui.Text($"拥有的坐骑数量 {configuration.Mounts.Count}");

                var BlindBoxData = new BlindBoxData();

                List<string> MyItem = new List<string>();
                MyItem.AddRange(configuration.Minions);
                MyItem.AddRange(configuration.Mounts);


                if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
                {
                    if (ImGui.BeginTabItem("特殊配给货箱（红莲）"))
                    {
                        ImGui.BeginChild("物品", new Vector2(-1, -1), false);
                        foreach (var item in BlindBoxData.MaterielContainer40)
                        {
                            if (!MyItem.Contains(item))
                            {
                                ImGui.Text(item);
                            }
                        }
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("特殊配给货箱（重生/苍穹）"))
                    {
                        ImGui.BeginChild("物品", new Vector2(-1, -1), false);
                        foreach (var item in BlindBoxData.MaterielContainer30)
                        {
                            if (!MyItem.Contains(item))
                            {
                                ImGui.Text(item);
                            }
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
            if (ImGui.Begin("设置", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text("这里空空如也。");
            }
            ImGui.End();
        }
    }
}
