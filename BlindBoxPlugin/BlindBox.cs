using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace BlindBoxPlugin
{
    public sealed class BlindBox : IDalamudPlugin
    {
        public string Name => "Blind Box";
        private const string commandName = "/blindbox";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static DataManager DataManager { get; private set; } = null!;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;

        public readonly Configuration PluginConfig;

        private readonly XivCommonBase Common;

        private readonly WindowSystem windowSystem = new("BlindBox");
        public readonly StatusWindow statusWindow;
        public readonly ConfigWindow configWindow;

        public BlindBox()
        {
            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PluginConfig.Initialize(PluginInterface);
            Common = new XivCommonBase(Hooks.Tooltips);
            Common.Functions.Tooltips.OnItemTooltip += OnItemTooltip;

            statusWindow = new StatusWindow(this);
            configWindow = new ConfigWindow(this);
            windowSystem.AddWindow(statusWindow);
            windowSystem.AddWindow(configWindow);

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开盲盒信息界面。"
            });

            PluginInterface.UiBuilder.Draw += Draw;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(commandName);
            windowSystem.RemoveAllWindows();
            PluginInterface.UiBuilder.Draw -= Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            Common.Functions.Tooltips.OnItemTooltip -= OnItemTooltip;
            Common.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            if (args == "config")
            {
                configWindow.IsOpen = true;
            }
            else
            {
                statusWindow.IsOpen = true;
            }
        }

        private void Draw()
        {
            windowSystem.Draw();
        }

        private void OpenConfigUi()
        {
            configWindow.IsOpen = true;
        }

        private void OnItemTooltip(ItemTooltip tooltip, ulong itemId)
        {
            if (itemId > 1_000_000)
            {
                itemId -= 1_000_000;
            }
            var item = DataManager.GetExcelSheet<Item>()!.GetRow((uint)itemId);
            if (item == null)
            {
                return;
            }

            PluginLog.Verbose($"[BlindBox] OnItemTooltip: {item.Name}({itemId})");

            if (BlindBoxData.BlindBoxInfoMap.TryGetValue(itemId, out var blindbox))
            {
                var description = tooltip[ItemTooltipString.Description];

                var text = $"\n已获得：{blindbox.AcquiredItems.Count()}/{blindbox.Items.Count}";
                description.Payloads.Add(new TextPayload(text));

                tooltip[ItemTooltipString.Description] = description;
            }

        }
    }
}
