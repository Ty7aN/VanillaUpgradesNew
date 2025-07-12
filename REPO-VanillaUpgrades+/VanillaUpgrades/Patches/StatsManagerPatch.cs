using HarmonyLib;
using System.Collections.Generic;
using VanillaUpgrades.Classes;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    internal class StatsManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPrefix(StatsManager __instance)
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
                __instance.dictionaryOfDictionaries.Add($"appliedPlayerUpgrade{upgradeItem.saveName}",
                    upgradeItem.appliedPlayerUpgrades);
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPostfix(StatsManager __instance)
        {
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                if (!__instance.dictionaryOfDictionaries.TryGetValue($"playerUpgrade{upgradeItem.saveName}",
                    out Dictionary<string, int> playerUpgrades))
                    continue;
                upgradeItem.playerUpgrades = playerUpgrades;
            }
        }
    }
}
