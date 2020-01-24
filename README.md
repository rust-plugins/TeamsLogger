# TeamsLogger
Simple logger for teams provides logging to console and/or own log file.

# Configuration
You can change it in file `\oxide\config\TeamsLogger.json`
```JSON
{
  "Print logs to console": true,
  "Print logs to file": true
}
``` 
# Log file example
Path to logs `\oxide\logs\TeamsLogger\teamslogger_common-####-##-##.txt`
```
17:36:22 Created 96  | Player: 'xan' (76561198051734570) created new team
17:36:23 Leave 96    | Player: 'xan' (76561198051734570, is leader: True)
17:36:23 Disband 96  | Team leader: 'xan' (76561198051734570)
17:36:25 Created 97  | Player: 'xan' (76561198051734570) created new team
17:36:26 Leave 97    | Player: 'xan' (76561198051734570, is leader: True)
17:36:26 Disband 97  | Team leader: 'xan' (76561198051734570)
```

# Localization messages
You can specify messages with formatting in file `\oxide\lang\##\TeamsLogger.json`
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