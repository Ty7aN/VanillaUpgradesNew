using HarmonyLib;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgradePlayerTumbleLaunch))]
    internal class ItemUpgradePlayerTumbleLaunchPatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyPrefix]
        static bool Upgrade(ItemToggle ___itemToggle) => Plugin.instance.Upgrade(___itemToggle);
    }
}
