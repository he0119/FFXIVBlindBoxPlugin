using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
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
        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;

        private readonly Configuration configuration;

        private readonly GameFunctions GameFunctions;
        private readonly XivCommonBase Common;

        private readonly WindowSystem windowSystem = new("BlindBox");
        private readonly StatusWindow statusWindow;
        private readonly ConfigWindow configWindow;

        public BlindBox()
        {
            configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(PluginInterface);
            GameFunctions = new GameFunctions(DataManager, SigScanner);
            Common = new XivCommonBase(Hooks.Tooltips);
            Common.Functions.Tooltips.OnItemTooltip += OnItemTooltip;

            statusWindow = new StatusWindow(configuration);
            configWindow = new ConfigWindow(configuration);
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
            if (args == "update")
            {
                UpdateAcquiredList();
            }
            else if (args == "config")
            {
                configWindow.IsOpen = true;
            }
            else
            {
                statusWindow.IsOpen = true;
                if (configuration.AutoUpdate)
                {
                    UpdateAcquiredList();
                }
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

            PluginLog.Debug($"[BlindBox] OnItemTooltip: {item.Name}({itemId})");

            if (itemId == 36635 || itemId == 36636)
            {
                var description = tooltip[ItemTooltipString.Description];

                var blindbox = itemId == 36635 ? BlindBoxData.MaterielContainer30 : BlindBoxData.MaterielContainer40;
                var text = $"\n已获得：{blindbox.Intersect(configuration.AcquiredItems()).Count()}/{blindbox.Count}";
                description.Payloads.Add(new TextPayload(text));

                tooltip[ItemTooltipString.Description] = description;
            }

        }

        private void UpdateAcquiredList()
        {
            List<string> minions = new();
            List<string> mounts = new();
            List<string> cards = new();

            var items = DataManager.GetExcelSheet<Item>()!;
            foreach (var item in items)
            {
                var action = item.ItemAction.Value;
                if (action == null)
                {
                    continue;
                }
                var type = (ActionType)action.Type;
                if (type == ActionType.Minions && GameFunctions.HasAcquired(item))
                {
                    minions.Add(item.Name);
                }
                else if (type == ActionType.Mounts && GameFunctions.HasAcquired(item))
                {
                    mounts.Add(item.Name);
                }
                else if (type == ActionType.Cards && GameFunctions.HasAcquired(item))
                {
                    cards.Add(item.Name);
                }
            }

            // 保存已有物品数据
            configuration.Minions = minions;
            configuration.Mounts = mounts;
            configuration.Cards = cards;
            configuration.Save();

            Chat.Print("盲盒数据更新成功！");
        }
    }
}
