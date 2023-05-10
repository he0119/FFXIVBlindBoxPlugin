// 物品获取信息


using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.GeneratedSheets;

namespace BlindBoxPlugin
{
    public unsafe class GameFunctions
    {
        /// <summary>
        /// https://github.com/Critical-Impact/CriticalCommonLib/blob/d00ed298fd08795c66ed93975144fdec27e1f544/Services/GameInterface.cs#L74
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
