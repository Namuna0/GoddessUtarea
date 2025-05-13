using Discord.WebSocket;

partial class Program
{
    class NpcStatus
    {
        public short MaxHp { get; set; }
        public short Hp { get; set; }
        public float AgiB { get; set; }
    }

    private async Task ShowNpcRes(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?show npc res ".Length);
        var texts = text.Split(" ");

        await Command(texts, 3, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            await ShowNpcResource(texts[0], status, message);
        });
    }

    private async Task SetNpcRes(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set npc res ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            status.MaxHp = short.Parse(texts[1]);
            status.Hp = status.MaxHp;

            await ShowNpcResource(texts[0], status, message);
        });
    }

    private async Task UpdateNpcHp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring($"?npc hp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            status.Hp += short.Parse(texts[1]);

            await ShowNpcResource(texts[0], status, message);
        });
    }

    private async Task ShowNpcBon(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring($"?show npc bon ".Length);
        var texts = text.Split(" ");
        await Command(texts, 3, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            await ShowNpcBonus(texts[0], status, message);
        });
    }

    private async Task SetNpcBon(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring($"?set npc agi ".Length);
        var texts = text.Split(" ");
        await Command(texts, 23, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            status.AgiB = float.Parse(texts[1]);

            await ShowNpcBonus(texts[0], status, message);
        });
    }

    private async Task ShowNpcResource(string npc, NpcStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
                $"{npc}\r\n" +
                "●リソース\r\n" +
                $"【HP】{status.Hp}/{status.MaxHp}\r\n" +
                "●状態\r\n");
    }

    private async Task ShowNpcBonus(string npc, NpcStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{npc}\r\n" +
            "●能力値B\r\n" +
            $"{status.AgiB.ToString("0.##")} 敏捷");
        ;
    }
}
