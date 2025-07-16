# SimpleTags CS2
A customizable chat and scoreboard tag system for Counter-Strike 2 servers with permission-based tags, chat formatting, and persistent player preferences.

## 🚀 Installation

### Basic Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [SimpleTags.zip](https://github.com/wiruwiru/SimpleTags-CS2/releases/latest) from releases
3. Extract and upload to your game server: `csgo/addons/counterstrikesharp/plugins/SimpleTags/`
4. Start server and configure the generated config file at `csgo/addons/counterstrikesharp/configs/plugins/SimpleTags/`

### Optional Dependencies (for PlayerSettings storage)
If you want persistent player preferences across server restarts:
1. Install [PlayerSettingsCS2](https://github.com/NickFox007/PlayerSettingsCS2/releases/latest) (required dependency)
2. Install [AnyBaseLibCS2](https://github.com/NickFox007/AnyBaseLibCS2/releases/latest) (required for PlayerSettings)

---

## 📋 Main Configuration Parameters

| Parameter            | Description                                                                                       | Required |
|----------------------|---------------------------------------------------------------------------------------------------|----------|
| `Commands`           | List of chat commands players can use to toggle tag display. (**Default**: `["css_tag", "css_tags"]`) | **YES**  |
| `CommandPermissions` | Permission flag required to use toggle commands. Leave empty for all players. (**Default**: `""`) | **YES**  |
| `Tags`               | Dictionary of tag configurations for different permission groups, SteamIDs, or "everyone". | **YES**  |
| `Settings`           | General plugin settings including default state and tag display method. | **YES**  |

### Tag Configuration Parameters
Each tag in the `Tags` dictionary supports the following properties:

| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `prefix`          | Text displayed before player name in chat messages. Supports color codes. (**Default**: `""`) | **YES**  |
| `nick_color`      | Color code for player name in chat. (**Default**: `""`) | **YES**  |
| `message_color`   | Color code for chat message text. (**Default**: `""`) | **YES**  |
| `scoreboard`      | Tag displayed on scoreboard (clan tag or name prefix). (**Default**: `""`) | **YES**  |
| `team_chat`       | Whether this tag applies to team chat messages. (**Default**: `true`) | **YES**  |

### Settings Parameters
| Parameter         | Description                                                                                         | Required |
|-------------------|-----------------------------------------------------------------------------------------------------|----------|
| `DefaultEnabled`  | Whether tags are enabled by default for new players. (**Default**: `true`) | **YES**  |
| `TagMethod`       | How tags are displayed on scoreboard: `"ClanTag"` or `"Rename"`. (**Default**: `"ClanTag"`) | **YES**  |

---

### Tag Method Types

| Method | Description | Behavior |
|--------|-------------|----------|
| **ClanTag** | Uses CS2's clan tag system | Displays tag in square brackets on scoreboard |
| **Rename** | Modifies player name | Adds tag as prefix to player name |

---

## 📊 Support

For issues, questions, or feature requests, please visit our [GitHub Issues](https://github.com/wiruwiru/SimpleTags-CS2/issues) page.