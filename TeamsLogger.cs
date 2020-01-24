using System;
using System.Linq;
using Oxide.Core;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("TeamsLogger", "NickRimmer", "1.0")]
    [Description("Simple plugin to log team events")]
    public class TeamsLogger : RustPlugin
    {
        private PluginConfig _config;

        void Init()
        {
            _config = Config.ReadObject<PluginConfig>();
            if (IsFirstRun()) LogCurrentTeams();
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

        private int LogCurrentTeams()
        {
            var teams = RelationshipManager.Instance.teams;
            if (!teams?.Any() == true) return 0;

            foreach (var team in teams)
            {
                LogKey(
                    "CurrentTeam",
                    team.Key,
                    string.Join(", ", team.Value.members.Select(GetUserInfo)));
            }

            return teams.Count;
        }

        private string GetUserInfo(ulong userId)
        {
            var user = Player.Players.FirstOrDefault(x => x.userID == userId);
            return user == null
                ? userId.ToString()
                : $"{user.displayName} ({userId})";
        }
        #endregion

        #region Team hooks

        object OnTeamInvite(BasePlayer inviter, BasePlayer target) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            inviter.Team?.teamID.ToString() ?? "-",
            inviter.displayName,
            inviter.userID,
            target.displayName,
            target.userID);

        object OnTeamRejectInvite(BasePlayer rejector, RelationshipManager.PlayerTeam team) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            rejector.displayName,
            rejector.userID);

        object OnTeamPromote(RelationshipManager.PlayerTeam team, BasePlayer newLeader) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            newLeader.displayName,
            newLeader.userID);

        object OnTeamLeave(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            player.displayName,
            player.userID,
            player.userID == team.teamLeader);

        object OnTeamKick(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            player.displayName,
            player.Team);

        object OnTeamAcceptInvite(RelationshipManager.PlayerTeam team, BasePlayer player) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            player.displayName,
            player.userID);

        void OnTeamDisbanded(RelationshipManager.PlayerTeam team) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            team.teamID,
            team.GetLeader().displayName,
            team.teamLeader);

        /*private object OnTeamUpdate(ulong currentTeam, ulong newTeam, BasePlayer player) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
            newTeam,
            player.displayName,
            player.userID,
            currentTeam);*/

        private void OnTeamCreated(BasePlayer player, RelationshipManager.PlayerTeam team) => LogKey(
            System.Reflection.MethodBase.GetCurrentMethod().Name,
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

        private object LogKey(string key, params object[] args) =>
            LogMessage(this[key, args]);

        private object LogMessage(string message)
        {
            if(_config.PrintToConsole) PrintToConsole(message);
            if(_config.PrintToFile) PrintToFile(message);
            return null;
        }

        private void PrintToConsole(string message) =>
            Puts(message.Replace('\t', ' '));

        private void PrintToFile(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = $"{timestamp} | {message}";

            LogToFile("common", message, this);
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
                this["OnTeamInvite"] =       "Invite {0}   | Player '{1}' ({2}) sent invite to '{3}' (4)";
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
                this["OnTeamInvite"] =       "Приглашение {0} | Игрок '{1}' ({2}) отправил приглашение игроку '{3}' (4)";
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
        }
    
        private class PluginData
        {
            public bool FirstStart { get; set; } = true;
        }
    
        #endregion
    }
}
