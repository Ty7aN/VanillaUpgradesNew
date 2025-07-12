using HarmonyLib;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgradePlayerSprintSpeed))]
    internal class ItemUpgradePlayerSprintSpeedPatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyPrefix]
        static bool Upgrade(ItemToggle ___itemToggle) => Plugin.instance.Upgrade(___itemToggle);
    }
}
