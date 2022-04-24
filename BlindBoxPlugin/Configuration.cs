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
        /// 已经拥有的物品
        /// </summary>
        public List<string> AcquiredItems = new();

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
