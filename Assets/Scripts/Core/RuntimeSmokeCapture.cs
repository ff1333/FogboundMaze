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
            var walkRoute = System.Array.Exists(arguments, value => value == "-fogboundWalkRoute");
            var healthCheck = System.Array.Exists(arguments, value => value == "-fogboundHealthCheck");
            var fatalHit = System.Array.Exists(arguments, value => value == "-fogboundFatalHit");
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
                if (!walkRoute)
                    director.Player.GetComponent<CharacterController>().Move(Vector3.forward * 3.5f);
                yield return null;
                if (firstPerson)
                    director.CameraRig.TickLook(Vector2.zero, true);
                if (walkRoute)
                {
                    var controller = director.Player.GetComponent<CharacterController>();
                    var path = MazePathfinder.FindPath(director.World.Layout,
                        director.World.Layout.Start, director.World.Layout.Goal);
                    var count = Mathf.Min(6, path.Count);
                    for (var i = 0; i < count; i++)
                    {
                        var destination = director.World.CellToWorld(path[i]);
                        for (var step = 0; step < 120; step++)
                        {
                            var delta = destination - director.Player.transform.position;
                            delta.y = 0f;
                            if (delta.magnitude < 0.1f) break;
                            var yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
                            director.CameraRig.TickLook(new Vector2(Mathf.DeltaAngle(director.CameraRig.Yaw, yaw), 0f), false);
                            controller.Move(Vector3.ClampMagnitude(delta, 0.18f));
                            yield return null;
                        }
                        var remaining = destination - director.Player.transform.position;
                        remaining.y = 0f;
                        if (remaining.magnitude >= 0.1f || director.Player.transform.position.y < -0.2f)
                        {
                            Debug.LogError($"FOGBOUND_WALK_FAIL cell={path[i]} position={director.Player.transform.position}");
                            Application.Quit(2);
                            yield break;
                        }
                    }
                    Debug.Log($"FOGBOUND_WALK_PASS cells={count} position={director.Player.transform.position}");
                }
                if (mapTrail)
                {
                    var path = MazePathfinder.FindPath(director.World.Layout,
                        director.World.Layout.Start, director.World.Layout.Goal);
                    for (var i = 0; i < Mathf.Min(5, path.Count); i++)
                    {
                        director.Player.Teleport(director.World.CellToWorld(path[i]) + Vector3.up * 0.2f);
                        yield return null;
                        yield return null;
                    }
                }
                yield return new WaitForSecondsRealtime(2.5f);
            }

            if (healthCheck || fatalHit)
            {
                var director = GameDirector.Instance;
                director.Player.Health.Damage(fatalHit ? 200f : 65f);
                Canvas.ForceUpdateCanvases();
                var top = director.Hud.transform.Find("Top Bar");
                var text = top.Find("Health").GetComponent<UnityEngine.UI.Text>().text;
                var track = top.Find("Health Track").GetComponent<RectTransform>();
                var fill = track.Find("Health Fill").GetComponent<RectTransform>();
                var ratio = fill.rect.width / track.rect.width;
                var expected = fatalHit ? 0f : 0.35f;
                if (Mathf.Abs(ratio - expected) > 0.005f
                    || text != (fatalHit ? "HP  0 / 100" : "HP  35 / 100")
                    || (fatalHit && director.Phase != GamePhase.Lost))
                {
                    Debug.LogError($"FOGBOUND_HEALTH_FAIL text={text} ratio={ratio} phase={director.Phase}");
                    Application.Quit(2);
                    yield break;
                }
                Debug.Log($"FOGBOUND_HEALTH_PASS text={text} ratio={ratio} phase={director.Phase}");
                yield return null;
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
