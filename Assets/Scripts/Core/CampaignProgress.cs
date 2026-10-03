using UnityEngine;

namespace FogboundMaze
{
    public sealed class CampaignProgress
    {
        public const string CompletionKey = "Fogbound.CompletedLevels";
        private readonly string key;
        private readonly bool useLegacy;

        public CampaignProgress(bool isolated = false)
        {
            key = isolated ? "Fogbound.Smoke.CompletedLevels" : CompletionKey;
            useLegacy = !isolated;
        }

        public int CompletedCount
        {
            get
            {
                // Only a continuous prefix counts; a stray selected-level value never unlocks a stage.
                var mask = PlayerPrefs.GetInt(key, 0);
                if (!PlayerPrefs.HasKey(key) && useLegacy)
                    return Mathf.Clamp(PlayerPrefs.GetInt("Fogbound.UnlockedLevel", 1) - 1, 0, 9);
                var count = 0;
                while (count < 10 && (mask & (1 << count)) != 0) count++;
                return count;
            }
        }

        public bool IsUnlocked(int level) => level >= 1 && level <= Mathf.Min(10, CompletedCount + 1);
        public bool IsCompleted(int level) => level >= 1 && level <= CompletedCount;

        public void Complete(int level)
        {
            if (!IsUnlocked(level)) return;
            var count = Mathf.Max(CompletedCount, level);
            PlayerPrefs.SetInt(key, (1 << count) - 1);
            PlayerPrefs.Save();
        }
    }
}
