Simple logger for teams provides additional logging. It can be configured to log into files, console output, Discord channel.

## Installation
To use this plugin with your Rust server, just copy `TeamsLogger.cs` into `\oxide\plugins` folder.  
It will create default language files and default configuration.

After plugin will be loaded, you can change configuration file.  
Then restart plugin using command in server console `oxide.reload TeamsLogger`

## Configuration
```JSON
{
  "Print logs to console": true,
  "Print logs to file": true,
  "Print logs to Discord": false,

  "DiscordBot": {
    "Api key": null,
    "Channel name": "teams-logger",
    "Show server name in status": true
  }
}
``` 

### Discord extension
To log messages into Discord, you'll need to install [Discord extension](https://umod.org/extensions/discord) and replace text `DISCORD_NOT_INSTALLED` by `DISCORD_INSTALLED` in the beginning of plugin file. 

### Discord Api key
To send messages, set `Api key` in configuration file. Instruction how to get this key you will find on [Discord extension page](https://umod.org/extensions/discord#getting-your-api-key).

## Localization
```JSON
{
  "OnTeamCreated":      "Created {0}  | Player: '{1}' ({2}) created new team",
  "OnTeamDisbanded":    "Disband {0}  | Team leader: '{1}' ({2})",
  "OnTeamLeave":        "Leave {0}    | Player: '{1}' ({2}, is leader: {3})",
  "OnTeamInvite":       "Invite {0}   | Player '{1}' ({2}) sent invite to '{3}' (4)",
  "OnTeamRejectInvite": "Reject {0}   | Player: '{1}' ({2}) rejected invite",
  "OnTeamPromote":      "Promote {0}  | Player: '{1}' ({2}) now is new leader",
  "OnTeamKick":         "Kick {0}     | Player: '{1}' ({2}) was kicked",
  "OnTeamAcceptInvite": "Accept {0}   | Player: '{1}' ({2}) accepted invite",
  "CurrentTeam":        "History {0}  | Previously created team with members: {1}"
}
```
## Log example
To file `\oxide\logs\TeamsLogger\teamslogger_common-####-##-##.txt`
```
17:36:22 Created 96  | Player: 'xan' (76561198051734570) created new team
17:36:23 Leave 96    | Player: 'xan' (76561198051734570, is leader: True)
17:36:23 Disband 96  | Team leader: 'xan' (76561198051734570)
17:36:25 Created 97  | Player: 'xan' (76561198051734570) created new team
17:36:26 Leave 97    | Player: 'xan' (76561198051734570, is leader: True)
17:36:26 Disband 97  | Team leader: 'xan' (76561198051734570)
```
### To Discord
![](https://i.imgur.com/VohQkDJ.png)