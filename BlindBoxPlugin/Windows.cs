using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace BlindBoxPlugin
{
    public class StatusWindow : Window, IDisposable
    {
        private readonly BlindBox Plugin;

        public StatusWindow(BlindBox plugin) : base("盲盒信息")
        {
            Plugin = plugin;

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
            var displayModeIndex = (int)Plugin.PluginConfig.DisplayMode;
            if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
            {
                Plugin.PluginConfig.DisplayMode = (DisplayMode)displayModeIndex;
                Plugin.PluginConfig.Save();
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
            if (ImGui.BeginTabItem(blindBox.Item.Name))
            {
                ImGui.BeginChild("items", new Vector2(-1, -1), false);
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
        private readonly BlindBox Plugin;

        public ConfigWindow(BlindBox plugin) : base("盲盒设置")
        {
            Plugin = plugin;

            // 默认为关闭
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(230, 75),
                MaximumSize = new Vector2(230, 75)
            };
            SizeCondition = ImGuiCond.Always;
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() { }

        public override void Draw()
        {
            // can't ref a property, so use a local copy
            if (ImGui.Button("转换数据"))
            {
                Plugin.convertWindow.IsOpen = true;
            }
        }
    }

    public class ConvertWindow : Window, IDisposable
    {
        private readonly BlindBox Plugin;
        private string _text = "";

        public ConvertWindow(BlindBox plugin) : base("转换数据")
        {
            Plugin = plugin;

            // 默认为关闭
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(600, 400),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            SizeCondition = ImGuiCond.Always;
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() { }

        public override void Draw()
        {
            var text = _text;
            if (ImGui.InputTextMultiline("##text", ref text, 10000, new Vector2(0, 0)))
            {
                _text = text;
            }

            if (ImGui.Button("转换"))
            {
                var items = _text.Split('\n');
                List<String> itemIds = new();

                foreach (var item in items)
                {
                    var i = BlindBox.DataManager.GetExcelSheet<Item>()?.Where(i => i.Name == item).FirstOrDefault();
                    if (i != null)
                    {
                        itemIds.Add(i.RowId.ToString());
                    }
                }
                ImGui.SetClipboardText(String.Join(",", itemIds));
            }
        }
    }
}
