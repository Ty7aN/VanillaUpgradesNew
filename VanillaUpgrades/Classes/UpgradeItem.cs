using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VanillaUpgrades.Classes
{
    internal class UpgradeItem
    {
        internal string name;
        internal string fullName;
        internal Dictionary<string, int> playerUpgrades;
        internal Dictionary<string, int> appliedPlayerUpgrades;
        internal string saveName;
        private string sectionName;
        private Dictionary<string, ConfigEntryBase> configEntries;

        public bool AddConfig<T>(string key, T defaultValue, string description = "")
        {
            if (configEntries.ContainsKey(key))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' already exists. Duplicates are not allowed.");
                return false;
            }
            ConfigEntryBase configEntryBase = null;
            if (defaultValue is int)
            {
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue, 
                    new ConfigDescription(description, new AcceptableValueRange<int>(0, 1000)));
            }
            else if (defaultValue is float)
            {
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue,
                    new ConfigDescription(description, new AcceptableValueRange<float>(-1, 100000)));
            }
            else
                configEntryBase = Plugin.instance.Config.Bind(sectionName, key, defaultValue, description);
            configEntries.Add(key, configEntryBase);
            return true;
        }

        public T GetConfig<T>(string key)
        {
            if (!configEntries.TryGetValue(key, out ConfigEntryBase value))
            {
                Plugin.instance.logger.LogWarning($"A config entry with the key '{key}' does not exist. Returning default value.");
                return default;
            }
            if (value is ConfigEntry<T> convertedValue)
                return convertedValue.Value;
            Plugin.instance.logger.LogWarning($"Type mismatch for config entry '{key}'." +
                $" Expected: {value.SettingType.FullName}, but got: {typeof(T).FullName}. Returning default value.");
            return default;
        }

        public int GetAmount(string steamId = null)
        {
            if (steamId != null)
                return playerUpgrades.ContainsKey(steamId) ? playerUpgrades[steamId] : 0;
            PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarLocal();
            if (playerAvatar != null)
                steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
            return steamId != null && playerUpgrades.ContainsKey(steamId) ? playerUpgrades[steamId] : 0;
        }

        internal bool Init(Item item)
        {
            fullName = item.itemAssetName;
            string start = "Item Upgrade ";
            if (!fullName.StartsWith(start))
                return false;
            string strippedName = fullName.Replace(start, "");
            bool hasPlayerText = true;
            if (strippedName == "Player Health")
                saveName = strippedName.Replace("Player ", "");
            else if (strippedName == "Player Energy")
                saveName = "Stamina";
            else if (strippedName == "Player Extra Jump")
                saveName = new string(strippedName.Replace("Player ", "").Where(x => !char.IsWhiteSpace(x)).ToArray());
            else if (strippedName == "Player Tumble Launch")
                saveName = "Launch";
            else if (strippedName == "Map Player Count")
            {
                saveName = new string(strippedName.Where(x => !char.IsWhiteSpace(x)).ToArray());
                hasPlayerText = false;
            }
            else if (strippedName == "Player Sprint Speed")
                saveName = "Speed";
            else if (strippedName == "Player Grab Strength")
                saveName = "Strength";
            else if (strippedName == "Player Grab Range")
                saveName = "Range";
            else if (strippedName == "Player Crouch Rest")
                saveName = "CrouchRest";
            else if (strippedName == "Player Tumble Wings")
                saveName = "TumbleWings";
            if (saveName == null)
                return false;
            if (hasPlayerText)
                strippedName = strippedName.Replace("Player ", "");
            name = strippedName;
            sectionName = strippedName;
            AddConfig("Enabled", true, "Whether the upgrade item can be spawned to the shop.");
            AddConfig("Max Amount", item.maxAmount, "The maximum number of times the upgrade item can appear in the truck.");
            AddConfig("Max Amount In Shop", item.maxAmountInShop, 
                "The maximum number of times the upgrade item can appear in the shop.");
            AddConfig("Minimum Price", item.value.valueMin, "The minimum cost to purchase the upgrade item." +
                "\nThe default price multiplier is set to 4. " +
                "(Note: Other mods may modify this multiplier, affecting the upgrade item's price.)");
            AddConfig("Maximum Price", item.value.valueMax, "The maximum cost to purchase the upgrade item." +
                "\nThe default price multiplier is set to 4. " +
                "(Note: Other mods may modify this multiplier, affecting the upgrade item's price.)");
            AddConfig("Price Increase Scaling", -1f,
                "The scale of the price increase based on the total number of upgrade item purchased." +
                "\nDefault scaling is set to 0.5. " +
                "(Note: Other mods may modify this value, affecting the game's default scaling.)" +
                "\nSet to -1 to use the default scaling.");
            AddConfig("Max Purchase Amount", item.maxPurchase ? item.maxPurchaseAmount : 0,
                "The maximum number of times the upgrade item can be purchased before it is no longer available in the shop." +
                "\nSet to 0 to disable the limit.");
            AddConfig("Allow Team Upgrades", false, "Whether the upgrade item applies to the entire team instead of just one player.");
            AddConfig("Sync Host Upgrades", false, "Whether the host should sync the item upgrade for the entire team.");
            AddConfig("Starting Amount", 0, "The number of times the upgrade item is applied at the start of the game.");
            item.maxAmount = GetConfig<int>("Max Amount");
            item.maxAmountInShop = GetConfig<int>("Max Amount In Shop");
            item.maxPurchaseAmount = GetConfig<int>("Max Purchase Amount");
            item.maxPurchase = item.maxPurchaseAmount > 0;
            item.value = ScriptableObject.CreateInstance<Value>();
            item.value.valueMin = GetConfig<float>("Minimum Price");
            item.value.valueMax = GetConfig<float>("Maximum Price");
            return true;
        }

        internal UpgradeItem()
        {
            playerUpgrades = new Dictionary<string, int>();
            appliedPlayerUpgrades = new Dictionary<string, int>();
            configEntries = new Dictionary<string, ConfigEntryBase>();
        }
    }
}
