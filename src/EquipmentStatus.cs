using Discord.WebSocket;
using System.Text.RegularExpressions;

partial class Program
{
    class EquipmentStatus
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

    private async Task ShowRes(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await DisplayResource(currentChara, message);
    }

    private async Task SetRes(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set res ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1111, message, user, async (currentChara) =>
        {
            var status = new EquipmentStatus();
            status.MaxHp = short.Parse(texts[0]);
            status.MaxSp = short.Parse(texts[1]);
            status.MaxSan = short.Parse(texts[2]);
            status.MaxMp = short.Parse(texts[3]);
            status.Hp = status.MaxHp;
            status.Sp = status.MaxSp;
            status.San = status.MaxSan;
            status.Mp = status.MaxMp;

            await ConnectDatabase(
                @"INSERT INTO character_equipment (id, max_hp, max_sp, max_san, max_mp, hp, sp, san, mp)" +
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

            await DisplayResource(currentChara, status, message);
        });
    }

    private async Task UpdateRes(string res, SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?{res} ".Length);
        var texts = text.Split(" ");
        await Command(texts, 1, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @$"INSERT INTO character_equipment (id, {res})" +
                @$"VALUES (@id, @{res})" +
                @$"ON CONFLICT (id) DO UPDATE SET {res} = character_equipment.{res} + EXCLUDED.{res};",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                    parameters.AddWithValue($"{res}", short.Parse(texts[0]));
                });

            await DisplayResource(currentChara, message);
        });
    }

    private async Task ShowBon(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await DisplayBonus(currentChara, message);
    }

    private async Task SetBon(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set bon ".Length);
        var texts = text.Split(" ");
        await Command(texts, 2222222222, message, user, async (currentChara) =>
        {
            var status = new EquipmentStatus();
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
                @"INSERT INTO character_equipment (id, vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b)" +
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

            await DisplayBonus(currentChara, status, message);
        });
    }

    private async Task SetWep(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set wep ".Length);
        var texts = text.Split(" ");
        await Command(texts, 3, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @"INSERT INTO character_equipment (id, wep_p)" +
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

    private async Task ShowMasterRes(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?show master res ".Length);
        var texts = text.Split(" ");

        if (texts.Length < 1)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        await DisplayResource(texts[0], message);
    }

    private async Task UpdateMasterRes(string res, SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?master {res} ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            await ConnectDatabase(
                @$"INSERT INTO character_equipment (id, {res})" +
                @$"VALUES (@id, @{res})" +
                @$"ON CONFLICT (id) DO UPDATE SET {res} = character_equipment.{res} + EXCLUDED.{res};",
                parameters =>
                {
                    parameters.AddWithValue("id", texts[0]);
                    parameters.AddWithValue($"{res}", short.Parse(texts[1]));
                });

            await DisplayResource(texts[0], message);
        });
    }

    private async Task DisplayResource(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
@"WITH upsert AS (
    INSERT INTO character_equipment (id, max_hp, max_sp, max_san, max_mp, hp, sp, san, mp)
    VALUES (@id, 0, 0, 0, 0, 0, 0, 0, 0)
    ON CONFLICT (id) DO NOTHING
)
SELECT max_hp, max_sp, max_san, max_mp, hp, sp, san, mp
FROM character_equipment
WHERE id = @id;",
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
                    "●状態\r\n" +
                    "●永続状態");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync(
                    $"{currentChara}\r\n" +
                    "●リソース\r\n" +
                    $"【HP】0/0【SP】0/0\r\n" +
                    $"【SAN】0/0【MP】0/0\r\n" +
                    "●状態\r\n" +
                    "●永続状態");
            });
    }

    private async Task DisplayResource(string currentChara, EquipmentStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
                $"{currentChara}\r\n" +
                "●リソース\r\n" +
                $"【HP】{status.Hp}/{status.MaxHp}【SP】{status.Sp}/{status.MaxSp}\r\n" +
                $"【SAN】{status.San}/{status.MaxSan}【MP】{status.Mp}/{status.MaxMp}\r\n" +
                "●状態\r\n" +
                "●永続状態");
    }

    private async Task DisplayBonus(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
@"WITH upsert AS (
  INSERT INTO character_equipment (id, vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b)
  VALUES (@id, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0)
  ON CONFLICT (id) DO NOTHING
)
SELECT vit_b, pow_b, str_b, int_b, mag_b, dex_b, agi_b, sns_b, app_b, luk_b
FROM character_equipment
WHERE id = @id;",
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
                await message.Channel.SendMessageAsync(
                    $"{currentChara}\r\n" +
                    "●能力値B\r\n" +
                    $"1 生命, 1 精神, 1 筋力, 1 知力, 1 魔力\r\n" +
                    $"1 器用, 1 敏捷, 1 感知, 1 魅力, 1 幸運");
            });
    }

    private async Task DisplayBonus(string currentChara, EquipmentStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{currentChara}\r\n" +
            "●能力値B\r\n" +
            $"{status.VitB.ToString("0.##")} 生命, {status.PowB.ToString("0.##")} 精神, {status.StrB.ToString("0.##")} 筋力, {status.IntB.ToString("0.##")} 知力, {status.MagB.ToString("0.##")} 魔力\r\n" +
            $"{status.DexB.ToString("0.##")} 器用, {status.AgiB.ToString("0.##")} 敏捷, {status.SnsB.ToString("0.##")} 感知, {status.AppB.ToString("0.##")} 魅力, {status.LukB.ToString("0.##")} 幸運");
        ;
    }
}
