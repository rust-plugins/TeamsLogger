#define DISCORD_NOT_INSTALLED

using Oxide.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

#if DISCORD_INSTALLED
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.DiscordObjects;
#endif

namespace Oxide.Plugins
{
    [Info("Teams Logger", "NickRimmer", "1.2")]
    [Description("Simple plugin to log team events")]
    public class TeamsLogger : RustPlugin
    {
        #if DISCORD_INSTALLED
        [DiscordClient]
        private DiscordClient _client;
        private Channel _discordChannel;
        #endif

        private PluginConfig _config;

        void Init()
        {
            _config = Config.ReadObject<PluginConfig>();
            if (IsFirstRun()) LogCurrentTeams();
        }

        void OnServerInitialized()
        {
            #if DISCORD_INSTALLED
            if (_config.PrintToDiscord) InitDiscord();
            #else
            if (_config.PrintToDiscord) PrintWarning("To enable Discord features, please define DISCORD_INSTALLED directive");
            #endif
        }

        private void Unload()
        {
            #if DISCORD_INSTALLED
            if (_client != null) Discord.CloseClient(_client);
            #endif
        }

        private void PrintToConsole(string message) => Puts(message);

        private void PrintToFile(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = $"{timestamp} | {message}";

            LogToFile("common", message, this);
        }

        private void PrintToDiscord(string message)
        {
            #if DISCORD_INSTALLED
            if (_client?.DiscordServer == null)
            {
                PrintWarning("Discord doesn't connected");
                return;
            }

            if (_discordChannel == null)
            {
                PrintWarning($"Discord channel with name '{_config.DiscordBot.ChannelName}' not found");
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = $"{timestamp} | {message}";
            _discordChannel.CreateMessage(_client, message);
            #else
            PrintWarning("To enable discord features, please define DISCORD_INSTALLED directive");
            #endif
        }

        #region Log exist teams on first run

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

        private void LogCurrentTeams()
        {
            var teams = RelationshipManager.Instance.teams;
            foreach (var team in teams)
            {
                LogKey(
                    "CurrentTeam",
                    team.Key,
                    string.Join(", ", team.Value.members.Select(GetUserInfo)));
            }
        }

        private string GetUserInfo(ulong userId)
        {
            var user = covalence.Players.FindPlayerById(userId.ToString());
            return user == null
                ? userId.ToString()
                : $"{user.Name} ({userId})";
        }
        #endregion

        #region Discord connection
        #if DISCORD_INSTALLED
        private void InitDiscord()
        {
            Puts("Init Discord connection...");
            if (string.IsNullOrEmpty(_config.DiscordBot.ApiKey))
            {
                PrintError($"To enable Discord messages you need to specify 'DiscordConfig.ApiKey' value in config");
                return;
            }

            Discord.CreateClient(this, _config.DiscordBot.ApiKey);
        }

        void Discord_GuildCreate(Guild guild)
        {
            if(_config.DiscordBot.EnableBotStatus)
                _client.UpdateStatus(new Presence { Game = new Ext.Discord.DiscordObjects.Game
                {
                    Name = covalence.Server.Name,
                    Type = ActivityType.Watching
                }});

            UpdateDiscordChannel();
        }

        void Discord_ChannelCreated(Channel channel) => UpdateDiscordChannel();
        void Discord_ChannelUpdate(Channel updatedChannel, Channel oldChannel) => UpdateDiscordChannel();
        void Discord_ChannelDelete(Channel channel) => UpdateDiscordChannel();
        //void Discord_GuildUpdate(Guild guild) => UpdateDiscordChannel();

        private void UpdateDiscordChannel()
        {
            _discordChannel = _client
                .DiscordServer
                .channels
                .FirstOrDefault(x => x.name.Equals(_config.DiscordBot.ChannelName, StringComparison.InvariantCultureIgnoreCase));

            if (_discordChannel == null) PrintWarning($"Discord channel with name '{_config.DiscordBot.ChannelName}' not found");
            else Puts($"Connected to discord channel: '{_discordChannel.name}' ({_discordChannel.id})");
        }
        
        #endif
        #endregion

        #region Team hooks

        void OnTeamInvite(BasePlayer inviter, BasePlayer target) => LogKey(
            "OnTeamInvite",
            inviter.Team.teamID,
            inviter.displayName,
            inviter.userID,
            target.displayName,
            target.userID);

        void OnTeamRejectInvite(BasePlayer rejector, RelationshipManager.PlayerTeam team) => LogKey(
            "OnTeamRejectInvite",
            team.teamID,
            rejector.displayName,
            rejector.userID);

        void OnTeamPromote(RelationshipManager.PlayerTeam team, BasePlayer newLeader) => LogKey(
            "OnTeamPromote",
            team.teamID,
            newLeader.displayName,
            newLeader.userID);

        void OnTeamLeave(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            "OnTeamLeave",
            team.teamID,
            player.displayName,
            player.userID,
            player.userID == team.teamLeader);

        void OnTeamKick(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            "OnTeamKick",
            team.teamID,
            player.displayName,
            player.Team);

        void OnTeamAcceptInvite(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            "OnTeamAcceptInvite",
            team.teamID,
            player.displayName,
            player.userID);

        void OnTeamDisbanded(RelationshipManager.PlayerTeam team) => LogKey(
            "OnTeamDisbanded",
            team.teamID,
            team.GetLeader().displayName,
            team.teamLeader);

        /*private void OnTeamUpdate(ulong currentTeam, ulong newTeam, BasePlayer player) => LogKey(
            "OnTeamUpdate",
            newTeam,
            player.displayName,
            player.userID,
            currentTeam);*/

        private void OnTeamCreated(BasePlayer player, RelationshipManager.PlayerTeam team) => LogKey(
            "OnTeamCreated",
            team.teamID,
            player.displayName,
            player.userID);

        #endregion

        #region Boring things

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new RuLocalization(), this, "ru");
            lang.RegisterMessages(new DefaultLocalization(), this);
        }
        protected override void LoadDefaultConfig() => Config.WriteObject(new PluginConfig(), true);

        private void LogKey(string key, params object[] args) =>
            LogMessage(this[key, args]);

        private void LogMessage(string message)
        {
            if (_config.PrintToConsole) PrintToConsole(message);
            if (_config.PrintToFile) PrintToFile(message);
            if (_config.PrintToDiscord) PrintToDiscord(message);
        }

        private string this[string key, params object[] args] => args?.Any() == true
            ? string.Format(lang.GetMessage(key, this), args)
            : lang.GetMessage(key, this);

        #endregion
    
        #region TeamsLogger.Localization

        private class DefaultLocalization : Dictionary<string, string>
        {
            public DefaultLocalization()
            {
                this["OnTeamCreated"] =      "Created {0}  | Player: '{1}' ({2}) created new team";
                this["OnTeamDisbanded"] =    "Disband {0}  | Team leader: '{1}' ({2})";
                this["OnTeamLeave"] =        "Leave {0}    | Player: '{1}' ({2}, is leader: {3})";
                this["OnTeamInvite"] =       "Invite {0}   | Player '{1}' ({2}) sent invite to '{3}' {4}";
                this["OnTeamRejectInvite"] = "Reject {0}   | Player: '{1}' ({2}) rejected invite";
                this["OnTeamPromote"] =      "Promote {0}  | Player: '{1}' ({2}) now is new leader";
                this["OnTeamKick"] =         "Kick {0}     | Player: '{1}' ({2}) was kicked";
                this["OnTeamAcceptInvite"] = "Accept {0}   | Player: '{1}' ({2}) accepted invite";
                //this["OnTeamUpdate"] =     "Update {0}   | Player: '{1}' ({2}) changed group {3} to {0}";
                this["CurrentTeam"] =        "History {0}  | Previously created team with members: {1}";
            }
        }
    
        private class RuLocalization : Dictionary<string, string>
        {
            public RuLocalization()
            {
                this["OnTeamCreated"] =      "Создана {0}     | Игрок: '{1}' ({2}) создал новую группу";
                this["OnTeamDisbanded"] =    "Удалена {0}     | Лидер: '{1}' ({2})";
                this["OnTeamLeave"] =        "Покинул {0}     | Игрок: '{1}' ({2}, лидер: {3})";
                this["OnTeamInvite"] =       "Приглашение {0} | Игрок '{1}' ({2}) отправил приглашение игроку '{3}' {4}";
                this["OnTeamRejectInvite"] = "Отказ {0}       | Игрок: '{1}' ({2}) отказался вступать в группу";
                this["OnTeamPromote"] =      "Лидер {0}       | Игрок: '{1}' ({2}) теперь новый лидер группы";
                this["OnTeamKick"] =         "Изгнан {0}      | Игрок: '{1}' ({2}) был выгнан из команды";
                this["OnTeamAcceptInvite"] = "Вступил {0}     | Игрок: '{1}' ({2}) принял приглашение вступить в группу";
                //this["OnTeamUpdate"] =     "Переход {0}     | Игрок: '{1}' ({2}) перешел из группы {3} в {0}";
                this["CurrentTeam"] =        "История {0}     | Ранее созданная группа с участниками: {1}";
            }
        }
    
        #endregion
    
        #region TeamsLogger.Models

        private class PluginConfig
        {
            [JsonProperty("Print logs to console")]
            public bool PrintToConsole { get; set; } = true;
    
            [JsonProperty("Print logs to file")]
            public bool PrintToFile { get; set; } = true;
    
            [JsonProperty("Print logs to Discord")]
            public bool PrintToDiscord { get; set; } = false;
    
            public PluginDiscordConfig DiscordBot = new PluginDiscordConfig();
    
            public class PluginDiscordConfig
            {
                [JsonProperty("Api key")] 
                public string ApiKey { get; set; }
    
                [JsonProperty("Channel name")]
                public string ChannelName { get; set; } = "teams-logger";
    
                [JsonProperty("Show server name in status")]
                public bool EnableBotStatus { get; set; } = true;
            }
        }
    
        private class PluginData
        {
            public bool FirstStart { get; set; } = true;
        }
    
        #endregion
    }
}
