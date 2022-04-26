using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BlindBoxPlugin
{
    public class StatusWindow : Window, IDisposable
    {
        private readonly Configuration PluginConfig;

        public StatusWindow(Configuration configuration) : base("盲盒信息")
        {
            PluginConfig = configuration;

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
            var displayModeIndex = (int)PluginConfig.DisplayMode;
            if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
            {
                PluginConfig.DisplayMode = (DisplayMode)displayModeIndex;
                PluginConfig.Save();
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
                switch (PluginConfig.DisplayMode)
                {
                    case DisplayMode.All:
                        foreach (var item in blindBox.Items)
                        {
                            DrawBlindBoxItem(item, blindBox.UniqueItems.Contains(item));
                        }
                        break;
                    case DisplayMode.Acquired:
                        foreach (var item in blindBox.Items.Intersect(PluginConfig.AcquiredItems))
                        {
                            DrawBlindBoxItem(item, blindBox.UniqueItems.Contains(item));
                        }
                        break;
                    case DisplayMode.Missing:
                        foreach (var item in blindBox.Items.Except(PluginConfig.AcquiredItems))
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
        private readonly Configuration PluginConfig;

        public ConfigWindow(Configuration configuration) : base("盲盒设置")
        {
            PluginConfig = configuration;

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
            var autoUpdate = PluginConfig.AutoUpdate;
            if (ImGui.Checkbox("自动更新", ref autoUpdate))
            {
                PluginConfig.AutoUpdate = autoUpdate;
                // can save immediately on change, if you don't want to provide a "Save and Close" button
                PluginConfig.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("是否在打开信息界面时自动更新盲盒数据。");
            }
        }
    }
}
