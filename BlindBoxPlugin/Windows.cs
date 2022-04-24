using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BlindBoxPlugin
{
    public class StatusWindow : Window, IDisposable
    {
        private readonly Configuration configuration;

        public StatusWindow(Configuration configuration) : base("盲盒信息")
        {
            this.configuration = configuration;

            // 默认为关闭
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(600, 400), MaximumSize = new Vector2(float.MaxValue, float.MaxValue) };
            SizeCondition = ImGuiCond.FirstUseEver;
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() { }

        public override void Draw()
        {
            var displayModes = Enum.GetNames<DisplayMode>();
            var displayModeIndex = (int)configuration.DisplayMode;
            if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
            {
                configuration.DisplayMode = (DisplayMode)displayModeIndex;
                configuration.Save();
            }

            if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
            {
                foreach (var item in BlindBoxData.BlindBoxInfoMap)
                {
                    var blindbox = item.Value;
                    DrawBlindBoxTab(blindbox);
                }

                ImGui.EndTabBar();
            }
        }

        private void DrawBlindBoxTab(BlindBoxInfo blindBox)
        {
            if (ImGui.BeginTabItem(blindBox.ItemName))
            {
                ImGui.BeginChild("items", new Vector2(-1, -1), false);
                switch (configuration.DisplayMode)
                {
                    case DisplayMode.All:
                        foreach (var item in blindBox.Items)
                        {
                            DrawBlindBoxItem(item, blindBox.UniqueItems.Contains(item));
                        }
                        break;
                    case DisplayMode.Acquired:
                        foreach (var item in blindBox.Items.Intersect(configuration.AcquiredItems))
                        {
                            DrawBlindBoxItem(item, blindBox.UniqueItems.Contains(item));
                        }
                        break;
                    case DisplayMode.Missing:
                        foreach (var item in blindBox.Items.Except(configuration.AcquiredItems))
                        {
                            DrawBlindBoxItem(item, blindBox.UniqueItems.Contains(item));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
        }

        private void DrawBlindBoxItem(string name, bool unique)
        {
            if (unique)
            {
                ImGui.Text("*");
                ImGui.SameLine();
            }
            ImGui.Text(name);
        }
    }

    public class ConfigWindow : Window, IDisposable
    {
        private readonly Configuration configuration;

        public ConfigWindow(Configuration configuration) : base("盲盒设置")
        {
            this.configuration = configuration;

            // 默认为关闭
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(230, 75), MaximumSize = new Vector2(230, 75) };
            SizeCondition = ImGuiCond.Always;
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() { }

        public override void Draw()
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
    }
}
