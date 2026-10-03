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
            var firstPerson = System.Array.Exists(arguments, value => value == "-fogboundFirstPerson");
            var machete = System.Array.Exists(arguments, value => value == "-fogboundMachete");
            var pause = System.Array.Exists(arguments, value => value == "-fogboundPause");
            var mapTrail = System.Array.Exists(arguments, value => value == "-fogboundMapTrail");
            yield return new WaitForSecondsRealtime(1f);

            if (pause)
            {
                var director = GameDirector.Instance;
                director.SelectWeapon(WeaponType.Pistol);
                director.SetPaused(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (gameplay)
            {
                var director = GameDirector.Instance;
                director.SelectWeapon(machete ? WeaponType.Machete : WeaponType.Pistol);
                director.Player.GetComponent<CharacterController>().Move(Vector3.forward * 3.5f);
                yield return null;
                if (firstPerson)
                    director.CameraRig.TickLook(Vector2.zero, true);
                if (mapTrail)
                {
                    var path = MazePathfinder.FindPath(director.World.Layout,
                        director.World.Layout.Start, director.World.Layout.Goal);
                    for (var i = 0; i < Mathf.Min(5, path.Count); i++)
                    {
                        director.Player.transform.position = director.World.CellToWorld(path[i]) + Vector3.up * 0.2f;
                        yield return null;
                        yield return null;
                    }
                }
                yield return new WaitForSecondsRealtime(2.5f);
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                ScreenCapture.CaptureScreenshot(output, 1);
                yield return new WaitForSecondsRealtime(1f);
            }

            Debug.Log($"FOGBOUND_RUNTIME_SMOKE_PASS mode={(pause ? "pause" : gameplay ? "gameplay" : "staging")} screenshot={output}");
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
