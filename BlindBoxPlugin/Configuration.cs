using Dalamud.Configuration;
using System;

namespace BlindBoxPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public uint SelectedItem { get; set; } = 0;

        // 显示盲盒物品模式
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Missing;

        // the below exist just to make saving less cumbersome
        public void Save()
        {
            Plugin.PluginInterface.SavePluginConfig(this);
        }
    }
}
