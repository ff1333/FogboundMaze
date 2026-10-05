using FogboundMaze;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public sealed class ReleaseVersionSync : IPreprocessBuildWithReport
{
    static ReleaseVersionSync()
    {
        EditorApplication.delayCall += Apply;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode) Apply();
        };
    }
    public int callbackOrder => -1000;
    public void OnPreprocessBuild(BuildReport report) => Apply();
    private static void Apply()
    {
        if (PlayerSettings.bundleVersion != ReleaseVersion.Current)
            PlayerSettings.bundleVersion = ReleaseVersion.Current;
        if (PlayerSettings.Android.bundleVersionCode != ReleaseVersion.AndroidCode)
            PlayerSettings.Android.bundleVersionCode = ReleaseVersion.AndroidCode;
    }
}
