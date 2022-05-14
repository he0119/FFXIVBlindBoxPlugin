using Dalamud.Configuration;
using Dalamud.Plugin;
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
