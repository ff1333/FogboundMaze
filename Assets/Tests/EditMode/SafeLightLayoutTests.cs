using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace FogboundMaze.Tests
{
    public sealed class SafeLightLayoutTests
    {
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void LateCampaignLights_CoverBranchesWithoutRemovingMiasma(int number)
        {
            var level = LevelCatalog.CreateDefault()[number - 1];
            var maze = MazeGenerator.GenerateForLevel(level);
            var lamps = SafeLightLayout.Generate(maze, level.seed);
            CollectionAssert.AreEqual(lamps, SafeLightLayout.Generate(maze, level.seed));
            Assert.That(new HashSet<Vector2Int>(lamps).Count, Is.EqualTo(lamps.Count));
            Assert.That(lamps.All(maze.Contains), Is.True);
            Assert.That(lamps, Does.Contain(maze.Start));

            // The solution is used only to audit the generated distribution and exposure.
            var path = MazePathfinder.FindPath(maze, maze.Start, maze.Goal);
            var onRoute = new HashSet<Vector2Int>(path);
            var offRoute = lamps.Count(cell => !onRoute.Contains(cell));
            var deadEnds = lamps.Count(cell => !onRoute.Contains(cell)
                && maze.GetOpenNeighbors(cell).Count() == 1);
            Assert.That(offRoute, Is.GreaterThan(lamps.Count / 3));
            Assert.That(deadEnds, Is.GreaterThan(0));
            var unsafeCells = 0;
            for (var y = 0; y < maze.Height; y++)
                for (var x = 0; x < maze.Width; x++)
                    if (!Safe(new Vector2(x, y), lamps, level.safeLightRadius)) unsafeCells++;

            var exposedMeters = 0f;
            path.Add(maze.Goal + Vector2Int.right);
            for (var i = 1; i < path.Count; i++)
                for (var sample = 0; sample < 50; sample++)
                {
                    var point = Vector2.Lerp(path[i - 1], path[i], (sample + .5f) / 50f);
                    if (!Safe(point, lamps, level.safeLightRadius)) exposedMeters += MazeWorld.CellSize / 50f;
                }
            var walkDamage = exposedMeters / 4.8f * level.miasmaDamagePerSecond;
            var sprintDamage = exposedMeters / 7.2f * level.miasmaDamagePerSecond;
            TestContext.WriteLine($"Level {number}: lamps={lamps.Count}, offRoute={offRoute}, deadEnds={deadEnds}, "
                + $"unsafeCells={unsafeCells}, exposedMeters={exposedMeters:F1}, walkDamage={walkDamage:F1}, sprintDamage={sprintDamage:F1}");
            Assert.That(unsafeCells, Is.GreaterThan(0), "Miasma must retain unsafe space.");
            Assert.That(sprintDamage, Is.LessThan(70f), "Direct traversal needs health left for combat and mistakes.");
            Assert.That(walkDamage, Is.LessThan(80f), "Walking the direct route must not require sprinting to survive miasma alone.");
        }

        private static bool Safe(Vector2 cell, List<Vector2Int> lamps, float radius)
        {
            return lamps.Any(lamp => ((Vector2)lamp - cell).sqrMagnitude * MazeWorld.CellSize * MazeWorld.CellSize <= radius * radius);
        }

        [Test]
        public void ReloadCompletionClip_IsDistinctAndContainsTwoAudiblePulses()
        {
            var clip = ProceduralAudio.ReloadComplete;
            Assert.That(clip, Is.Not.SameAs(ProceduralAudio.Reload));
            var samples = new float[clip.samples];
            Assert.That(clip.GetData(samples, 0), Is.True);
            Assert.That(samples.All(value => !float.IsNaN(value) && Mathf.Abs(value) <= 1f), Is.True);
            Assert.That(samples.Take((int)(clip.frequency * .06f)).Max(value => Mathf.Abs(value)), Is.GreaterThan(.1f));
            Assert.That(samples.Skip((int)(clip.frequency * .1f)).Max(value => Mathf.Abs(value)), Is.GreaterThan(.1f));
        }
    }
}
