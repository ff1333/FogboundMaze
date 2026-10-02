using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace FogboundMaze.Tests
{
    public sealed class GameFlowTests
    {
        [UnitySetUp]
        public IEnumerator LoadMainScene()
        {
            PlayerPrefs.DeleteKey("Fogbound.SelectedLevel");
            PlayerPrefs.DeleteKey("Fogbound.UnlockedLevel");
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayingRun_SpawnsEnemyAfterVisibleTelegraphDelay()
        {
            var director = GameDirector.Instance;
            director.SelectWeapon(WeaponType.Machete);
            director.BeginRun();
            yield return new WaitForSeconds(3.1f);
            Assert.That(director.ActiveEnemyCount, Is.GreaterThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator UnifiedInput_MovesPlayerAndSwitchesCameraMode()
        {
            var director = GameDirector.Instance;
            director.SelectWeapon(WeaponType.Pistol);
            director.BeginRun();
            var start = director.Player.transform.position;
            director.Input.SetMobileMove(Vector2.right);
            yield return new WaitForSeconds(0.45f);
            director.Input.SetMobileMove(Vector2.zero);
            var horizontalDelta = director.Player.transform.position - start;
            horizontalDelta.y = 0f;
            Assert.That(horizontalDelta.magnitude, Is.GreaterThan(0.05f));

            Assert.That(director.CameraRig.IsFirstPerson, Is.False);
            director.Input.PressMobileToggle();
            yield return null;
            yield return null;
            Assert.That(director.CameraRig.IsFirstPerson, Is.True);
        }

        [UnityTest]
        public IEnumerator MiasmaOutsideSafeLight_DamagesPlayer()
        {
            var director = GameDirector.Instance;
            for (var level = 2; level <= 7; level++)
            {
                director.NextLevel();
                yield return null;
            }
            director.SelectWeapon(WeaponType.Machete);
            director.BeginRun();

            var unsafePosition = FindUnsafeCell(director);
            Assert.That(unsafePosition.HasValue, Is.True);
            director.Player.transform.position = unsafePosition.Value + Vector3.up * 0.2f;
            var before = director.Player.Health.Current;
            yield return new WaitForSeconds(0.45f);
            Assert.That(director.Player.Health.Current, Is.LessThan(before));
        }

        private static Vector3? FindUnsafeCell(GameDirector director)
        {
            for (var y = 0; y < director.World.Layout.Height; y++)
            {
                for (var x = 0; x < director.World.Layout.Width; x++)
                {
                    var position = director.World.CellToWorld(new Vector2Int(x, y));
                    if (!director.World.IsWithinSafeLight(position, director.CurrentLevel.safeLightRadius))
                        return position;
                }
            }
            return null;
        }

        [UnityTest]
        public IEnumerator StartChooseEnterAndWin_CompletesCoreLoop()
        {
            var director = GameDirector.Instance;
            Assert.That(director, Is.Not.Null);
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Staging));
            Assert.That(director.World.Layout.Width, Is.EqualTo(7));
            Assert.That(director.Player.Weapon.Type, Is.EqualTo(WeaponType.None));

            director.SelectWeapon(WeaponType.Pistol);
            yield return null;
            Assert.That(director.Player.Weapon.Type, Is.EqualTo(WeaponType.Pistol));
            Assert.That(director.World.StartGate.activeSelf, Is.False);

            director.BeginRun();
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Playing));
            director.CompleteLevel();
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Won));
        }

        [UnityTest]
        public IEnumerator CampaignCanAdvanceToLevelTen_WithDifferentMazeSize()
        {
            var director = GameDirector.Instance;
            for (var level = 2; level <= 10; level++)
            {
                director.NextLevel();
                yield return null;
            }

            Assert.That(director.CurrentLevel.levelNumber, Is.EqualTo(10));
            Assert.That(director.World.Layout.Width, Is.EqualTo(15));
            Assert.That(director.World.Layout.Height, Is.EqualTo(14));
            Assert.That(director.CurrentLevel.miasma, Is.True);
            Assert.That(director.World.SafeLights, Is.Not.Empty);
            Assert.That(director.EnemyPoolCapacity, Is.GreaterThanOrEqualTo(14));
        }
    }
}
