// 物品获取信息

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin
{
    public unsafe class GameFunctions
    {
        /// <summary>
        /// https://github.com/Critical-Impact/CriticalCommonLib/blob/042c3b21cce7c5667814daa622d1c66af517d263/Services/UnlockTrackerService.cs#L110-L177
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsUnlocked(Item item)
        {
            bool? unlocked = null;
            if (item.RowId == 0)
                return false;

            switch ((ActionType)item.ItemAction.Value.Type)
            {
                case ActionType.Companion:
                    unlocked = UIState.Instance()->IsCompanionUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.BuddyEquip:
                    unlocked = UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.Mount:
                    unlocked = PlayerState.Instance()->IsMountUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.SecretRecipeBook:
                    unlocked = PlayerState.Instance()->IsSecretRecipeBookUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.UnlockLink:
                    unlocked = UIState.Instance()->IsUnlockLinkUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.TripleTriadCard when item.AdditionalData.Is<TripleTriadCard>():
                    unlocked = UIState.Instance()->IsTripleTriadCardUnlocked((ushort)item.AdditionalData.RowId);
                    break;
                case ActionType.FolkloreTome:
                    unlocked = PlayerState.Instance()->IsFolkloreBookUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.OrchestrionRoll when item.AdditionalData.Is<Orchestrion>():
                    unlocked = PlayerState.Instance()->IsOrchestrionRollUnlocked(item.AdditionalData.RowId);
                    break;
                case ActionType.FramersKit:
                    unlocked = PlayerState.Instance()->IsFramersKitUnlocked(item.AdditionalData.RowId);
                    break;
                case ActionType.Ornament:
                    unlocked = PlayerState.Instance()->IsOrnamentUnlocked(item.ItemAction.Value.Data[0]);
                    break;
                case ActionType.Glasses:
                    unlocked = PlayerState.Instance()->IsGlassesUnlocked((ushort)item.AdditionalData.RowId);
                    break;
            }

            if (unlocked != null)
            {
                return (bool)unlocked;
            }

            var row = ExdModule.GetItemRowById(item.RowId);
            if (row == null) return false;
            return UIState.Instance()->IsItemActionUnlocked(row) == 1;
        }
    }
}
