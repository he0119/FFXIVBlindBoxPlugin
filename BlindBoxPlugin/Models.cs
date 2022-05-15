namespace BlindBoxPlugin
{
    public enum ActionType : ushort
    {
        Minions = 853, // minions
        Bardings = 1_013, // bardings
        Mounts = 1_322, // mounts
        CrafterBooks = 2_136, // crafter books
        Miscellaneous = 2_633, // riding maps, blu totems, emotes/dances, hairstyles
        Cards = 3_357, // cards
        GathererBooks = 4_107, // gatherer books
        OrchestrionRolls = 25_183, // orchestrion rolls
                                   // these appear to be server-side
                                   // FieldNotes = 19_743, // bozjan field notes
        FashionAccessories = 20_086, // fashion accessories
                                     // missing: 2_894 (always false)
    }
    public enum DisplayMode
    {
        All,
        Acquired,
        Missing
    }

    public static class DisplayModeExtensions
    {
        public static string Names(this DisplayMode displayMode)
        {
            switch (displayMode)
            {
                case DisplayMode.All:
                    return "所有";
                case DisplayMode.Acquired:
                    return "已获得";
                case DisplayMode.Missing:
                    return "未获得";
                default:
                    return "未知";
            }
        }
    }
}
