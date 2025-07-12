﻿using HarmonyLib;

namespace VanillaUpgrades.Patches
{
    [HarmonyPatch(typeof(ItemUpgradePlayerTumbleWings))]
    internal class ItemUpgradePlayerTumbleWingsPatch
    {
        [HarmonyPatch("Upgrade")]
        [HarmonyPrefix]
        static bool Upgrade(ItemToggle ___itemToggle) => Plugin.instance.Upgrade(___itemToggle);
    }
}