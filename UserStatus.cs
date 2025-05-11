using Discord.WebSocket;
using System.Text.RegularExpressions;

partial class Program
{
    class CharacterStatus
    {
        public int MaxHp { get; set; }
        public int MaxSp { get; set; }
        public int MaxSan { get; set; }
        public int MaxMp { get; set; }
        public int Hp { get; set; }
        public int Sp { get; set; }
        public int San { get; set; }
        public int Mp { get; set; }

        public string VitB { get; set; } = "0";
        public string PowB { get; set; } = "0";
        public string StrB { get; set; } = "0";
        public string IntB { get; set; } = "0";
        public string MagB { get; set; } = "0";
        public string DexB { get; set; } = "0";
        public string AgiB { get; set; } = "0";
        public string SnsB { get; set; } = "0";
        public string AppB { get; set; } = "0";
        public string LukB { get; set; } = "0";

        public string WepP { get; set; } = "0";

        public void SetResource(int hp, int sp, int san, int mp)
        {
            MaxHp = hp;
            MaxSp = sp;
            MaxSan = san;
            MaxMp = mp;
            Hp = hp;
            Sp = sp;
            San = san;
            Mp = mp;
        }

        public void SetBonus(string vitB, string powB, string strB, string intB, string magB, string dexB, string agiB, string snsB, string appB, string lukB)
        {
            VitB = vitB;
            PowB = powB;
            StrB = strB;
            IntB = intB;
            MagB = magB;
            DexB = dexB;
            AgiB = agiB;
            SnsB = snsB;
            AppB = appB;
            LukB = lukB;
        }

        public void SetWeaponPower(string weponPower)
        {
            WepP = weponPower;
        }
    }

    class UserStatus
    {
        public string CurrentCharacter { get; set; } = string.Empty;

        public Dictionary<string, CharacterStatus> CharStatusDic { get; set; } = new Dictionary<string, CharacterStatus>();
    }

    private async Task Login(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?login ".Length);
        var texts = text.Split(" ");

        if (texts.Length < 1)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        UserStatus userStatus;
        if (!_userStatusDic.TryGetValue(user.Id, out userStatus))
        {
            _userStatusDic.Add(user.Id, new UserStatus());
        }

        CharacterStatus characterStatus;
        if (!userStatus.CharStatusDic.TryGetValue(texts[0], out characterStatus))
        {
            userStatus.CharStatusDic[texts[0]] = new CharacterStatus();
        }

        userStatus.CurrentCharacter = texts[0];
        await message.Channel.SendMessageAsync($"こんにちは、{texts[0]}さん！");
    }

    private async Task SetBon(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set bon ".Length);
        var texts = text.Split(" ");
        await Command(texts, 10, message, user, async (userStatus, charStatus) =>
        {
            charStatus.SetBonus(
                texts[0], texts[1], texts[2], texts[3], texts[4],
                texts[5], texts[6], texts[7], texts[8], texts[9]);
            await ShowBonus(userStatus, charStatus, message);
        });
    }

    private async Task DiceRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?r ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            CalcFormula(texts[0], charStatus, out string culcResult, out string showResult);

            string comment = string.Empty;
            if (texts.Length > 1)
            {
                comment = texts[1];
            }

            await message.Channel.SendMessageAsync(
            $"<@{user.Id}> {userStatus.CurrentCharacter} :game_die:{comment}\r\n" +
            $"{showResult}=>{culcResult}");
        });
    }

    private async Task SetRes(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set res ".Length);
        var texts = text.Split(" ");
        await Command(texts, 4, message, user, async (userStatus, charStatus) =>
        {
            charStatus.SetResource(int.Parse(texts[0]), int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]));
            await ShowResource(userStatus, charStatus, message);
        });
    }

    private async Task UpdateHp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?hp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            charStatus.Hp += int.Parse(texts[0]);
            await ShowResource(userStatus, charStatus, message);
        });
    }

    private async Task UpdateSp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?sp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            charStatus.Sp += int.Parse(texts[0]);
            await ShowResource(userStatus, charStatus, message);
        });
    }

    private async Task UpdateSan(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?san ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            charStatus.San += int.Parse(texts[0]);
            await ShowResource(userStatus, charStatus, message);
        });
    }

    private async Task UpdateMp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?mp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            charStatus.Mp += int.Parse(texts[0]);
            await ShowResource(userStatus, charStatus, message);
        });
    }

    private async Task SetWep(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set wep ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (userStatus, charStatus) =>
        {
            charStatus.SetWeaponPower(texts[0]);
            await message.Channel.SendMessageAsync($"武器威力「{texts[1].Replace("*", @"\*")}」を登録しました！");
        });
    }

    private async Task Command(string[] texts, int length, SocketMessage message, SocketGuildUser user, Func<UserStatus, CharacterStatus, Task> onCompleted)
    {
        if (texts.Length < length)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        UserStatus userStatus;
        if (!_userStatusDic.TryGetValue(user.Id, out userStatus))
        {
            _userStatusDic.Add(user.Id, new UserStatus());
        }

        CharacterStatus characterStatus;
        if (!userStatus.CharStatusDic.TryGetValue(userStatus.CurrentCharacter, out characterStatus))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await onCompleted.Invoke(userStatus, characterStatus);
    }

    private async Task ShowBonus(UserStatus userStatus, CharacterStatus charStatus, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{userStatus.CurrentCharacter}\r\n" +
            "●能力値B\r\n" +
            $"{charStatus.VitB} 生命, {charStatus.PowB} 精神, {charStatus.StrB} 筋力, {charStatus.IntB} 知力, {charStatus.MagB} 魔力\r\n" +
            $"{charStatus.DexB} 器用, {charStatus.AgiB} 敏捷, {charStatus.SnsB} 感知, {charStatus.AppB} 魅力, {charStatus.LukB} 幸運");
    }

    private async Task ShowResource(UserStatus userStatus, CharacterStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{userStatus.CurrentCharacter}\r\n" +
            "●リソース\r\n" +
            $"【HP】{status.Hp}/{status.MaxHp}【SP】{status.Sp}/{status.MaxSp}\r\n" +
            $"【SAN】{status.San}/{status.MaxSan}【MP】{status.Mp}/{status.MaxMp}\r\n" +
            $"【防御点】\r\n" +
            "●状態\r\n" +
            "●永続状態");
    }

    private void CalcFormula(string originalText, CharacterStatus status, out string culcResult, out string showResult)
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

    private void CalcBonusDice(string text, string bonusText, ref string culcText, ref string showText)
    {
        List<int> dices = new List<int>();

        float culcBonus = float.Parse(bonusText);
        string showBonus = culcBonus.ToString("0.##");

        showText = Regex.Replace(showText, text, match =>
        {
            int dice = _ms.Next(1, 101);
            dices.Add(dice);
            return $"1d100({dice})*" + showBonus;
        });

        int i = 0;
        culcText = Regex.Replace(culcText, text, match =>
        {
            var result = (culcBonus * dices[i]).ToString();

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

    private void ReplaceBonus(string text, string bonusText, ref string culcText, ref string showText)
    {
        culcText = culcText.Replace(text, bonusText);
        showText = showText.Replace(text, bonusText);
    }
}