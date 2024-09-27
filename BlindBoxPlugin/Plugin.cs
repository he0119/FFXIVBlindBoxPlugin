using BlindBoxPlugin.Windows;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BlindBoxPlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;

        public string Name => "Blind Box";
        private const string CommandName = "/blindbox";

        public Configuration Configuration { get; init; }

        public readonly WindowSystem WindowSystem = new("BlindBox");
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            MainWindow = new MainWindow(this);
            ConfigWindow = new ConfigWindow(this);

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开盲盒信息界面。"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;

            // This adds a button to the plugin installer entry of this plugin which allows
            // to toggle the display status of the configuration ui
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

            // Adds another button that is doing the same but for the main ui of the plugin
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            if (args == "config")
            {
                ConfigWindow.Toggle();
            }
            else
            {
                MainWindow.Toggle();
            }
        }

        private void DrawUI() => WindowSystem.Draw();
        public void ToggleConfigUI() => ConfigWindow.Toggle();
        public void ToggleMainUI() => MainWindow.Toggle();
    }
}
