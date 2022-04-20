using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

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
        private readonly GameFunctions gameFunctions;

        private readonly WindowSystem windowSystem = new("BlindBox");
        private readonly StatusWindow statusWindow;
        private readonly ConfigWindow configWindow;

        public BlindBox()
        {
            configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(PluginInterface);
            gameFunctions = new GameFunctions(DataManager, SigScanner);

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

        //
        private void UpdateAcquiredList()
        {
            List<string> minions = new();
            List<string> mounts = new();

            var items = DataManager.GetExcelSheet<Item>()!;
            foreach (var item in items)
            {
                var action = item.ItemAction.Value;
                if (action == null)
                {
                    continue;
                }
                var type = (ActionType)action.Type;
                if (type == ActionType.Minions && gameFunctions.HasAcquired(item))
                {
                    minions.Add(item.Name);
                }
                if (type == ActionType.Mounts && gameFunctions.HasAcquired(item))
                {
                    mounts.Add(item.Name);
                }
            }

            // 保存已有物品数据
            configuration.Minions = minions;
            configuration.Mounts = mounts;
            configuration.Save();

            Chat.Print("盲盒数据更新成功！");
        }
    }
}
