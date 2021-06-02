Simple plugin to provide additional logging of teams events. It can be configured to log into files, console output, Discord channel.

## Configuration

```JSON
{
  "CONSOLE SETTINGS": {
    "Anti-Spam Measure (Seconds)": 3.0,
    "Log to Console": true,
    "On Logged Colors (1 - Red, 2 - Yellow, 3 - White)": 3
  },
  "DISCORD SETTINGS": {
    "Anti-Spam Measure (Seconds)": 3.0,
    "Discord Rate Limit": 360,
    "Log to Discord": false,
    "Webhook URL": "---DISCORD WEBHOOK HERE---"
  },
  "FILE SETTINGS": {
    "Anti-Spam Measure (Seconds)": 3.0,
    "Log to Files": false
  }
}
``` 

### Discord

To send logs via Discord, you'll need to configure [Discord WebHook url](https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks).

## Localization

```JSON

  "OnTeamCreated": "OnTeamCreated | Team ID: {0} | Player: '{1}' ({2}) created a new team.",
  "OnTeamDisbanded": "OnTeamDisbanded | Team ID: {0} | Team Leader: '{1}' ({2}).",
  "OnTeamLeave": "OnTeamLeave | Team ID: {0} | Player: '{1}' ({2}, is leader: {3}).",
  "OnTeamInvite": "OnTeamInvite | Team ID: {0} | Player '{1}' ({2}) sent invite to {3} ({4}).",
  "OnTeamRejectInvite": "OnTeamRejectInvite | Team ID: {0} | Player: '{1}' ({2}) rejected invite from {3}.",
  "OnTeamPromote": "OnTeamPromote | Team ID: {0} | Player: '{1}' ({2}) is the new leader.",
  "OnTeamKick": "OnTeamKick | Team ID: {0} | {1} was kicked. | Team Leader: {2}",
  "OnTeamAcceptInvite": "OnTeamAcceptInvite | Team ID: {0} | Player: '{1}' ({2}) accepted invite from {3}.",
  "CurrentTeam": "CurrentTeams: {0} | Previously created team with members: {1}"
}
```

## Log Example

To file `oxide/logs/TeamsLogger\/eamslogger_common-####-##-##.txt`
```
08:51:22 | OnTeamCreated | Team ID: 12 | Player: 'Ujiou' (765611) created a new team.
08:51:24 | OnTeamLeave | Team ID: 12 | Player: 'Ujiou' (765611, is leader: True).
08:51:25 | OnTeamCreated | Team ID: 13 | Player: 'Ujiou' (765611) created a new team.
08:51:27 | OnTeamLeave | Team ID: 13 | Player: 'Ujiou' (765611, is leader: True).
08:51:28 | OnTeamCreated | Team ID: 14 | Player: 'Ujiou' (765611) created a new team.
08:51:33 | OnTeamLeave | Team ID: 14 | Player: 'Ujiou' (765611, is leader: True).
```