using HarmonyLib;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgradePlayerGrabStrength))]
    internal class ItemUpgradePlayerGrabStrengthPatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyPrefix]
        static bool Upgrade(ItemToggle ___itemToggle) => Plugin.instance.Upgrade(___itemToggle);
    }
}
