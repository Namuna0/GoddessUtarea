using Discord.WebSocket;
using System.Text.RegularExpressions;

partial class Program
{
    class CharacterStatus
    {
        public short MaxHp { get; set; }
        public short MaxSp { get; set; }
        public short MaxSan { get; set; }
        public short MaxMp { get; set; }
        public short Hp { get; set; }
        public short Sp { get; set; }
        public short San { get; set; }
        public short Mp { get; set; }

        public float VitB { get; set; }
        public float PowB { get; set; }
        public float StrB { get; set; }
        public float IntB { get; set; }
        public float MagB { get; set; }
        public float DexB { get; set; }
        public float AgiB { get; set; }
        public float SnsB { get; set; }
        public float AppB { get; set; }
        public float LukB { get; set; }

        public string WepP { get; set; } = "0";
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

        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            _currentCharaDic.Add(user.Id, texts[0]);
        }

        await message.Channel.SendMessageAsync($"こんにちは、{texts[0]}さん！");
    }

    private async Task SimpleRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
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

    private async Task DiceRoll(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?r ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"SELECT vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b FROM user_status WHERE id = @id",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                },
                async (reader) =>
                {
                    var status = new CharacterStatus();
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

                    CalcFormula(texts[0], status, out string culcResult, out string showResult);

                    string comment = string.Empty;
                    if (texts.Length > 1)
                    {
                        comment = $"：{texts[1]}";
                    }

                    await message.Channel.SendMessageAsync(
                    $"<@{user.Id}> :game_die:{currentChara}\r\n" +
                    $"{showResult}=>{culcResult}");
                },
                async () =>
                {
                    await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
                });
        });
    }

    private async Task SetRes(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set res ".Length);
        var texts = text.Split(" ");
        await Command(texts, 4, message, user, async (currentChara) =>
        {
            var status = new CharacterStatus();
            status.MaxHp = short.Parse(texts[0]);
            status.MaxSp = short.Parse(texts[1]);
            status.MaxSan = short.Parse(texts[2]);
            status.MaxMp = short.Parse(texts[3]);
            status.Hp = status.MaxHp;
            status.Sp = status.MaxSp;
            status.San = status.MaxSan;
            status.Mp = status.MaxMp;

            await ConnectDatabase(
                @"INSERT INTO user_status (id, max_hp, max_sp, max_san, max_mp, hp, sp, san, mp)" +
                @"VALUES (@id, @max_hp, @max_sp, @max_san, @max_mp, @hp, @sp, @san, @mp)" +
                @"ON CONFLICT (id) DO UPDATE SET max_hp = EXCLUDED.max_hp, max_sp = EXCLUDED.max_sp, max_san = EXCLUDED.max_san, max_mp = EXCLUDED.max_mp, hp = EXCLUDED.hp, sp = EXCLUDED.sp, san = EXCLUDED.san, mp = EXCLUDED.mp;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("max_hp", status.MaxHp);
                    parameters.AddWithValue("max_sp", status.MaxSp);
                    parameters.AddWithValue("max_san", status.MaxSan);
                    parameters.AddWithValue("max_mp", status.MaxMp);
                    parameters.AddWithValue("hp", status.Hp);
                    parameters.AddWithValue("sp", status.Sp);
                    parameters.AddWithValue("san", status.San);
                    parameters.AddWithValue("mp", status.Mp);
                });

            await ShowResource(currentChara, status, message);
        });
    }

    private async Task UpdateHp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?hp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO user_status (id, hp)" +
                @"VALUES (@id, @hp)" +
                @"ON CONFLICT (id) DO UPDATE SET hp = user_status.hp + EXCLUDED.hp;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("hp", short.Parse(texts[0]));
                });

            await ShowResource(currentChara, message);
        });
    }

    private async Task UpdateSp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?sp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO user_status (id, sp)" +
                @"VALUES (@id, @sp)" +
                @"ON CONFLICT (id) DO UPDATE SET sp = user_status.sp + EXCLUDED.sp;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("sp", short.Parse(texts[0]));
                });

            await ShowResource(currentChara, message);
        });
    }

    private async Task UpdateSan(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?san ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO user_status (id, san)" +
                @"VALUES (@id, @hp)" +
                @"ON CONFLICT (id) DO UPDATE SET san = user_status.san + EXCLUDED.san;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("san", short.Parse(texts[0]));
                });

            await ShowResource(currentChara, message);
        });
    }

    private async Task UpdateMp(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?mp ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO user_status (id, mp)" +
                @"VALUES (@id, @mp)" +
                @"ON CONFLICT (id) DO UPDATE SET mp = user_status.mp + EXCLUDED.mp;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("mp", short.Parse(texts[0]));
                });

            await ShowResource(currentChara, message);
        });
    }

    private async Task SetWep(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set wep ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO user_status (id, wep_p)" +
                @"VALUES (@id, @wep_p)" +
                @"ON CONFLICT (id) DO UPDATE SET wep_p = EXCLUDED.wep_p;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("wep_p", texts[0]);
                });

            await message.Channel.SendMessageAsync($"{currentChara}：武器威力「{texts[0].Replace("*", @"\*")}」を登録しました。");
        });
    }

    private async Task SetBon(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?set bon ".Length);
        var texts = text.Split(" ");
        await Command(texts, 10, message, user, async (currentChara) =>
        {
            var status = new CharacterStatus();
            status.VitB = float.Parse(texts[0]);
            status.PowB = float.Parse(texts[1]);
            status.StrB = float.Parse(texts[2]);
            status.IntB = float.Parse(texts[3]);
            status.MagB = float.Parse(texts[4]);
            status.DexB = float.Parse(texts[5]);
            status.AgiB = float.Parse(texts[6]);
            status.SnsB = float.Parse(texts[7]);
            status.AppB = float.Parse(texts[8]);
            status.LukB = float.Parse(texts[9]);

            await ConnectDatabase(
                @"INSERT INTO user_status (id, vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b)" +
                @"VALUES (@id, @vit_b, @pow_b, @str_b, @int_b, @mag_b, @dex_b, @agi_b, @sns_b, @app_b, @luk_b)" +
                @"ON CONFLICT (id) DO UPDATE SET vit_b = EXCLUDED.vit_b, pow_b = EXCLUDED.pow_b, str_b = EXCLUDED.str_b, int_b = EXCLUDED.int_b, mag_b = EXCLUDED.mag_b, dex_b = EXCLUDED.dex_b, agi_b = EXCLUDED.agi_b, sns_b = EXCLUDED.sns_b, app_b = EXCLUDED.app_b, luk_b = EXCLUDED.luk_b;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue("vit_b", status.VitB);
                    parameters.AddWithValue("pow_b", status.PowB);
                    parameters.AddWithValue("str_b", status.StrB);
                    parameters.AddWithValue("int_b", status.IntB);
                    parameters.AddWithValue("mag_b", status.MagB);
                    parameters.AddWithValue("dex_b", status.DexB);
                    parameters.AddWithValue("agi_b", status.AgiB);
                    parameters.AddWithValue("sns_b", status.SnsB);
                    parameters.AddWithValue("app_b", status.AppB);
                    parameters.AddWithValue("luk_b", status.LukB);
                });

            await ShowBonus(currentChara, status, message);
        });
    }

    private async Task ShowResource(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
            @"SELECT max_hp, max_sp, max_san, max_mp, hp, sp, san, mp FROM user_status WHERE id = @id",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
            },
            async (reader) =>
            {
                await message.Channel.SendMessageAsync(
                    $"{currentChara}\r\n" +
                    "●リソース\r\n" +
                    $"【HP】{reader.GetInt16(4)}/{reader.GetInt16(0)}【SP】{reader.GetInt16(5)}/{reader.GetInt16(1)}\r\n" +
                    $"【SAN】{reader.GetInt16(6)}/{reader.GetInt16(2)}【MP】{reader.GetInt16(7)}/{reader.GetInt16(3)}\r\n" +
                    $"【防御点】\r\n" +
                    "●状態\r\n" +
                    "●永続状態");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
            });
    }

    private async Task ShowResource(string currentChara, CharacterStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
                $"{currentChara}\r\n" +
                "●リソース\r\n" +
                $"【HP】{status.Hp}/{status.MaxHp}【SP】{status.Sp}/{status.MaxSp}\r\n" +
                $"【SAN】{status.San}/{status.MaxSan}【MP】{status.Mp}/{status.MaxMp}\r\n" +
                $"【防御点】\r\n" +
                "●状態\r\n" +
                "●永続状態");
    }

    private async Task ShowBonus(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
            @"SELECT vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b FROM user_status WHERE id = @id",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
            },
            async (reader) =>
            {
                await message.Channel.SendMessageAsync(
                    $"{currentChara}\r\n" +
                    "●能力値B\r\n" +
                    $"{reader.GetFloat(0).ToString("0.##")} 生命, {reader.GetFloat(1).ToString("0.##")} 精神, {reader.GetFloat(2).ToString("0.##")} 筋力, {reader.GetFloat(3).ToString("0.##")} 知力, {reader.GetFloat(4).ToString("0.##")} 魔力\r\n" +
                    $"{reader.GetFloat(5).ToString("0.##")} 器用, {reader.GetFloat(6).ToString("0.##")} 敏捷, {reader.GetFloat(7).ToString("0.##")} 感知, {reader.GetFloat(8).ToString("0.##")} 魅力, {reader.GetFloat(9).ToString("0.##")} 幸運");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
            });
    }

    private async Task ShowBonus(string currentChara, CharacterStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{currentChara}\r\n" +
            "●能力値B\r\n" +
            $"{status.VitB.ToString("0.##")} 生命, {status.PowB.ToString("0.##")} 精神, {status.StrB.ToString("0.##")} 筋力, {status.IntB.ToString("0.##")} 知力, {status.MagB.ToString("0.##")} 魔力\r\n" +
            $"{status.DexB.ToString("0.##")} 器用, {status.AgiB.ToString("0.##")} 敏捷, {status.SnsB.ToString("0.##")} 感知, {status.AppB.ToString("0.##")} 魅力, {status.LukB.ToString("0.##")} 幸運");
        ;
    }

    public async Task Command(string[] texts, int length, SocketMessage message, SocketGuildUser user, Func<string, Task> onCompleted)
    {
        if (texts.Length < length)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await onCompleted.Invoke(currentChara);
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

//var createSql = @"CREATE TABLE IF NOT EXISTS user_status (
//id TEXT PRIMARY KEY,
//max_hp SMALLINT DEFAULT 0,
//max_sp  SMALLINT DEFAULT 0,
//max_san SMALLINT DEFAULT 0,
//max_mp SMALLINT DEFAULT 0,
//hp SMALLINT DEFAULT 0,
//sp  SMALLINT DEFAULT 0,
//san SMALLINT DEFAULT 0,
//mp SMALLINT DEFAULT 0,
//vit_b REAL DEFAULT 0.0,
//pow_b REAL DEFAULT 0.0,
//str_b REAL DEFAULT 0.0,
//int_b REAL DEFAULT 0.0,
//mag_b REAL DEFAULT 0.0,
//dex_b REAL DEFAULT 0.0,
//agi_b REAL DEFAULT 0.0,
//sns_b REAL DEFAULT 0.0,
//app_b REAL DEFAULT 0.0,
//luk_b REAL DEFAULT 0.0,
//wep_p TEXT DEFAULT '0',
//created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP);";