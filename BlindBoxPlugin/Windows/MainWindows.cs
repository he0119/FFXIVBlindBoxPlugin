using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BlindBoxPlugin.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private readonly Plugin Plugin;

        // We give this window a hidden ID using ##
        // So that the user will see "My Amazing Window" as window title,
        // but for ImGui the ID is "My Amazing Window##With a hidden ID"
        public MainWindow(Plugin plugin) : base("盲盒信息")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(500, 300), MaximumSize = new Vector2(float.MaxValue, float.MaxValue) };
            SizeCondition = ImGuiCond.FirstUseEver;

            Plugin = plugin;
        }

        public void Dispose() { }

        public override void Draw()
        {
            // 选择盲盒显示内容
            var displayModes = Enum.GetNames<DisplayMode>();
            var displayModeIndex = (int)Plugin.PluginConfig.DisplayMode;
            ImGui.SetNextItemWidth(80);
            if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
            {
                Plugin.PluginConfig.DisplayMode = (DisplayMode)displayModeIndex;
                Plugin.PluginConfig.Save();
            }

            // 盲盒选择
            var windowsWidth = ImGui.GetWindowWidth();
            if (ImGui.BeginChild("Selectors", new Vector2(windowsWidth * 0.4f, -1), true))
            {
                foreach (var item in BlindBoxData.BlindBoxInfoMap)
                {
                    var blindbox = item.Value;
                    if (ImGui.Selectable(blindbox.Item.Name, blindbox.Item.RowId == Plugin.PluginConfig.SelectedItem))
                    {
                        Plugin.PluginConfig.SelectedItem = blindbox.Item.RowId;
                        Plugin.PluginConfig.Save();
                    }
                }
                ImGui.EndChild();
            }
            ImGui.SameLine();
            // 盲盒内容
            if (ImGui.BeginChild("Contents", new Vector2(-1, -1), true))
            {
                if (BlindBoxData.BlindBoxInfoMap.TryGetValue(Plugin.PluginConfig.SelectedItem, out var blindBox))
                {
                    switch (Plugin.PluginConfig.DisplayMode)
                    {
                        case DisplayMode.All:
                            foreach (var item in blindBox.Items)
                            {
                                DrawBlindBoxItem(item.Name, blindBox.UniqueItems.Contains(item));
                            }
                            break;
                        case DisplayMode.Acquired:
                            foreach (var item in blindBox.AcquiredItems)
                            {
                                DrawBlindBoxItem(item.Name, blindBox.UniqueItems.Contains(item));
                            }
                            break;
                        case DisplayMode.Missing:
                            foreach (var item in blindBox.MissingItems)
                            {
                                DrawBlindBoxItem(item.Name, blindBox.UniqueItems.Contains(item));
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    ImGui.Text("请选择一个盲盒");
                }
                ImGui.EndChild();
            }
        }
        private static void DrawBlindBoxItem(string name, bool unique)
        {
            if (unique)
            {
                ImGui.Text("*");
                ImGui.SameLine();
            }
            ImGui.Text(name);
        }
    }
}
