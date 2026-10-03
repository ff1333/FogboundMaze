using UnityEngine;
using UnityEngine.UI;

namespace FogboundMaze
{
    public sealed partial class GameHud
    {
        private GameObject guidePanel;
        private Text guideTitle;
        private Text guideBody;
        private readonly Button[] guideTabs = new Button[10];

        public static string LevelBrief(int number) => number switch
        {
            1 => "THE FIRST EXIT\nLearn the corridors. Ordinary zombies follow the maze paths. Reach the exit alive.",
            2 => "MORE CROSSROADS\nA larger maze and more dead ends. The map remembers where you have walked.",
            3 => "GROWING PURSUIT\nMore zombies and faster reinforcements. Keep moving and check your ammunition.",
            4 => "DENSE FOG\nFog now limits distant visibility. Use nearby landmarks and your explored map.",
            5 => "HEAVY ELITES\nLarge purple zombies join the hunt. They have more health, move faster and hit harder.",
            6 => "DAY AND NIGHT\nLight changes during the run. Your flashlight remains available through the night.",
            7 => "MIASMA AND SAFE LIGHTS\nOutside a lamp's safe radius, miasma drains health. Travel from light to light.",
            8 => "DEEP ZONE\nFog, elites, night and miasma combine. Longer routes and tighter safe zones raise the pressure.",
            9 => "LOCKDOWN\nMore frequent enemies and more elites. Retrace explored paths when a route becomes unsafe.",
            _ => "THE LAST EXIT\nThe largest maze and strongest combined pressure. Clear this exit to finish the campaign."
        };

        private void BuildGuide()
        {
            guidePanel = PanelObject("Field Guide", transform, new Color(0.035f, 0.045f, 0.05f, 0.98f));
            SetRect(guidePanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            var heading = Label("Heading", guidePanel.transform, "FIELD GUIDE", 52, TextAnchor.MiddleLeft);
            SetRect(heading.rectTransform, new Vector2(.07f,.84f), new Vector2(.9f,.95f));
            for (var i = 0; i < 10; i++)
            {
                var number = i + 1;
                var tab = Button("Chapter " + number, guidePanel.transform, "LEVEL " + number.ToString("00"), Panel);
                var row = i % 5;
                var col = i / 5;
                SetRect(tab.GetComponent<RectTransform>(), new Vector2(.07f + col * .13f,.68f - row * .112f),
                    new Vector2(.19f + col * .13f,.775f - row * .112f));
                tab.onClick.AddListener(() => ShowGuideChapter(number));
                guideTabs[i] = tab;
            }
            guideTitle = Label("Chapter Title", guidePanel.transform, "", 36, TextAnchor.UpperLeft, Accent);
            SetRect(guideTitle.rectTransform, new Vector2(.38f,.62f), new Vector2(.93f,.79f));
            guideBody = Label("Chapter Details", guidePanel.transform, "", 27, TextAnchor.UpperLeft);
            SetRect(guideBody.rectTransform, new Vector2(.38f,.22f), new Vector2(.92f,.62f));
            var back = Button("Back", guidePanel.transform, "BACK", new Color(.18f,.24f,.26f));
            SetRect(back.GetComponent<RectTransform>(), new Vector2(.07f,.07f), new Vector2(.25f,.15f));
            back.onClick.AddListener(() => GameDirector.Instance.ShowTitle());
            ShowGuideChapter(1);
            guidePanel.SetActive(false);
        }

        private void ShowGuideChapter(int number)
        {
            var level = LevelCatalog.CreateDefault()[number - 1];
            var brief = LevelBrief(number).Split('\n');
            guideTitle.text = $"{number:00}  /  {brief[0]}";
            guideBody.text = brief[1] + $"\n\nMAZE  {level.width} x {level.height}     ENEMIES  {level.maxEnemies} MAX"
                + $"\nSPAWN INTERVAL  {level.spawnInterval:0.0}s     ELITE CHANCE  {level.eliteChance:P0}"
                + $"\n\nFOG  {(level.fogDensity > .01f ? "ACTIVE" : "CLEAR")}     DAY / NIGHT  {(level.dayNightCycle ? "ACTIVE" : "OFF")}"
                + (level.miasma ? $"\nMIASMA  {level.miasmaDamagePerSecond:0.0} HP/s     SAFE RADIUS  {level.safeLightRadius:0.0}m" : "\nMIASMA  OFF")
                + "\n\nClear the previous level to unlock the next. Replaying a cleared level keeps your progress.";
            for (var i = 0; i < guideTabs.Length; i++)
                guideTabs[i].GetComponent<Image>().color = i + 1 == number ? new Color(.15f,.40f,.34f) : Panel;
        }
    }
}
