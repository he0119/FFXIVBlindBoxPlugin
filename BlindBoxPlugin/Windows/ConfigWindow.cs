using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        private readonly Configuration Configuration;

        private string _text = "";
        private List<string> _result = [];

        // We give this window a constant ID using ###
        // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
        // and the window ID will always be "###XYZ counter window" for ImGui
        public ConfigWindow(Plugin plugin) : base("盲盒设置")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(480, 270),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            SizeCondition = ImGuiCond.Always;

            Configuration = plugin.Configuration;
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
                    var result = string.Join("\n", _result);
                    ImGui.InputTextMultiline("##result", ref result, ushort.MaxValue, new Vector2(0, 0), ImGuiInputTextFlags.ReadOnly);

                    if (ImGui.Button("转换"))
                    {
                        var items = _text.Split('\n');
                        List<string> itemIds = [];

                        foreach (var item in items)
                        {
                            var i = Plugin.DataManager.GetExcelSheet<Item>()?.Where(i => i.Name == item).FirstOrDefault();
                            if (i != null)
                            {
                                // 如果名称正确，添加到结果列表
                                itemIds.Add(i.Value.RowId.ToString());
                            }
                            else
                            {
                                itemIds.Add("名称有误");
                            }
                        }
                        _result = itemIds;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("输出到剪贴板"))
                    {
                        ImGui.SetClipboardText(string.Join(",", _result));
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}
