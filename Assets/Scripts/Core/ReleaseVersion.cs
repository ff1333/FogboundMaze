namespace FogboundMaze
{
    public static class ReleaseVersion
    {
        public const string Current = "1.3.0";
        public const int AndroidCode = 9;
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
