using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LCcountdownMod2
{
    public class Countdowner : MonoBehaviour
    {
        public AudioSource SFX;
        public TMP_Text TXT;
        public Image Sprite;
        public Animator anim;
        public void Start()
        {
            SetColors();
        }
        public void Count(string TXT5REAL, int TextSize)
        {
            anim.Play("CountDown");
            SFX.Play();
            TXT.text = TXT5REAL;
            TXT.fontSize = TextSize;
        }
        public void SetColors()
        {
            TXT.color = ParseColor(LCcountdownMod2.TextColor);
            Sprite.color = ParseColor(LCcountdownMod2.CircleColor);
        }
        private static Color ParseColor(string colorString)
        {
            // Remove parentheses and split the string
            var values = colorString.Trim('(', ')').Split(',');
            if (values.Length == 4 &&
                int.TryParse(values[0], out int r) &&
                int.TryParse(values[1], out int g) &&
                int.TryParse(values[2], out int b) &&
                int.TryParse(values[3], out int a))
            {
                // Convert to Color with normalized values
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
            LCcountdownMod2.Logger.LogWarning($"Unable to parse color config!!!: Input {colorString} The input should be somthing like (000,000,000,000) for (R,G,B,A)");
            return Color.white; // Default color if parsing fails
        }
    }
}
