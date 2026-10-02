using UnityEngine;

namespace FogboundMaze
{
    public static class ProceduralAudio
    {
        private static AudioClip pistol;
        private static AudioClip machete;

        public static AudioClip Pistol => pistol ??= Create("Pistol", 0.11f, 125f, 730f, 0.36f, 37);
        public static AudioClip Machete => machete ??= Create("Machete", 0.18f, 90f, 230f, 0.24f, 91);

        private static AudioClip Create(string name, float duration, float low, float high, float noiseMix, int seed)
        {
            const int sampleRate = 22050;
            var count = Mathf.CeilToInt(duration * sampleRate);
            var samples = new float[count];
            var random = new System.Random(seed);
            for (var i = 0; i < count; i++)
            {
                var time = i / (float)sampleRate;
                var progress = i / (float)count;
                var frequency = Mathf.Lerp(high, low, progress);
                var tone = Mathf.Sin(time * frequency * Mathf.PI * 2f);
                var noise = (float)random.NextDouble() * 2f - 1f;
                samples[i] = (tone * (1f - noiseMix) + noise * noiseMix) * (1f - progress) * 0.55f;
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
