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
        private delegate byte HasCardDelegate(IntPtr localPlayer, ushort cardId);
        private readonly HasCardDelegate _hasCard;
        private readonly IntPtr _cardStaticAddr;

        public BlindBox()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            PluginUi = new PluginUI(Configuration);

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开盲盒信息界面。"
            });

            PluginInterface.UiBuilder.Draw += Draw;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

            var hasIaUnlockedPtr = SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");
            var hasCardPtr = SigScanner.ScanText("40 53 48 83 EC 20 48 8B D9 66 85 D2 74");
            _cardStaticAddr = SigScanner.GetStaticAddressFromSig("41 0F B7 17 48 8D 0D");

            if (hasIaUnlockedPtr == IntPtr.Zero || hasCardPtr == IntPtr.Zero || _cardStaticAddr == IntPtr.Zero)
            {
                throw new ApplicationException("Could not get pointers for game functions");
            }

            _hasItemActionUnlocked = Marshal.GetDelegateForFunctionPointer<HasItemActionUnlockedDelegate>(hasIaUnlockedPtr);
            _hasCard = Marshal.GetDelegateForFunctionPointer<HasCardDelegate>(hasCardPtr);
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
            if (args == "update")
            {
                UpdateAcquiredList();
            }
            else if (args == "settings")
            {
                PluginUi.SettingsVisible = true;
            }
            else
            {
                PluginUi.Visible = true;
                if (Configuration.AutoUpdate)
                {
                    UpdateAcquiredList();
                }
            }
        }

        private void Draw()
        {
            PluginUi.Draw();
        }

        private void OpenConfigUi()
        {
            PluginUi.SettingsVisible = true;
        }

        // https://github.com/VergilGao/GoodMemoryCN/blob/master/GoodMemory/GameFunctions.cs
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
                if (type == ActionType.Minions && HasAcquired(item))
                {
                    minions.Add(item.Name);
                }
                if (type == ActionType.Mounts && HasAcquired(item))
                {
                    mounts.Add(item.Name);
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

        private bool HasAcquired(Item item)
        {
            var action = item.ItemAction.Value;

            if (action == null)
            {
                return false;
            }

            var type = (ActionType)action.Type;

            if (type != ActionType.Cards)
            {
                return HasItemActionUnlocked(item);
            }

            var cardId = item.AdditionalData;
            var card = DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
            return card != null && HasCard((ushort)card.RowId);
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

        private bool HasCard(ushort cardId)
        {
            return _hasCard(_cardStaticAddr, cardId) == 1;
        }
    }
}
