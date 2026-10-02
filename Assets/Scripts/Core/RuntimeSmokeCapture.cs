using System.Collections;
using System.IO;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class RuntimeSmokeCapture : MonoBehaviour
    {
        private IEnumerator Start()
        {
            var arguments = System.Environment.GetCommandLineArgs();
            var output = ReadArgument(arguments, "-fogboundScreenshot");
            var gameplay = System.Array.Exists(arguments, value => value == "-fogboundGameplay");
            yield return new WaitForSecondsRealtime(1f);

            if (gameplay)
            {
                GameDirector.Instance.SelectWeapon(WeaponType.Pistol);
                GameDirector.Instance.BeginRun();
                GameDirector.Instance.Player.transform.position = GameDirector.Instance.World.EntryPosition + Vector3.up * 0.2f;
                yield return new WaitForSecondsRealtime(2.5f);
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                ScreenCapture.CaptureScreenshot(output, 1);
                yield return new WaitForSecondsRealtime(1f);
            }

            Debug.Log($"FOGBOUND_RUNTIME_SMOKE_PASS mode={(gameplay ? "gameplay" : "staging")} screenshot={output}");
            Application.Quit(0);
        }

        private static string ReadArgument(string[] arguments, string name)
        {
            for (var i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name) return arguments[i + 1];
            }
            return string.Empty;
        }
    }
}
