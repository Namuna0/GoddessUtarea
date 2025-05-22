using Discord.WebSocket;
using System.Text.RegularExpressions;

partial class Program
{
    class NpcStatus
    {
        public short MaxHp { get; set; }
        public short Hp { get; set; }
        public float AgiB { get; set; }
    }

    private async Task SimpleRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        CalcFormula("1d100", null, out string culcResult, out string showResult);

        await message.Channel.SendMessageAsync(
        $"<@{user.Id}> :game_die:{currentChara}\r\n" +
        $"{showResult}=>{culcResult}");
    }

    private async Task DiceRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?r ".Length);
        var texts = text.Split(" ");

        if (!GetPaseFlag(texts, 3) && !GetPaseFlag(texts, 33))
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await ConnectDatabase(
            @"SELECT vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b, wep_p FROM character_equipment WHERE id = @id",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
            },
            async (reader) =>
            {
                var status = new EquipmentStatus();
                status.VitB = reader.GetFloat(0);
                status.PowB = reader.GetFloat(1);
                status.StrB = reader.GetFloat(2);
                status.IntB = reader.GetFloat(3);
                status.MagB = reader.GetFloat(4);
                status.DexB = reader.GetFloat(5);
                status.AgiB = reader.GetFloat(6);
                status.SnsB = reader.GetFloat(7);
                status.AppB = reader.GetFloat(8);
                status.LukB = reader.GetFloat(9);
                status.WepP = reader.GetString(10);

                CalcFormula(texts[0], status, out string culcResult, out string showResult);

                string comment = string.Empty;
                if (texts.Length > 1)
                {
                    comment = $"：{texts[1]}";
                }

                await message.Channel.SendMessageAsync(
                $"<@{user.Id}> :game_die:{currentChara}{comment}\r\n" +
                $"{showResult}=>{culcResult}");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
            });
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
    private void CalcFormula(string originalText, EquipmentStatus status, out string culcResult, out string showResult)
    {
        string culcText = originalText;
        string showText = originalText;

        if (status != null)
        {
            ReplaceWeponPower(@"武器威力R:(\d+)", status.WepP, ref culcText, ref showText);

            ReplaceBonus("[生命B]", status.VitB, ref culcText, ref showText);
            ReplaceBonus("[精神B]", status.PowB, ref culcText, ref showText);
            ReplaceBonus("[筋力B]", status.StrB, ref culcText, ref showText);
            ReplaceBonus("[知力B]", status.IntB, ref culcText, ref showText);
            ReplaceBonus("[魔力B]", status.MagB, ref culcText, ref showText);
            ReplaceBonus("[器用B]", status.DexB, ref culcText, ref showText);
            ReplaceBonus("[敏捷B]", status.AgiB, ref culcText, ref showText);
            ReplaceBonus("[感知B]", status.SnsB, ref culcText, ref showText);
            ReplaceBonus("[魅力B]", status.AppB, ref culcText, ref showText);
            ReplaceBonus("[幸運B]", status.LukB, ref culcText, ref showText);
        }

        CalcDice(ref culcText, ref showText);

        if (status != null)
        {
            CalcBonusDice("生命R", status.VitB, ref culcText, ref showText);
            CalcBonusDice("精神R", status.PowB, ref culcText, ref showText);
            CalcBonusDice("筋力R", status.StrB, ref culcText, ref showText);
            CalcBonusDice("知力R", status.IntB, ref culcText, ref showText);
            CalcBonusDice("魔力R", status.MagB, ref culcText, ref showText);
            CalcBonusDice("器用R", status.DexB, ref culcText, ref showText);
            CalcBonusDice("敏捷R", status.AgiB, ref culcText, ref showText);
            CalcBonusDice("感知R", status.SnsB, ref culcText, ref showText);
            CalcBonusDice("魅力R", status.AppB, ref culcText, ref showText);
            CalcBonusDice("幸運R", status.LukB, ref culcText, ref showText);
        }

        var expr = new NCalc.Expression(culcText);

        showText = showText.Replace("*", @"\*");

        culcResult = float.Parse(expr.Evaluate()?.ToString() ?? "0").ToString("0.##");
        showResult = showText;
    }

    private void CalcDice(ref string culcText, ref string showText)
    {
        List<int> totals = new List<int>();

        showText = Regex.Replace(showText, @"(\d+)d(\d+)", match =>
        {
            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);

            int total = 0;
            string dices = "";

            for (int i = 0; i < count; i++)
            {
                if (i > 0) dices += ", ";

                int dice = _ms.Next(1, sides + 1);

                total += dice;
                dices += dice.ToString();
            }
            totals.Add(total);

            return $"{count}d{sides}({dices})";
        });

        int i = 0;
        culcText = Regex.Replace(culcText, @"(\d+)d(\d+)", match =>
        {
            var result = (totals[i]).ToString();

            i++;

            return result;
        });
    }

    private void CalcBonusDice(string text, float bonus, ref string culcText, ref string showText)
    {
        List<int> dices = new List<int>();

        string showBonus = bonus.ToString("0.##");

        showText = Regex.Replace(showText, text, match =>
        {
            int dice = _ms.Next(1, 101);
            dices.Add(dice);
            return $"1d100({dice})*" + showBonus;
        });

        int i = 0;
        culcText = Regex.Replace(culcText, text, match =>
        {
            var result = (bonus * dices[i]).ToString();

            i++;

            return result;
        });
    }

    private void ReplaceWeponPower(string text, string weaponPower, ref string culcText, ref string showText)
    {
        culcText = Regex.Replace(culcText, text, match =>
        {
            return weaponPower.Replace("[スキル値]", match.Groups[1].Value);
        });

        showText = Regex.Replace(culcText, text, match =>
        {
            return weaponPower.Replace("[スキル値]", match.Groups[1].Value);
        });
    }

    private void ReplaceBonus(string text, float bonus, ref string culcText, ref string showText)
    {
        var bonusText = bonus.ToString("0.##");

        culcText = culcText.Replace(text, bonusText);
        showText = showText.Replace(text, bonusText);
    }
}
