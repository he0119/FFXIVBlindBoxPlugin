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

            SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(500, 300), MaximumSize = new Vector2(float.MaxValue, float.MaxValue) };
            SizeCondition = ImGuiCond.FirstUseEver;
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
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

    public class ConfigWindow : Window, IDisposable
    {
        private readonly BlindBox Plugin;
        private string _text = "";
        private List<String> _result = new List<string>();

        public ConfigWindow(BlindBox plugin) : base("盲盒设置")
        {
            Plugin = plugin;

            // 默认为关闭
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(480, 270),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            SizeCondition = ImGuiCond.Always;
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
            {
                if (ImGui.BeginTabItem("数据转换"))
                {
                    var windowsWidth = ImGui.GetWindowWidth();
                    var text = _text;
                    ImGui.SetNextItemWidth(windowsWidth * 0.5f - 22);
                    if (ImGui.InputTextMultiline("##text", ref text, ushort.MaxValue, new Vector2(0, 0)))
                    {
                        _text = text;
                    }
                    ImGui.SameLine();
                    ImGui.Text("=>");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(windowsWidth * 0.5f - 22);
                    var result = String.Join("\n", _result);
                    ImGui.InputTextMultiline("##result", ref result, ushort.MaxValue, new Vector2(0, 0), ImGuiInputTextFlags.ReadOnly);

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
                            else
                            {
                                itemIds.Add("名称有误");
                            }
                        }
                        _result = itemIds;
                        // ImGui.SetClipboardText(String.Join(",", itemIds));
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("输出到剪贴板"))
                    {
                        ImGui.SetClipboardText(String.Join(",", _result));
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}
