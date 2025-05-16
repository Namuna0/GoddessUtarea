using Discord.WebSocket;

partial class Program
{
    class NpcStatus
    {
        public short MaxHp { get; set; }
        public short Hp { get; set; }
        public float AgiB { get; set; }
    }

    private async Task ShowNpcRes(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?show npc res ".Length);
        var texts = text.Split(" ");

        await Command(texts, 3, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            await DisplayNpcResource(texts[0], status, message);
        });
    }

    private async Task SetNpcRes(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set npc res ".Length);
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

            await DisplayNpcResource(texts[0], status, message);
        });
    }

    private async Task UpdateNpcHp(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?npc hp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            status.Hp += short.Parse(texts[1]);

            await DisplayNpcResource(texts[0], status, message);
        });
    }

    private async Task ShowNpcBon(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?show npc bon ".Length);
        var texts = text.Split(" ");
        await Command(texts, 3, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            await DisplayNpcBonus(texts[0], status, message);
        });
    }

    private async Task SetNpcBon(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?set npc agi ".Length);
        var texts = text.Split(" ");
        await Command(texts, 23, message, user, async (currentChara) =>
        {
            if (!_npcStatus.TryGetValue(texts[0], out var status))
            {
                status = new NpcStatus();
                _npcStatus.Add(texts[0], status);
            }

            status.AgiB = float.Parse(texts[1]);

            await DisplayNpcBonus(texts[0], status, message);
        });
    }

    private async Task NpcDiceRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?npc r ".Length);
        var texts = text.Split(" ");

        if (!GetPaseFlag(texts, 33) && !GetPaseFlag(texts, 333))
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        if (!_npcStatus.TryGetValue(texts[0], out var status))
        {
            status = new NpcStatus();
            _npcStatus.Add(texts[0], status);
        }

        NpcCalcFormula(texts[1], status, out string culcResult, out string showResult);

        string comment = string.Empty;
        if (texts.Length > 2)
        {
            comment = $"：{texts[2]}";
        }

        await message.Channel.SendMessageAsync(
        $"<@{user.Id}> :game_die:{texts[0]}{comment}\r\n" +
        $"{showResult}=>{culcResult}");
    }

    private async Task DisplayNpcResource(string npc, NpcStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
                $"{npc}\r\n" +
                "●リソース\r\n" +
                $"【HP】{status.Hp}/{status.MaxHp}\r\n" +
                "●状態\r\n");
    }

    private async Task DisplayNpcBonus(string npc, NpcStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{npc}\r\n" +
            "●能力値B\r\n" +
            $"{status.AgiB.ToString("0.##")} 敏捷");
        ;
    }

    private void NpcCalcFormula(string originalText, NpcStatus status, out string culcResult, out string showResult)
    {
        string culcText = originalText;
        string showText = originalText;

        if (status != null)
        {
            ReplaceBonus("[敏捷B]", status.AgiB, ref culcText, ref showText);
        }

        CalcDice(ref culcText, ref showText);

        if (status != null)
        {
            CalcBonusDice("敏捷R", status.AgiB, ref culcText, ref showText);
        }

        var expr = new NCalc.Expression(culcText);

        showText = showText.Replace("*", @"\*");

        culcResult = float.Parse(expr.Evaluate()?.ToString() ?? "0").ToString("0.##");
        showResult = showText;
    }
}
