using Discord.WebSocket;
using System.Text.RegularExpressions;

partial class Program
{
    private async Task Login(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?login ".Length);
        var texts = text.Split(" ");

        if (!GetPaseFlag(texts, 3))
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        _currentCharaDic[user.Id] = texts[0];

        await message.Channel.SendMessageAsync($"こんにちは、{texts[0]}さん！");
    }

}
