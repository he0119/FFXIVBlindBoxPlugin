using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;

namespace BlindBoxPlugin
{
    public class BlindBoxInfo
    {
        public Item Item;
        public List<Item> Items;
        public List<Item> UniqueItems = new();

        public BlindBoxInfo(uint id, List<uint> itemIds)
        {
            Item = GetItem(id);
            Items = GetItems(itemIds);
        }
        public BlindBoxInfo(uint id, List<uint> itemIds, List<uint> uniqueItemIds)
        {
            Item = GetItem(id);
            Items = GetItems(itemIds);
            UniqueItems = GetItems(uniqueItemIds);
        }

        private static Item GetItem(uint id)
        {
            var item = BlindBox.DataManager.GetExcelSheet<Item>()?.GetRow(id);
            if (item == null) return new Item();
            return item;
        }

        private static List<Item> GetItems(List<uint> ids)
        {
            return ids.Select(id => GetItem(id)).ToList();
        }

        public List<Item> AcquiredItems
        {
            get => Items.Where(item => GameFunctions.HasAcquired(item)).ToList();
        }

        public List<Item> MissingItems
        {
            get => Items.Where(item => !GameFunctions.HasAcquired(item)).ToList();
        }
    }

    public class BlindBoxData
    {
        public static readonly Dictionary<ulong, BlindBoxInfo> BlindBoxInfoMap = new()
        {
            // 特殊配给货箱（重生/苍穹）
            [36635] = new BlindBoxInfo(
                36635,
                new() { 14083, 16564, 6003, 6004, 6005, 6168, 6173, 6174, 6175, 6177, 6179, 6184, 6185, 6187, 6188, 6189, 6190, 6191, 6192, 6195, 6199, 6200, 6203, 6204, 6205, 6208, 6213, 6214, 7559, 7564, 7566, 7567, 7568, 8193, 8194, 8195, 8196, 8199, 8200, 8201, 8202, 8205, 9347, 9348, 9349, 9350, 10071, 12049, 12051, 12052, 12054, 12055, 12056, 12057, 12058, 12061, 12062, 12069, 13275, 13276, 13279, 13283, 13284, 14093, 14094, 14095, 14098, 14100, 14103, 15436, 15437, 15440, 15441, 15447, 16568, 16570, 16572, 16572, 16573, 17525, 17527 }
            ),
            // 特殊配给货箱（红莲）
            [36636] = new BlindBoxInfo(
                36636,
                new() { 21052, 23023, 24219, 24630, 20524, 20525, 20528, 20529, 20530, 20531, 20533, 20536, 20537, 20538, 20539, 20541, 20542, 20544, 20545, 20546, 20547, 21055, 21057, 21058, 21059, 21060, 21063, 21064, 21065, 21193, 21907, 21911, 21915, 21916, 21917, 21918, 21919, 21920, 21921, 21922, 23027, 23028, 23030, 23032, 23036, 23989, 23998, 24000, 24001, 24002, 24634, 24635, 24639, 24640, 24902, 24903 }
            ),
            // 九宫幻卡白金包
            [10077] = new BlindBoxInfo(
                10077,
                new() { 9822, 9826, 9827, 9828, 9834, 9840, 9842, 9848, 9851, 9830, 14208 },
                new() { 9828, 9840, 9842, 9848, 9851 }
            ),
            // 九宫幻卡铜包
            [10128] = new BlindBoxInfo(
                10128,
                new() { 9775, 9776, 9779, 9782, 9783, 16759, 9795, 9796, 9797, 9798, 16760, 16762, 9809, 15621, 16765 },
                new() { 16759, 16760, 16762 }
            ),
            // 九宫幻卡银包
            [10129] = new BlindBoxInfo(
                10129,
                new() { 9785, 9786, 9787, 9788, 9790, 9792, 9812, 9814, 9821, 14199, 9827, 9828, 9811, 9813 },
                new() { 9788, 9790, 9827, 9828 }
            ),
            // 九宫幻卡金包
            [10130] = new BlindBoxInfo(
                10130,
                new() { 9799, 9800, 9801, 9805, 9822, 9829, 9839, 9847, 14192, 9825, 9824, 9826, 9837, 9836, 9838 },
                new() { 9839, 9847, 14192 }
            ),
            // 九宫幻卡灵银包
            [13380] = new BlindBoxInfo(
                13380,
                new() { 9810, 9823, 9841, 9843, 9844, 13367, 13368, 13372, 14193 }
            ),
            // 帝国九宫幻卡包
            [17702] = new BlindBoxInfo(
                17702,
                new() { 13378, 16774, 16775, 17681, 17682, 17686 },
                new() { 16774, 16775, 17681, 17682, 17686 }
            ),
            // 九宫幻卡梦想包
            [28652] = new BlindBoxInfo(
                28652,
                new() { 26765, 26766, 26767, 26768, 26772, 28653, 28655, 28657, 28658, 28660, 28661 },
                new() { 28653, 28655, 28657, 28658, 28660, 28661 }
            ),
        };
    }
}
