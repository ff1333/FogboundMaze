using UnityEngine;

namespace FogboundMaze
{
    public static class ProceduralAudio
    {
        private static AudioClip pistol;
        private static AudioClip machete;
        private static AudioClip impact;
        private static AudioClip reload;
        public static AudioClip Impact => impact ??= Create("Hit Confirm", .075f, 240f, 900f, .65f, 103);
        public static AudioClip Reload => reload ??= Create("Magazine", .13f, 110f, 1800f, .8f, 221);

        public static AudioClip SubmachineGun => pistol ??= Create("SMG", 0.085f, 95f, 950f, 0.55f, 37);
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
