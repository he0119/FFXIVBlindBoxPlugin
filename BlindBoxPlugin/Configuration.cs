using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace BlindBoxPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        // 打开界面时自动更新获得物品信息
        public bool AutoUpdate { get; set; } = true;
        // 显示盲盒物品模式
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Missing;

        /// <summary>
        /// 已经拥有的宠物
        /// </summary>
        public List<string> Minions = new();
        /// <summary>
        /// 已经拥有的坐骑
        /// </summary>
        public List<string> Mounts = new();
        /// <summary>
        /// 已经拥有的幻卡
        /// </summary>
        public List<string> Cards = new();

        // the below exist just to make saving less cumbersome
        public List<string> AcquiredItems()
        {
            List<string> acquiredItems = new();
            acquiredItems.AddRange(Minions);
            acquiredItems.AddRange(Mounts);
            return acquiredItems;
        }

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
