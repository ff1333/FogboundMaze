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
            var levels = System.Array.Exists(arguments, value => value == "-fogboundLevels");
            var loadout = System.Array.Exists(arguments, value => value == "-fogboundLoadout");
            var actors = System.Array.Exists(arguments, value => value == "-fogboundActors");
            var guide = System.Array.Exists(arguments, value => value == "-fogboundGuide");
            var combat = System.Array.Exists(arguments, value => value == "-fogboundCombat");
            yield return new WaitForSecondsRealtime(1f);
            if (Application.version != ReleaseVersion.Current)
            {
                Debug.LogError($"FOGBOUND_VERSION_FAIL expected={ReleaseVersion.Current} actual={Application.version}");
                Application.Quit(2);
                yield break;
            }
            Debug.Log($"FOGBOUND_VERSION_PASS {Application.version}");
            if (guide)
            {
                GameDirector.Instance.ShowGuide();
                GameDirector.Instance.Hud.transform.Find("Field Guide/Chapter 7")
                    .GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            }

            if (levels || loadout || pause || gameplay || actors)
            {
                GameDirector.Instance.ShowLevelSelection();
                if (!levels) GameDirector.Instance.SelectLevel(1);
                yield return null;
            }

            if (actors)
            {
                var director = GameDirector.Instance;
                director.SelectWeapon(machete ? WeaponType.LongBlade : WeaponType.SubmachineGun);
                var pool = FindFirstObjectByType<EnemyPool>();
                foreach (var elite in new[] { false, true })
                {
                    var enemy = pool.Spawn(director.Player, director.World,
                        director.World.EntryPosition + new Vector3(elite ? 1.1f : -1.1f, 0f, elite ? 0.8f : 0f), elite);
                    enemy.transform.LookAt(director.Player.transform.position);
                }
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (pause)
            {
                var director = GameDirector.Instance;
                director.SelectWeapon(WeaponType.SubmachineGun);
                director.SetPaused(true);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else if (gameplay)
            {
                var director = GameDirector.Instance;
                director.SelectWeapon(machete ? WeaponType.LongBlade : WeaponType.SubmachineGun);
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
                if (combat)
                {
                    GameDirector.Instance.Player.Weapon.Tick(true,true,false);
                    if (machete) yield return new WaitForSeconds(.15f);
                    Time.timeScale = 0f;
                    yield return null;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                CaptureFrame(output);
                if (!File.Exists(output))
                {
                    Debug.LogError("FOGBOUND_SCREENSHOT_FAIL " + output);
                    Application.Quit(2);
                    yield break;
                }
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

        private static void CaptureFrame(string output)
        {
            // Render explicitly so a hidden/background smoke-test window still produces evidence.
            var camera = GameDirector.Instance.CameraRig.Camera;
            var canvas = GameDirector.Instance.Hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = .31f;
            Canvas.ForceUpdateCanvases();
            var target = new RenderTexture(Screen.width, Screen.height, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);
            texture.Apply();
            File.WriteAllBytes(output, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Destroy(target);
            Destroy(texture);
        }
    }
}
