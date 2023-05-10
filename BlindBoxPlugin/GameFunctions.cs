// 物品获取信息
// https://github.com/VergilGao/GoodMemoryCN/blob/master/GoodMemory/GameFunctions.cs

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.GeneratedSheets;
using System;

namespace BlindBoxPlugin
{
    public unsafe class GameFunctions
    {
        public static bool HasAcquired(Item item)
        {
            var action = item.ItemAction.Value;

            if (action == null)
            {
                return false;
            }

            var type = (ActionType)action.Type;

            if (type != ActionType.Cards)
            {
                var itemExdPtr = ExdModule.GetItemRowById(item.RowId);
                if (itemExdPtr != null)
                {
                    return UIState.Instance()->IsItemActionUnlocked(itemExdPtr) == 1;
                }

                return false;
            }

            var cardId = item.AdditionalData;
            var card = BlindBox.DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
            return card != null && UIState.Instance()->IsTripleTriadCardUnlocked((ushort)card.RowId);
        }
    }
}
