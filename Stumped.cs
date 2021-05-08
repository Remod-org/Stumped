#region License (GPL v3)
/*
    DESCRIPTION
    Copyright (c) 2021 RFC1920 <desolationoutpostpve@gmail.com>

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/
#endregion License Information (GPL v3)
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Stumped", "RFC1920", "1.0.3")]
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
                ["nogather"] = "You cannot gather this stump yet. {0} more minutes...",
                ["nogather1"] = "You cannot gather this stump yet. {0} more second(s)...",
                ["nogather2"] = "You cannot gather this stump yet. {0} more minute(s)..."
            }, this);
        }

        object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            BasePlayer player = entity?.ToPlayer();
            if (player == null) return null;

            var tree = dispenser.GetComponentInParent<TreeEntity>();
            if (tree != null)
            {
                if (playerGathered.ContainsKey(player.userID))
                {
                    foreach (Stumps stump in playerGathered[player.userID])
                    {
                        if (stump.netid == entity.net.ID)
                        {
                            //Puts("Blocking gather for this player.");
                            return true;
                        }
                    }
                }
            }
            return null;
        }

        object OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            // Possibly ineffective in avoiding users spamming E to get points in ZLevels, etc.
            bool genstump = true;
            var tree = dispenser.GetComponentInParent<TreeEntity>();
            if (tree != null)
            {
                if (!playerGathered.ContainsKey(player.userID))
                {
                    playerGathered.Add(player.userID, new HashSet<Stumps>());
                }

                if ((configData.Options.stumpPercentChance >= 0.5f) && (configData.Options.stumpPercentChance < 100))
                {
                    genstump = false;
                    System.Random random = new System.Random();
                    if (random.Next(100) < Math.Abs(configData.Options.stumpPercentChance))
                    {
                        genstump = true;
                    }
                }
                if (genstump)
                {
                    var stump = GameManager.server.CreateEntity("assets/bundled/prefabs/autospawn/collectable/wood/wood-collectable.prefab", tree.transform.position, tree.transform.rotation);
                    NextTick(() =>
                    {
                        stump.Spawn();
                        playerGathered[player.userID].Add(new Stumps() { netid = stump.net.ID, chopped = DateTime.Now });
                    });
                }
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
                        TimeSpan sec = TimeSpan.FromSeconds(configData.Options.protectedMinutes * 60);
                        if (DateTime.Now - stump.chopped < sec)
                        {
                            var endtime = stump.chopped + new TimeSpan(0, configData.Options.protectedMinutes, 0);
                            var towait = endtime - DateTime.Now;
                            var seconds = Math.Floor(towait.TotalSeconds);

                            if (seconds < 60)
                            {
                                Message(player.IPlayer, "nogather1", seconds.ToString());
                            }
                            else
                            {
                                Message(player.IPlayer, "nogather2", towait.Minutes.ToString());
                            }
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

            if (configData.Version < new VersionNumber(1, 0, 2))
            {
                configData.Options.stumpPercentChance = 100;
            }

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
            config.Options.stumpPercentChance = 100;

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
            public float stumpPercentChance;
        }
        #endregion
    }
}
