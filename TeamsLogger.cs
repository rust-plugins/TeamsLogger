using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("Teams Logger", "Ujiou", "1.5.1")]
    [Description("Simple plugin to log team events.")]
    class TeamsLogger : CovalencePlugin
    {
        #region Config & Data   
        private class PluginData
        {
            public bool FirstStart { get; set; } = true;
        }

        private ConfigData configData;

        class ConfigData
        {
            [JsonProperty(PropertyName = "CONSOLE SETTINGS")]
            public ConsoleSettings conSettings { get; set; }

            [JsonProperty(PropertyName = "FILE SETTINGS")]
            public FileSettings fileSettings { get; set; }

            [JsonProperty(PropertyName = "DISCORD SETTINGS")]
            public DiscordSettings disSettings { get; set; }

            public class ConsoleSettings
            {
                [JsonProperty(PropertyName = "Log to Console")]
                public bool logToConsole { get; set; } = true;

                [JsonProperty(PropertyName = "On Logged Colors (1 - Red, 2 - Yellow, 3 - White)")]
                public int consColorScheme = 3;

                [JsonProperty(PropertyName = "Anti-Spam Measure (Seconds)")]
                public float conAntiSpamMeasure { get; set; } = 3.0f;
            }

            public class FileSettings
            {
                [JsonProperty(PropertyName = "Log to Files")]
                public bool logToFile { get; set; } = false;

                [JsonProperty(PropertyName = "Anti-Spam Measure (Seconds)")]
                public float fileAntiSpamTime { get; set; } = 3.0f;
            }

            public class DiscordSettings
            {
                [JsonProperty(PropertyName = "Log to Discord")]
                public bool logToDiscord { get; set; } = false;

                [JsonProperty(PropertyName = "Anti-Spam Measure (Seconds)")]
                public float discordTime { get; set; } = 3.0f;

                [JsonProperty(PropertyName = "Discord Rate Limit")]
                public int discordTimeout { get; set; } = 360;

                [JsonProperty("Webhook URL")]
                public string discordURL { get; set; } = "---DISCORD WEBHOOK HERE---";
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                    throw new JsonException();
            }
            catch
            {
                LoadDefaultConfig();
                SaveConfig();
            }
        }
        void SaveConfig() => Config.WriteObject(configData, true);

        protected override void LoadDefaultConfig()
        {
            LogWarning("Creating a new config file.");
            configData = LoadBaseConfig();
            SaveConfig();
        }

        private ConfigData LoadBaseConfig()
        {
            return new ConfigData
            {
                conSettings = new ConfigData.ConsoleSettings { },
                fileSettings = new ConfigData.FileSettings { },
                disSettings = new ConfigData.DiscordSettings { }
            };
        }
        #endregion

        #region Hooks
        private void Init() { if (IsFirstRun()) LogCurrentTeams(); }

        protected override void LoadDefaultMessages() => lang.RegisterMessages(new DefaultLocalization(), this); // Load Default Messages

        private void OnTeamCreated(BasePlayer player, RelationshipManager.PlayerTeam team) => KeyType(player,
                "OnTeamCreated", team.teamID, player.displayName, player.userID);

        private void OnTeamInvite(BasePlayer inviter, BasePlayer target) => KeyType(inviter,
               "OnTeamInvite", inviter.Team.teamID, inviter.displayName, inviter.userID, target.displayName, target.userID);

        private void OnTeamAcceptInvite(RelationshipManager.PlayerTeam team, BasePlayer player) => KeyType(player,
              "OnTeamAcceptInvite", team.teamID, player.displayName, player.userID, team.GetLeader().displayName);

        private void OnTeamRejectInvite(BasePlayer rejector, RelationshipManager.PlayerTeam team) => KeyType(rejector,
               "OnTeamRejectInvite", team.teamID, rejector.displayName, rejector.userID, team.GetLeader());

        private void OnTeamPromote(RelationshipManager.PlayerTeam team, BasePlayer newLeader) => KeyType(newLeader,
               "OnTeamPromote", team.teamID, newLeader.displayName, newLeader.userID);

        private void OnTeamLeave(RelationshipManager.PlayerTeam team, BasePlayer player) => KeyType(player,
               "OnTeamLeave", team.teamID, player.displayName, player.userID, player.userID == team.teamLeader);

        private void OnTeamKick(RelationshipManager.PlayerTeam team, BasePlayer player, ulong target) => KeyType(player,
                "OnTeamKick", team.teamID, target, player.displayName);

        private void OnTeamDisbanded(RelationshipManager.PlayerTeam team) => KeyType(team.GetLeader(),
                "OnTeamDisbanded", team.teamID, team.GetLeader().displayName, team.GetLeader().userID);
        #endregion

        #region Helpers
        List<ulong> discordID = new List<ulong>();
        List<ulong> fileID = new List<ulong>();
        List<ulong> playerId = new List<ulong>();
        private readonly Dictionary<string, string> headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };

        private void PrintToConsole(BasePlayer player, string log)
        {
            if (playerId.Contains(player.userID)) return;
            playerId.Add(player.userID);
            timer.Once(configData.conSettings.conAntiSpamMeasure, () => playerId.Remove(player.userID));
            switch (configData.conSettings.consColorScheme)
            {
                case 1:
                    LogError(log);
                    return;
                case 2:
                    LogWarning(log);
                    return;
                case 3:
                    Puts(log);
                    return;
                default:
                    Puts("There was an issue with loading the config. You need to set the value to:");
                    LogError("1 - Red");
                    LogWarning("2 - Yellow");
                    Puts("3 - White");
                    return;
            }
        }

        private void PrintToFile(BasePlayer player, string log)
        {
            var timestamp = DateTime.UtcNow.ToString("hh:mm:ss");
            log = $"{timestamp} | {log}";
            if (fileID.Contains(player.userID)) return;
            fileID.Add(player.userID);
            timer.Once(configData.fileSettings.fileAntiSpamTime, () => fileID.Remove(player.userID));
            LogToFile("common", log, this);
        }

        private void PrintToDiscord(BasePlayer player, string log)
        {
            if (!configData.disSettings.logToDiscord) return;
            if (string.IsNullOrEmpty(configData.disSettings.discordURL))
            {
                LogError("You need to provide a valid URL!");
                return;
            }
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            if (discordID.Contains(player.userID)) return;
            discordID.Add(player.userID);
            timer.Once(configData.disSettings.discordTime, () => discordID.Remove(player.userID));
            webrequest.Enqueue(configData.disSettings.discordURL, "{\"content\": \"" + timestamp + " | " + log + "\"}", (code, response) => { }, this, RequestMethod.POST, headers, configData.disSettings.discordTimeout);

        }

        private void KeyType(BasePlayer player, string key, params object[] args) => PrintingMessages(player, this[key, args]);

        private void PrintingMessages(BasePlayer player, string log)
        {
            if (configData.conSettings.logToConsole) PrintToConsole(player, log);
            if (configData.fileSettings.logToFile) PrintToFile(player, log);
            if (configData.disSettings.logToDiscord) PrintToDiscord(player, log);
        }

        #region Logging Previously Create Teams
        private void OtherType(string key, params object[] args) => OtherMessages(this[key, args]);

        private bool IsFirstRun()
        {
            var dataFileName = $"{GetType().Name}.{nameof(PluginData)}";
            var data = Interface.uMod.DataFileSystem.ReadObject<PluginData>(dataFileName);
            if (data.FirstStart)
            {
                data.FirstStart = false;
                Interface.uMod.DataFileSystem.WriteObject(dataFileName, data);
                return true;
            }
            return false;
        }

        private string GetUserInfo(ulong userId)
        {
            var user = covalence.Players.FindPlayerById(userId.ToString());
            return user == null ? userId.ToString() : $"{user.Name} ({userId})";
        }

        private void LogCurrentTeams()
        {
            var teams = RelationshipManager.ServerInstance.teams;
            foreach (var team in teams)
            {
                OtherType("CurrentTeam", team.Key, string.Join(", ", team.Value.members.Select(GetUserInfo)));
            }
        }

        private void OtherPrintToFile(string log)
        {
            var timestamp = DateTime.UtcNow.ToString("hh:mm:ss");
            log = $"{timestamp} | {log}";
            LogToFile("common", log, this);
        }

        private void OtherPrintToDiscord(string log)
          {
            if (!configData.disSettings.logToDiscord) return;
            if (string.IsNullOrEmpty(configData.disSettings.discordURL))
            {
                LogError("You need to provide a valid URL!");
                return;
            }
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            webrequest.Enqueue(configData.disSettings.discordURL, "{\"content\": \"" + timestamp + " | " + log + "\"}", (code, response) => { }, this, RequestMethod.POST, headers, configData.disSettings.discordTimeout);
        }

        private void OtherPrintToConsole(string log)
        {
            switch (configData.conSettings.consColorScheme) {
                case 1:
                    LogError(log);
                    return;
                case 2:
                    LogWarning(log);
                    return;
                case 3:
                    Puts(log);
                    return;
                default:
                    break;
            }
        }

        private void OtherMessages(string log)
        {
            if (configData.conSettings.logToConsole) OtherPrintToConsole(log);
            if (configData.fileSettings.logToFile) OtherPrintToFile(log);
            if (configData.disSettings.logToDiscord) OtherPrintToDiscord(log);
        }

        #endregion

        private string this[string key, params object[] args] => args?.Any() == true ? string.Format(lang.GetMessage(key, this), args) : lang.GetMessage(key, this);
        #endregion

        #region Localization
        private class DefaultLocalization : Dictionary<string, string>
        {
            public DefaultLocalization()
            {
                this["OnTeamCreated"] = "OnTeamCreated | Team ID: {0} | Player: '{1}' ({2}) created a new team.";
                this["OnTeamDisbanded"] = "OnTeamDisbanded | Team ID: {0} | Team Leader: '{1}' ({2}).";
                this["OnTeamLeave"] = "OnTeamLeave | Team ID: {0} | Player: '{1}' ({2}, is leader: {3}).";
                this["OnTeamInvite"] = "OnTeamInvite | Team ID: {0} | Player '{1}' ({2}) sent invite to {3} ({4}).";
                this["OnTeamRejectInvite"] = "OnTeamRejectInvite | Team ID: {0} | Player: '{1}' ({2}) rejected invite from {3}.";
                this["OnTeamPromote"] = "OnTeamPromote | Team ID: {0} | Player: '{1}' ({2}) is the new leader.";
                this["OnTeamKick"] = "OnTeamKick | Team ID: {0} | {1} was kicked. | Team Leader: {2}";
                this["OnTeamAcceptInvite"] = "OnTeamAcceptInvite | Team ID: {0} | Player: '{1}' ({2}) accepted invite from {3}.";
                this["CurrentTeam"] = "CurrentTeams: {0} | Previously created team with members: {1}";
            }
        }
        #endregion
    }
}