using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VanillaUpgrades.Classes
{
    internal class VanillaUpgradesManager : MonoBehaviour
    {
        internal static VanillaUpgradesManager instance;
        internal PhotonView photonView;
        private bool checkPlayerUpgrades;

        private void Awake()
        {
            instance = this;
            photonView = GetComponent<PhotonView>();
            if (SemiFunc.IsMasterClientOrSingleplayer())
                StartCoroutine("WaitUntilLevel");
        }

        private void FixedUpdate()
        {
            if (!checkPlayerUpgrades)
                return;
            foreach (UpgradeItem upgradeItem in Plugin.instance.upgradeItems)
            {
                int startingAmount = upgradeItem.GetConfig<int>("Starting Amount");
                if (startingAmount >= 0)
                {
                    foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll())
                    {
                        string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                        if (!upgradeItem.appliedPlayerUpgrades.ContainsKey(steamId))
                            upgradeItem.appliedPlayerUpgrades[steamId] = 0;
                        if (upgradeItem.appliedPlayerUpgrades[steamId] == startingAmount)
                            continue;
                        Upgrade(upgradeItem.name, steamId, startingAmount - upgradeItem.appliedPlayerUpgrades[steamId]);
                        upgradeItem.appliedPlayerUpgrades[steamId] = startingAmount;
                    }
                }
                if (upgradeItem.GetConfig<bool>("Sync Host Upgrades"))
                {
                    int hostAmount = upgradeItem.GetAmount();
                    foreach (PlayerAvatar playerAvatar in SemiFunc.PlayerGetAll().Where(x => x != SemiFunc.PlayerAvatarLocal()))
                    {
                        string steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                        int amount = upgradeItem.GetAmount(steamId);
                        Upgrade(upgradeItem.name, steamId, hostAmount - amount);
                    }
                }
            }
        }

        private IEnumerator WaitUntilLevel()
        {
            yield return new WaitUntil(() => SemiFunc.LevelGenDone());
            checkPlayerUpgrades = true;
        }

        private void UpdateRightAway(UpgradeItem upgradeItem, string steamId, int amount)
        {
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamId);
            if (playerAvatar)
            {
                if (upgradeItem.name == "Health")
                {
                    PlayerHealth playerHealth = playerAvatar.playerHealth;
                    FieldInfo fieldInfo = AccessTools.Field(typeof(PlayerHealth), "maxHealth");
                    int maxHealth = (int)fieldInfo.GetValue(playerHealth);
                    int finalMaxHealth = maxHealth + (20 * amount);
                    fieldInfo.SetValue(playerHealth, finalMaxHealth);
                    int difference = finalMaxHealth - maxHealth;
                    if (difference > 0)
                        playerHealth.Heal(difference, false);
                    else
                        playerHealth.Hurt(difference, false);
                }
                else if (upgradeItem.name == "Energy")
                {
                    //use transpiler to get the config of it too at PlayerController LateStart
                    //check for the other things too
                    PlayerController.instance.EnergyStart += amount * 10f;
                    PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
                }
                else if (upgradeItem.name == "Extra Jump")
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(PlayerController), "JumpExtra");
                    fieldInfo.SetValue(PlayerController.instance,
                        (int)fieldInfo.GetValue(PlayerController.instance) + amount);
                }
                else if (upgradeItem.name == "Tumble Launch")
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(PlayerTumble), "tumbleLaunch");
                    fieldInfo.SetValue(playerAvatar.tumble,
                        (int)fieldInfo.GetValue(playerAvatar.tumble) + amount);
                }
                else if (upgradeItem.name == "Map Player Count")
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(PlayerController), "upgradeMapPlayerCount");
                    fieldInfo.SetValue(playerAvatar,
                        (int)fieldInfo.GetValue(playerAvatar) + amount);
                }
                else if (upgradeItem.name == "Sprint Speed")
                {
                    PlayerController.instance.SprintSpeed += amount * 1f;
                    PlayerController.instance.SprintSpeedUpgrades += amount * 1f;
                }
                else if (upgradeItem.name == "Grab Strength")
                    playerAvatar.physGrabber.grabStrength += amount * 0.2f;
                else if (upgradeItem.name == "Grab Range")
                    playerAvatar.physGrabber.grabRange += amount * 1f;
            }
        }

        internal void Upgrade(string upgradeItemName, string steamId, int amount = 1)
        {
            if (amount == 0)
                return;
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == upgradeItemName);
            if (upgradeItem == null)
                return;
            upgradeItem.playerUpgrades[steamId] += amount;
            if (SemiFunc.IsMasterClientOrSingleplayer())
                UpdateRightAway(upgradeItem, steamId, amount);
            if (SemiFunc.IsMasterClient())
                photonView.RPC("UpgradeRPC", RpcTarget.Others, upgradeItemName, steamId, upgradeItem.playerUpgrades[steamId]);
        }

        [PunRPC]
        internal void UpgradeRPC(string upgradeItemName, string steamId, int amount)
        {
            UpgradeItem upgradeItem = Plugin.instance.upgradeItems.FirstOrDefault(x => x.name == upgradeItemName);
            if (upgradeItem == null)
                return;
            int previousAmount = upgradeItem.playerUpgrades[steamId];
            Plugin.instance.logger.LogMessage(previousAmount + " - " + amount);
            upgradeItem.playerUpgrades[steamId] = amount;
            UpdateRightAway(upgradeItem, steamId, amount - previousAmount);
        }
    }
}
