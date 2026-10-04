namespace FogboundMaze
{
    public static class ReleaseVersion
    {
        public const string Current = "1.2.1";
        public const int AndroidCode = 7;
        public static string Display
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.PlayerSettings.bundleVersion;
#else
                return UnityEngine.Application.version;
#endif
            }
        }
    }
}
