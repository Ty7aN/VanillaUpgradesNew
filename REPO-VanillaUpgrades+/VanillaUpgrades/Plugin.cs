using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VanillaUpgrades.Classes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;

namespace VanillaUpgrades
{
    [BepInPlugin(modGUID, modName, modVer)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "bulletbot.vanillaupgrades";
        private const string modName = "VanillaUpgrades";
        private const string modVer = "1.0.4";

        internal static Plugin instance;
        internal ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(modGUID);

        internal List<UpgradeItem> upgradeItems;

        internal static float UpgradeValueIncrease(float upgradeValueIncrease, string itemAssetName)
        {
            if (VanillaUpgradesManager.instance == null)
                return upgradeValueIncrease;
            UpgradeItem upgradeItem = instance.upgradeItems.FirstOrDefault(x => x.fullName == itemAssetName);
            if (upgradeItem == null)
                return upgradeValueIncrease;
            float value = upgradeItem.GetConfig<float>("Price Increase Scaling");
            if (value < 0)
                value = upgradeValueIncrease;
            return value;
        }

        internal bool Upgrade(ItemToggle itemToggle)
        {
            if (VanillaUpgradesManager.instance == null)
                return true;
            UpgradeItem upgradeItem = upgradeItems.FirstOrDefault(x =>
                x.fullName == itemToggle.gameObject.GetComponent<ItemAttributes>().item.itemAssetName);
            if (upgradeItem == null)
                return true;
            if (upgradeItem.GetConfig<bool>("Allow Team Upgrades"))
            {
                foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll())
                    VanillaUpgradesManager.instance.Upgrade(upgradeItem.name, SemiFunc.PlayerGetSteamID(playerAvatar));
            }
            else
            {
                VanillaUpgradesManager.instance.Upgrade(upgradeItem.name,
                    SemiFunc.PlayerGetSteamID(
                        SemiFunc.PlayerAvatarGetFromPhotonID(
                            (int)AccessTools.Field(typeof(ItemToggle), "playerTogglePhotonID").GetValue(itemToggle))));
            }
            return false;
        }

        void Awake()
        {
            instance = this;
            logger = BepInEx.Logging.Logger.CreateLogSource(modName);
            upgradeItems = new List<UpgradeItem>();
            foreach (Item item in Resources.LoadAll<Item>("items/"))
            {
                UpgradeItem upgradeItem = new UpgradeItem();
                if (upgradeItem.Init(item))
                    upgradeItems.Add(upgradeItem);
            }
            SceneManager.activeSceneChanged += delegate
            {
                if (RunManager.instance == null || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu
                    || RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
                    return;
                GameObject manager = new GameObject("Vanilla Upgrades Manager");
                PhotonView photonView = manager.AddComponent<PhotonView>();
                photonView.ViewID = 1864;
                manager.AddComponent<VanillaUpgradesManager>();
            };
            logger.LogMessage($"{modName} has started.");
            harmony.PatchAll();
        }
    }
}