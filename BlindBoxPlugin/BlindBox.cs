using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private delegate byte HasItemActionUnlockedDelegate(IntPtr mem);
        private readonly HasItemActionUnlockedDelegate _hasItemActionUnlocked;

        public BlindBox()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            PluginUi = new PluginUI(Configuration);

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "显示盲盒中物品信息。"
            });

            PluginInterface.UiBuilder.Draw += Draw;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            var hasIaUnlockedPtr = SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");

            if (hasIaUnlockedPtr == IntPtr.Zero)
            {
                throw new ApplicationException("Could not get pointers for game functions");
            }

            _hasItemActionUnlocked = Marshal.GetDelegateForFunctionPointer<HasItemActionUnlockedDelegate>(hasIaUnlockedPtr);
        }

        public void Dispose()
        {
            PluginUi.Dispose();
            PluginInterface.UiBuilder.Draw -= Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
            CommandManager.RemoveHandler(commandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            PluginUi.Visible = true;
            RefreshMinionList();
        }

        private void RefreshMinionList()
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
                if (type != ActionType.Cards && HasItemActionUnlocked(item))
                {
                    if (type == ActionType.Minions)
                    {
                        minions.Add(item.Name);
                    }
                    else if (type == ActionType.Mounts)
                    {
                        mounts.Add(item.Name);
                    }
                }
            }
            
            // 保存已有物品数据
            Configuration.Minions = minions;
            Configuration.Mounts = mounts;
            Configuration.Save();

            Chat.Print("盲盒数据更新成功！");
        }

        private enum ActionType : ushort
        {
            Minions = 853, // minions
            Bardings = 1_013, // bardings
            Mounts = 1_322, // mounts
            CrafterBooks = 2_136, // crafter books
            Miscellaneous = 2_633, // riding maps, blu totems, emotes/dances, hairstyles
            Cards = 3_357, // cards
            GathererBooks = 4_107, // gatherer books
            OrchestrionRolls = 25_183, // orchestrion rolls
                                       // these appear to be server-side
                                       // FieldNotes = 19_743, // bozjan field notes
            FashionAccessories = 20_086, // fashion accessories
                                         // missing: 2_894 (always false)
        }

        private unsafe bool HasItemActionUnlocked(Item item)
        {
            var itemAction = item.ItemAction.Value;
            if (itemAction == null)
            {
                return false;
            }

            var type = (ActionType)itemAction.Type;

            var mem = Marshal.AllocHGlobal(256);
            *(uint*)(mem + 142) = itemAction.RowId;

            if (type == ActionType.OrchestrionRolls)
            {
                *(uint*)(mem + 112) = item.AdditionalData;
            }

            var ret = this._hasItemActionUnlocked(mem) == 1;

            Marshal.FreeHGlobal(mem);

            return ret;
        }

        private void Draw()
        {
            this.PluginUi.Draw();
        }

        private void OpenConfigUi()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
