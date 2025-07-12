using HarmonyLib;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgradeMapPlayerCount))]
    internal class ItemUpgradeMapPlayerCountPatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyPrefix]
        static bool Upgrade(ItemToggle ___itemToggle) => Plugin.instance.Upgrade(___itemToggle);
    }
}
