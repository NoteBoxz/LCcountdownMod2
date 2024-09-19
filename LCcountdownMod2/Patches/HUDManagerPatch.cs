using HarmonyLib;
using LCcountdownMod2;
using UnityEngine;

namespace LCcountdownMod2
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void MakeCountdown(HUDManager __instance)
        {
            LCcountdownMod2.CountDownInstace = GameObject.Instantiate(LCcountdownMod2.countdownPrefab, __instance.HUDContainer.transform.parent).GetComponent<Countdowner>();

            // Find the "Panel" GameObject
            Transform panelTransform = __instance.HUDContainer.transform.parent.Find("Panel");

            if (panelTransform != null && !LCcountdownMod2.SpawnCountdownInfrountOfUI)
            {
                // Set the countdown prefab to be below the "Panel" GameObject
                LCcountdownMod2.CountDownInstace.transform.SetSiblingIndex(panelTransform.GetSiblingIndex() + 1);
            }
            else if(!LCcountdownMod2.SpawnCountdownInfrountOfUI)
            {
                LCcountdownMod2.Logger.LogWarning("No 'pannle' found");
            }
        }
    }
}
