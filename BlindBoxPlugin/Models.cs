namespace BlindBoxPlugin
{
    public enum DisplayMode
    {
        All,
        Acquired,
        Missing
    }

    public class DisplayModeNames
    {
        public static string[] Names()
        {
            return new[]
            {
                "所有",
                "已获得",
                "未获得"
            };
        }
    }
}
