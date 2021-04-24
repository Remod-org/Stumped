﻿using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Stumped", "RFC1920", "1.0.1")]
    [Description("Trees leave stumps!")]
    class Stumped : RustPlugin
    {
        private ConfigData configData;
        public Dictionary<ulong, HashSet<Stumps>> playerGathered = new Dictionary<ulong, HashSet<Stumps>>();

        public class Stumps
        {
            public ulong netid;
            public DateTime chopped;
        }

       #region Message
        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
        private void Message(IPlayer player, string key, params object[] args) => player.Message(Lang(key, player.Id, args));
        #endregion

        #region hooks
        void Loaded() => LoadConfigVariables();

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["nogather"] = "You cannot gather this stump yet. {0} more minutes..."
            }, this);
        }

        object OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            var tree = dispenser.GetComponentInParent<TreeEntity>();
            if (tree != null)
            {
                if (!playerGathered.ContainsKey(player.userID))
                {
                    playerGathered.Add(player.userID, new HashSet<Stumps>());
                }

                var stump = GameManager.server.CreateEntity("assets/bundled/prefabs/autospawn/collectable/wood/wood-collectable.prefab", tree.transform.position, tree.transform.rotation);
                NextTick(() =>
                {
                    stump.Spawn();
                    playerGathered[player.userID].Add(new Stumps() { netid = stump.net.ID, chopped = DateTime.Now });
                });
            }
            return null;
        }

        object OnCollectiblePickup(Item item, BasePlayer player, CollectibleEntity entity)
        {
            // Block for player who chopped down the tree...
            if (playerGathered.ContainsKey(player.userID))
            {
                foreach (Stumps stump in playerGathered[player.userID])
                {
                    if (stump.netid == entity.net.ID)
                    {
                        TimeSpan min = TimeSpan.FromMinutes(configData.Options.protectedMinutes);
                        if (DateTime.Now - stump.chopped < min)
                        {
                            var endtime = stump.chopped + new TimeSpan(0, configData.Options.protectedMinutes, 0);
                            var towait = endtime - DateTime.Now;

                            Message(player.IPlayer, "nogather", towait.Minutes.ToString());
                            return true;
                        }
                        else
                        {
                            playerGathered[player.userID].RemoveWhere((x) => x.netid == entity.net.ID);
                            return null;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region config
        private void LoadConfigVariables()
        {
            configData = Config.ReadObject<ConfigData>();

            configData.Version = Version;
            SaveConfig(configData);
        }
        protected override void LoadDefaultConfig()
        {
            Puts("Creating new config file.");
            var config = new ConfigData
            {
                Version = Version,
            };
            config.Options.protectedMinutes = 10;

            SaveConfig(config);
        }
        private void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }

        private class ConfigData
        {
            public Options Options = new Options();
            public VersionNumber Version;
        }
        private class Options
        {
            public int protectedMinutes;
        }
        #endregion
    }
}