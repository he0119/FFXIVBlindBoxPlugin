// 物品获取信息
// https://github.com/VergilGao/GoodMemoryCN/blob/master/GoodMemory/GameFunctions.cs

using Lumina.Excel.GeneratedSheets;
using System;
using System.Runtime.InteropServices;

namespace BlindBoxPlugin
{
    public class GameFunctions
    {
        private static bool initialized = false;

        private delegate byte HasCardDelegate(IntPtr localPlayer, ushort cardId);
        private unsafe delegate byte HasItemActionUnlockedDelegate(long mem, long a2, long* a3);
        private delegate long ItemToUlongDelegate(uint id);

        private static IntPtr _cardStaticAddr;
        private static HasCardDelegate _hasCard = null!;
        private static HasItemActionUnlockedDelegate _hasItemActionUnlocked = null!;
        private static ItemToUlongDelegate _itemToUlong = null!;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var hasIaUnlockedPtr = BlindBox.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A6 32 C0");
            var hasCardPtr = BlindBox.SigScanner.ScanText("40 53 48 83 EC 20 48 8B D9 66 85 D2 74");
            var itemToUlongPtr = BlindBox.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 85 C0 74 33 83 7F 04 00");
            _cardStaticAddr = BlindBox.SigScanner.GetStaticAddressFromSig("41 0F B7 17 48 8D 0D");

            if (hasIaUnlockedPtr == IntPtr.Zero || hasCardPtr == IntPtr.Zero || _cardStaticAddr == IntPtr.Zero || itemToUlongPtr == IntPtr.Zero)
            {
                throw new ApplicationException("Could not get pointers for game functions");
            }

            _hasItemActionUnlocked = Marshal.GetDelegateForFunctionPointer<HasItemActionUnlockedDelegate>(hasIaUnlockedPtr);
            _hasCard = Marshal.GetDelegateForFunctionPointer<HasCardDelegate>(hasCardPtr);
            _itemToUlong = Marshal.GetDelegateForFunctionPointer<ItemToUlongDelegate>(itemToUlongPtr);
        }

        public static bool HasAcquired(Item item)
        {
            if (!initialized) throw new ApplicationException("GameFunctions not initialized");

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
            var card = BlindBox.DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
            return card != null && HasCard((ushort)card.RowId);
        }

        private static unsafe bool HasItemActionUnlocked(Item item)
        {
            var itemAction = item.ItemAction.Value;
            if (itemAction == null)
            {
                return false;
            }

            long itemId = _itemToUlong(item.RowId);
            if (itemId == 0L)
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
            var ret = _hasItemActionUnlocked(itemId, 0L, (long*)mem) == 1;

            Marshal.FreeHGlobal(mem);

            return ret;
        }

        private static bool HasCard(ushort cardId)
        {
            return _hasCard(_cardStaticAddr, cardId) == 1;
        }
    }
}
