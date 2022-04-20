using Dalamud.Data;
using Dalamud.Game;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Runtime.InteropServices;

namespace BlindBoxPlugin
{
    public class GameFunctions
    {
        private readonly DataManager _DataManager;
        private readonly SigScanner _SigScanner;

        private delegate byte HasItemActionUnlockedDelegate(IntPtr mem);
        private readonly HasItemActionUnlockedDelegate _hasItemActionUnlocked;
        private delegate byte HasCardDelegate(IntPtr localPlayer, ushort cardId);
        private readonly HasCardDelegate _hasCard;
        private readonly IntPtr _cardStaticAddr;

        public GameFunctions(DataManager dataManager, SigScanner sigScanner)
        {
            _DataManager = dataManager;
            _SigScanner = sigScanner;

            var hasIaUnlockedPtr = _SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");
            var hasCardPtr = _SigScanner.ScanText("40 53 48 83 EC 20 48 8B D9 66 85 D2 74");
            this._cardStaticAddr = _SigScanner.GetStaticAddressFromSig("41 0F B7 17 48 8D 0D");

            if (hasIaUnlockedPtr == IntPtr.Zero || hasCardPtr == IntPtr.Zero || this._cardStaticAddr == IntPtr.Zero)
            {
                throw new ApplicationException("Could not get pointers for game functions");
            }

            this._hasItemActionUnlocked = Marshal.GetDelegateForFunctionPointer<HasItemActionUnlockedDelegate>(hasIaUnlockedPtr);
            this._hasCard = Marshal.GetDelegateForFunctionPointer<HasCardDelegate>(hasCardPtr);
        }

        // https://github.com/VergilGao/GoodMemoryCN/blob/master/GoodMemory/GameFunctions.cs
        public bool HasAcquired(Item item)
        {
            var action = item.ItemAction.Value;

            if (action == null)
            {
                return false;
            }

            var type = (ActionType)action.Type;

            if (type != ActionType.Cards)
            {
                return this.HasItemActionUnlocked(item);
            }

            var cardId = item.AdditionalData;
            var card = _DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
            return card != null && this.HasCard((ushort)card.RowId);
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
            return this._hasCard(this._cardStaticAddr, cardId) == 1;
        }
    }
}
