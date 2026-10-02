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
