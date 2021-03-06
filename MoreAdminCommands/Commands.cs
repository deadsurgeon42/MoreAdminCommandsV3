﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace MoreAdminCommands
{
    public class Cmds
    {
        //Searching

        #region FindPermission
        public static void FindPerms(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                foreach (Command cmd in TShockAPI.Commands.ChatCommands)
                {
                    if (cmd.Names.Contains(args.Parameters[0]))
                    {
                        args.Player.SendInfoMessage(string.Format("Permission to use {0}: {1}",
                            cmd.Name, cmd.Permissions[0] != "" ? cmd.Permissions[0] : "Nothing"));
                        return;
                    }
                }
                args.Player.SendErrorMessage("Command not be found.");
            }
            else
            {
				args.Player.SendErrorMessage("Invalid syntax. Try {0}findperm [command]", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
            }
        }
        #endregion

        //Kills
        #region AutoKill
        public static void AutoKill(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var plyList = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);

                if (plyList.Count > 1)
                    TShock.Utils.SendMultipleMatchError(args.Player, plyList.Select(p => p.Name));
                else if (plyList.Count < 1)
                    args.Player.SendErrorMessage(plyList.Count.ToString() + " players matched.");

                else
                    if (!plyList[0].Group.HasPermission("mac.kill") || args.Player == plyList[0])
                    {
                        var player = Utils.GetPlayers(plyList[0].Index);

                        player.autoKill = !player.autoKill;

                        args.Player.SendSuccessMessage(string.Format("{0}abled auto-killed on {1}",
                            player.autoKill ? "En" : "Dis", player.name));
                        player.TSPlayer.SendInfoMessage(string.Format("You are {0} being auto-killed",
                            player.autoKill ? "now" : "no longer"));

                        if (player.autoKill)
                        {
                            if (!updateTimers.autoKillTimer.Enabled)
                                updateTimers.autoKillTimer.Enabled = true;
                        }
                    }
                    else
                        args.Player.SendErrorMessage("You cannot autokill this player!");
            }
            else
				args.Player.SendErrorMessage("Invalid syntax: {0}autokill <playerName>", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
        }
        #endregion

        #region KillAll
        public static void KillAll(CommandArgs args)
        {
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null)
                {
                    if (plr != args.Player)
                    {
                        plr.DamagePlayer(999999);
                    }
                }
            }
            TSPlayer.All.SendInfoMessage(args.Player.Name + " killed everyone!");
            args.Player.SendSuccessMessage("You killed everyone!");
        }
        #endregion

        #region TeamUnlock
        public static void TeamUnlock(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                string str = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

                var player = Utils.GetPlayers(args.Player.Index);

                switch (args.Parameters[0].ToLower())
                {
                    case "red":
                        if (str == MAC.config.redPass)
                        {
                            player.accessRed = true;
                            args.Player.SendErrorMessage("Red team unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "blue":
                        if (str == MAC.config.bluePass)
                        {
                            player.accessBlue = true;
                            args.Player.SendMessage("Blue team unlocked.", Color.LightBlue);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "green":
                        if (str == MAC.config.greenPass)
                        {
                            player.accessGreen = true;
                            args.Player.SendSuccessMessage("Green team unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    case "yellow":
                        if (str == MAC.config.yellowPass)
                        {
                            player.accessYellow = true;
                            args.Player.SendInfoMessage("Yellow unlocked.");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Incorrect password.");
                        }
                        break;

                    default:
                        args.Player.SendErrorMessage("Invalid team color.");
                        break;
                }
            }
            else
            {
				args.Player.SendErrorMessage("Improper Syntax. Proper Syntax: {0}teamunlock <teamcolor> <password>", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
            }
        }
        #endregion

        //Misc server/player related
        #region FreezeTime
        public static void FreezeTime(CommandArgs args)
        {
            MAC.timeFrozen = !MAC.timeFrozen;
            MAC.freezeDayTime = Main.dayTime;
            MAC.timeToFreezeAt = Main.time;

            if (MAC.timeFrozen)
            {
				if (!args.Silent)
					TSPlayer.All.SendInfoMessage(args.Player.Name.ToString() + " froze time.");
                if (!updateTimers.timeTimer.Enabled)
                    updateTimers.timeTimer.Enabled = true;
            }
            else
            {
				if (!args.Silent)
					TSPlayer.All.SendInfoMessage(args.Player.Name.ToString() + " unfroze time.");
                if (updateTimers.timeTimer.Enabled)
                    updateTimers.timeTimer.Enabled = false;
            }
        }
        #endregion

        #region ForceGive
        public static void ForceGive(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}forcegive <item type/id> <player> [item amount] [prefix id/name]", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
                return;
            }
            if (string.IsNullOrEmpty(args.Parameters[0]))
            {
                args.Player.SendErrorMessage("Missing item name/id.");
                return;
            }
            if (string.IsNullOrEmpty(args.Parameters[1]))
            {
                args.Player.SendErrorMessage("Missing player name.");
                return;
            }
            int itemAmount = 0;
            int prefix = 0;
            var items = TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
            args.Parameters.RemoveAt(0);
            string plStr = args.Parameters[0];
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count == 1)
                int.TryParse(args.Parameters[0], out itemAmount);
            else if (args.Parameters.Count == 2)
            {
                int.TryParse(args.Parameters[0], out itemAmount);
                var found = TShock.Utils.GetPrefixByIdOrName(args.Parameters[1]);
                if (found.Count == 1)
                    prefix = found[0];
            }


            if (items.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid item type!");
            }
            else if (items.Count > 1)
            {
				TShock.Utils.SendMultipleMatchError(args.Player, items.Select(p => p.Name));
            }
            else
            {
                var item = items[0];
                if (item.type >= 1 && item.type < Main.maxItemTypes)
                {
                    var players = TShockAPI.TShock.Utils.FindPlayer(plStr);
                    if (players.Count == 0)
                    {
                        args.Player.SendErrorMessage("Invalid player!");
                    }
                    else if (players.Count > 1)
                    {
                        TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
                    }
                    else
                    {
                        var plr = players[0];
                        if (itemAmount == 0 || itemAmount > item.maxStack)
                            itemAmount = item.maxStack;
                        if (plr.GiveItemCheck(item.type, item.Name, item.width, item.height, itemAmount, prefix))
                        {
                            args.Player.SendSuccessMessage("Gave {0} {1} {2}(s).", plr.Name, itemAmount, item.Name);
                            plr.SendSuccessMessage("{0} gave you {1} {2}(s).", args.Player.Name, itemAmount, item.Name);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("The item is banned and the config prevents spawning banned items.");
                        }
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("Invalid item type!");
                }
            }
        }
        #endregion

        #region AutoHeal
        public static void AutoHeal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                var player = Utils.GetPlayers(args.Player.Index);
                player.isHeal = !player.isHeal;

                args.Player.SendSuccessMessage("Autoheal is now " + (player.isHeal ? "on" : "off"));
            }
            else
            {
                string str = args.Parameters[0];

                var findPlayers = TShockAPI.TShock.Utils.FindPlayer(str);

                if (findPlayers.Count > 1)
                {
                    TShock.Utils.SendMultipleMatchError(args.Player, findPlayers.Select(p => p.Name));
                }
                else if (findPlayers.Count == 0)
                {
                    args.Player.SendErrorMessage("No players matched.");
                }
                else
                {
                    var player = Utils.GetPlayers(args.Parameters[0]);
                    TShockAPI.TSPlayer ply = findPlayers[0];

                    player.isHeal = !player.isHeal;

                    if (player.isHeal)
                    {
                        args.Player.SendInfoMessage("You have activated auto-heal for " + ply.Name + ".");
						if (!args.Silent)
							ply.SendInfoMessage(args.Player.Name + " has activated auto-heal on you");
                    }

                    else
                    {
                        args.Player.SendInfoMessage("You have deactivated auto-heal for " + ply.Name + ".");
						if (!args.Silent)
							ply.SendInfoMessage(args.Player.Name + " has deactivated auto-heal on you");
                    }
                }
            }
        }
        #endregion

        #region HealAll
        public static void HealAll(CommandArgs args)
        {
            int healCount = 0;
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.ConnectionAlive && player.Active)
                {
                    player.Heal();
                    healCount++;
                }
            }
            args.Player.SendSuccessMessage("Healed {0} player{1}", healCount, healCount == 0 || healCount > 1 ? "s" : "");
        }
        #endregion
		
        #region MoonPhase
        public static void MoonPhase(CommandArgs args)
        {
            int phase;
            bool result = Int32.TryParse(args.Parameters[0], out phase);
            if (result && phase > -1 && phase < 8 && args.Parameters.Count > 0)
            {
                string phaseName = "";
                Main.moonPhase = phase;

                #region PhaseName
                switch (phase)
                {
                    case 0:
                        phaseName = "full";
                        break;
                    case 1:
                        phaseName = "3/4";
                        break;
                    case 2:
                        phaseName = "1/2";
                        break;
                    case 3:
                        phaseName = "1/4";
                        break;
                    case 4:
                        phaseName = "new";
                        break;
                    case 5:
                        phaseName = "1/4";
                        break;
                    case 6:
                        phaseName = "1/2";
                        break;
                    case 7:
                        phaseName = "3/4";
                        break;
                }
                #endregion

				if (!args.Silent)
					TSPlayer.All.SendInfoMessage("Moon phase set to {0}.", phaseName);
            }
            else
				args.Player.SendErrorMessage("Invalid usage! Proper usage: {0}moon [0-7]", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
        }
        #endregion

        #region Reload
        public static void ReloadMore(CommandArgs args)
        {
            Utils.SetUpConfig();
            args.Player.SendInfoMessage("Reloaded MoreAdminCommands config file");
        }
        #endregion

        //Spawning
        #region SpawnGroup
        public static void SpawnGroup(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
				args.Player.SendErrorMessage("Invalid syntax: {0}spawngroup <npcGroupName>", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
            else
            {
                bool didspawn = false;
                string nGroup = string.Join(" ", args.Parameters[0]);
                didspawn = Utils.getSpawnGroup(nGroup, args.Player);
                if (didspawn)
                    args.Player.SendSuccessMessage("Mobs spawned successfully");
                else
                    args.Player.SendErrorMessage("Command failed! Try a different group name.");
            }
        }
        #endregion

        #region SpawnMobPlayer
        public static void SpawnMobPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 3)
            {
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}smp <mob name/id> <username> <amount>", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
            }
            else
            {
                if (TShock.Utils.FindPlayer(args.Parameters[1]).Count != 0)
                {
                    var player = TShock.Utils.FindPlayer(args.Parameters[1])[0];
                    
                    int mobID;

                    if (int.TryParse(args.Parameters[0], out mobID))
                    {
                        if (mobID <= 377 && mobID >= -65)
                        {
                            NPC npc = TShock.Utils.GetNPCById(mobID);

                            int amount;
                            if (int.TryParse(args.Parameters[2], out amount))
                            {
                                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, player.TileX, player.TileY, 50, 20);
                                TSPlayer.All.SendSuccessMessage("{0} was spawned {1} time{2} near {3}",
                                    npc.FullName, amount, amount > 1 || amount == 0 ? "s" : "", player.Name);
                            }
                            else
                            {
                                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, player.TileX, player.TileY, 50, 20);
                                TSPlayer.All.SendSuccessMessage("{0} was spawned 1 time near {3}",
                                     npc.FullName, player.Name);
                            }
                        }
                        else
                            args.Player.SendErrorMessage("Invalid NPC ID.");
                    }
                    else
                    {
                        if (TShock.Utils.GetNPCByName(args.Parameters[0]).Count != 0)
                        {
                            NPC npc = TShock.Utils.GetNPCByName(args.Parameters[0])[0];

                            int amount;
                            if (int.TryParse(args.Parameters[2], out amount))
                            {
                                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, player.TileX, player.TileY, 50, 20);
                                TSPlayer.All.SendSuccessMessage("{0} was spawned {1} time{2} near {3}",
                                    npc.FullName, amount, amount > 1 || amount == 0 ? "s" : "", player.Name);
                            }
                            else
                            {
                                TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, player.TileX, player.TileY, 50, 20);
                                TSPlayer.All.SendSuccessMessage("{0} was spawned 1 time near {1}",
                                     npc.FullName, player.Name);
                            }
                        }
                        else
                            args.Player.SendErrorMessage("Invalid NPC name.");
                    }
                }
                else
                    args.Player.SendErrorMessage("Player not found.");
            }
        }
        #endregion

        #region SpawnByMe
        public static void SpawnByMe(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
				args.Player.SendErrorMessage("Invalid syntax. Try {0}sbm <mob name/ID> [amount]", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
            else
            {
                int mobID;
                if (int.TryParse(args.Parameters[0], out mobID))
                {
                    if (TShock.Utils.GetNPCByIdOrName(mobID.ToString()).Count != 0)
                    {
                        NPC npc = TShock.Utils.GetNPCByIdOrName(mobID.ToString())[0];

                        int amount;
                        if (int.TryParse(args.Parameters[1], out amount))
                        {
                            TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX,
                                args.Player.TileY, 2, 5);
                            TSPlayer.All.SendSuccessMessage("Spawned {0} {1}{2}",
                                     amount, npc.FullName, amount > 1 || amount == 0 ? "'s" : "");
                        }
                        else
                        {
                            TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, args.Player.TileX,
                                args.Player.TileY, 2, 5);
                            TSPlayer.All.SendSuccessMessage("Spawned 1 {0}", npc.FullName);
                        }
                    }
                    else
                        args.Player.SendErrorMessage("Invalid mob ID or name");
                }
                else
                {
                    if (TShock.Utils.GetNPCByIdOrName(args.Parameters[0]).Count != 0)
                    {
                        NPC npc = TShock.Utils.GetNPCByIdOrName(args.Parameters[0])[0];

                        int amount;
                        if (int.TryParse(args.Parameters[1], out amount))
                        {
                            TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, amount, args.Player.TileX,
                                args.Player.TileY, 2, 5);
                            TSPlayer.All.SendSuccessMessage("Spawned {0} {1}{2}",
                                     amount, npc.FullName, amount > 1 || amount == 0 ? "'s" : "");
                        }
                        else
                        {
                            TSPlayer.Server.SpawnNPC(npc.type, npc.FullName, 1, args.Player.TileX,
                                args.Player.TileY, 2, 5);
                            TSPlayer.All.SendSuccessMessage("Spawned 1 {0}", npc.FullName);
                        }
                    }
                    else
                        args.Player.SendErrorMessage("Invalid mob ID or name");
                }
            }
        }
        #endregion


        //Player management
        #region MuteAll
        public static void MuteAll(CommandArgs args)
        {
            int muteCount = 0;

            MAC.muteAll = !MAC.muteAll;
            if (MAC.muteAll)
            {
                MAC.config.muteAllReason = "";
                MAC.config.muteAllReason = string.Join(" ", args.Parameters);

                if (MAC.config.muteAllReason == "")
                    MAC.config.muteAllReason = MAC.config.defaultMuteAllReason;

                foreach (TSPlayer player in TShock.Players)
                    if (player != null && !player.Group.HasPermission(Permissions.mute))
                    {
                        player.mute = true;
                        muteCount++;
                    }

                if (!args.Silent)
                    TSPlayer.All.SendInfoMessage(args.Player.Name + " has muted everyone.");
                args.Player.SendSuccessMessage("You have muted everyone ({0} people) without the mute permission. " +
					"They will remain muted until you use {1}muteall again.", muteCount, (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));

            }
            else
            {
                foreach (TSPlayer player in TShock.Players)
                    if (player != null && player.mute)
                        player.mute = false;

                TSPlayer.All.SendInfoMessage("{0} has unmuted everyone.", args.Player.Name);
            }
        }
        #endregion

        #region ViewAll
        public static void ViewAll(CommandArgs args)
        {
            var player = Utils.GetPlayers(args.Player.Index);

            player.viewAll = !player.viewAll;

            if (player.viewAll)
            {
                args.Player.SendInfoMessage("View All mode has been turned on.");
                if (!updateTimers.viewAllTimer.Enabled)
                    updateTimers.viewAllTimer.Enabled = true;
            }

            else
            {
                args.Player.SetTeam(Main.player[args.Player.Index].team);
                foreach (TSPlayer tply in TShock.Players)
                {
                    NetMessage.SendData((int)PacketTypes.PlayerTeam, args.Player.Index, -1, NetworkText.Empty, tply.Index);
                }
                args.Player.SendInfoMessage("View All mode has been turned off.");
            }
        }
        #endregion

        //Butchering
        #region ButcherNear
        public static void ButcherNear(CommandArgs args)
        {
            int nearby = 50;
            if (args.Parameters.Count > 0)
            {
                if (!int.TryParse(args.Parameters[0], out nearby))
                {
					args.Player.SendErrorMessage("Improper Syntax. Proper Syntax: {0}butchernear [distance]", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier)); 
                    return;
                }
            }
            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if ((Main.npc[i].active && !Main.npc[i].friendly &&
                    Utils.getDistance(args.Player.TPlayer.position, Main.npc[i].position, nearby)))
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 1f, 1);
                    killcount++;
                }
            }
            args.Player.SendInfoMessage(string.Format("Killed {0} NPC(s) within a radius of {1} blocks.", killcount, nearby));
			if (!args.Silent)
				TSPlayer.All.SendInfoMessage(string.Format("{0} killed {1} NPC{2}", args.Player.Name, killcount,
					killcount == 0 || killcount > 1 ? "s" : ""));
        }
        #endregion

        #region ButcherAll
        public static void ButcherAll(CommandArgs args)
        {
            int killcount = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
			if (!args.Silent)
            TSPlayer.All.SendInfoMessage(string.Format("{0} killed {1} NPC{2}.", args.Player.Name, killcount,
                killcount == 0 || killcount > 1 ? "s" : ""));
        }
        #endregion

        #region ButcherFriendly
        public static void ButcherFriendly(CommandArgs args)
        {
            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            args.Player.SendInfoMessage(string.Format("Killed {0} friendly NPCs.", killcount));
			if (!args.Silent)
				TSPlayer.All.SendInfoMessage(string.Format("{0} killed {1} friendly NPCs", args.Player.Name, killcount));
        }
        #endregion

        #region ButcherNPC
        public static void ButcherNPC(CommandArgs args)
        {

            if (args.Parameters.Count < 1)
            {
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}butchernpc <npc name/id>", (args.Silent ? TShock.Config.CommandSilentSpecifier : TShock.Config.CommandSpecifier));
                return;
            }

            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendErrorMessage("Missing npc name/id");
                return;
            }

            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);

            if (npcs.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid npc type!");
            }

            else if (npcs.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, npcs.Select(p => p.FullName));
            }

            else
            {
                var npc = npcs[0];

                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    int killcount = 0;

                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == npc.type)
                        {
                            TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                            killcount++;
                        }
                    }

                    args.Player.SendInfoMessage(string.Format("Killed {0} " + npc.FullName + "(s).", killcount));
					if (!args.Silent)
						TSPlayer.All.SendInfoMessage(string.Format("{0} {1}(s) were killed", npc.FullName, killcount));
                }

                else
                    args.Player.SendErrorMessage("Invalid npc type!");
            }
        }
        #endregion
    }
}
