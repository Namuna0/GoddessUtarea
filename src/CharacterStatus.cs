using Discord.WebSocket;
using System.Text;

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

    private async Task ShowSta(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await DisplayStatus(currentChara, message);
    }

    private async Task SetSta(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?set sta ".Length);
        var texts = text.Split(" ");

        await Command(texts, 3333, message, user, async (currentChara) =>
        {
            var race = texts[0];
            var gender = texts[1];
            var life_path = texts[2];
            var life_path2 = texts[3];

            await ConnectDatabase($@"
INSERT INTO character_status (id, race, gender, life_path)
VALUES (@id, @race, @gender, @life_path)
ON CONFLICT (id) DO UPDATE
SET race = EXCLUDED.race,
gender = EXCLUDED.gender,
life_path = EXCLUDED.life_path;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue($"race", race);
                parameters.AddWithValue($"gender", gender);
                parameters.AddWithValue($"life_path", $"{life_path}, {life_path2}");
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task SetAbi(string abi, SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?set {abi} ".Length);
        var texts = text.Split(" ");

        await Command(texts, 21111, message, user, async (currentChara) =>
        {
            int baseK = short.Parse(texts[0]);
            int growth = short.Parse(texts[1]);
            int life = short.Parse(texts[2]);
            int classK = short.Parse(texts[3]);
            float race = float.Parse(texts[4]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, {abi}_base, {abi}_growth, {abi}_life, {abi}_class, {abi}_race)
VALUES (@id, @{abi}_base, @{abi}_growth, @{abi}_life, @{abi}_class, @{abi}_race)
ON CONFLICT (id) DO UPDATE
SET {abi}_base = EXCLUDED.{abi}_base,
{abi}_growth = EXCLUDED.{abi}_growth,
{abi}_life = EXCLUDED.{abi}_life,
{abi}_class = EXCLUDED.{abi}_class,
{abi}_race = EXCLUDED.{abi}_race;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue($"{abi}_base", baseK);
                parameters.AddWithValue($"{abi}_growth", growth);
                parameters.AddWithValue($"{abi}_life", life);
                parameters.AddWithValue($"{abi}_class", classK);
                parameters.AddWithValue($"{abi}_race", race);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task SetEle(string ele, SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?set {ele} ".Length);
        var texts = text.Split(" ");

        await Command(texts, 211, message, user, async (currentChara) =>
        {
            int baseK = short.Parse(texts[0]);
            int title = short.Parse(texts[1]);
            float race = float.Parse(texts[2]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, {ele}_base, {ele}_title, {ele}_race)
VALUES (@id, @{ele}_base, @{ele}_title, @{ele}_race)
ON CONFLICT (id) DO UPDATE
SET {ele}_base = EXCLUDED.{ele}_base,
{ele}_title = EXCLUDED.{ele}_title,
{ele}_race = EXCLUDED.{ele}_race;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue($"{ele}_base", baseK);
                parameters.AddWithValue($"{ele}_title", title);
                parameters.AddWithValue($"{ele}_race", race);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task UpdateLevel(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?level ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            int level = short.Parse(texts[0]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, level)
VALUES (@id, @level)
ON CONFLICT (id) DO UPDATE
SET level = character_status.level + EXCLUDED.level;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("level", level);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task UpdateExp(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?exp ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            int level = short.Parse(texts[0]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, exp)
VALUES (@id, @exp)
ON CONFLICT (id) DO UPDATE
SET exp = character_status.exp + EXCLUDED.exp;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("exp", level);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task UpdateDet(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?det ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            int level = short.Parse(texts[0]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, det)
VALUES (@id, @det)
ON CONFLICT (id) DO UPDATE
SET det = character_status.det + EXCLUDED.det;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("det", level);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task UpdateExpDet(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?e&d ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            int add = short.Parse(texts[0]);

            await ConnectDatabase($@"
INSERT INTO character_status (id, exp, det)
VALUES (@id, @exp, @det)
ON CONFLICT (id) DO UPDATE
SET exp = character_status.exp + EXCLUDED.exp,
det = character_status.det + EXCLUDED.det;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("exp", add);
                parameters.AddWithValue("det", add);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task SetResRate(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?set resrate ".Length);
        var texts = text.Split(" ");

        await Command(texts, 2222, message, user, async (currentChara) =>
        {
            float hp_rate = float.Parse(texts[0]);
            float sp_rate = float.Parse(texts[1]);
            float san_rate = float.Parse(texts[2]);
            float mp_rate = float.Parse(texts[3]);

            await ConnectDatabase(@"
INSERT INTO character_status (id, hp_rate, sp_rate, san_rate, mp_rate)
VALUES (@id, @hp_rate, @sp_rate, @san_rate, @mp_rate)
ON CONFLICT (id) DO UPDATE
SET hp_rate = EXCLUDED.hp_rate,
sp_rate = EXCLUDED.sp_rate,
san_rate = EXCLUDED.san_rate,
mp_rate = EXCLUDED.mp_rate;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue($"hp_rate", hp_rate);
                parameters.AddWithValue($"sp_rate", sp_rate);
                parameters.AddWithValue($"san_rate", san_rate);
                parameters.AddWithValue($"mp_rate", mp_rate);
            });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task AddSta(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?add sta ".Length);
        var texts = text.Split(" ");
        await Command(texts, 33, message, user, async (currentChara) =>
        {
            string listName = string.Empty;
            if (texts[0] == "習得称号") listName = "title_list";
            else if (texts[0] == "習得クラス") listName = "class_list";
            else if (texts[0] == "設定クラス") listName = "applied_class_list";
            else if (texts[0] == "習得特性") listName = "trait_list";
            else if (texts[0] == "習得スキル") listName = "skill_list";
            else
            {
                await message.Channel.SendMessageAsync("存在しないリストです。");
                return;
            }

            await ConnectDatabase(@$"
INSERT INTO character_status (id, race, gender, life_path,
    vit_base, vit_growth, vit_life, vit_class, vit_race,
    pow_base, pow_growth, pow_life, pow_class, pow_race,
    str_base, str_growth, str_life, str_class, str_race,
    int_base, int_growth, int_life, int_class, int_race,
    mag_base, mag_growth, mag_life, mag_class, mag_race,
    dex_base, dex_growth, dex_life, dex_class, dex_race,
    agi_base, agi_growth, agi_life, agi_class, agi_race,
    sns_base, sns_growth, sns_life, sns_class, sns_race,
    app_base, app_growth, app_life, app_class, app_race,
    luk_base, luk_growth, luk_life, luk_class, luk_race,
    fire_base, fire_title, fire_race,
    water_base, water_title, water_race,
    wind_base, wind_title, wind_race,
    electric_base, electric_title, electric_race,
    cold_base, cold_title, cold_race,
    soil_base, soil_title, soil_race,
    light_base, light_title, light_race,
    dark_base, dark_title, dark_race,
    level, exp, det,
    hp_rate, sp_rate, san_rate, mp_rate,
    title_list,
    class_list,
    applied_class_list,
    trait_list,
    skill_list)
    VALUES (@id, '', '', '',
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    1, 0, 0,
    1.0, 1.0, 1.0, 1.0,
    to_jsonb(ARRAY[@item_id]),
    '[]'::jsonb,
    '[]'::jsonb,
    '[]'::jsonb,
    '[]'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET {listName} = (
    SELECT jsonb_agg(DISTINCT elem)
    FROM jsonb_array_elements(character_status.{listName} || to_jsonb(@item_id)) as elem
);",
                parameters =>
                {
                    parameters.AddWithValue("item_id", texts[1]);
                    parameters.AddWithValue("id", currentChara);
                });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task RemoveSta(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?remove sta ".Length);
        var texts = text.Split(" ");
        await Command(texts, 33, message, user, async (currentChara) =>
        {
            string listName = string.Empty;
            if (texts[0] == "習得称号") listName = "title_list";
            else if (texts[0] == "習得クラス") listName = "class_list";
            else if (texts[0] == "設定クラス") listName = "applied_class_list";
            else if (texts[0] == "習得特性") listName = "trait_list";
            else if (texts[0] == "習得スキル") listName = "skill_list";
            else
            {
                await message.Channel.SendMessageAsync("存在しないリストです。");
                return;
            }

            await ConnectDatabase(@$"
INSERT INTO character_status (id, race, gender, life_path,
    vit_base, vit_growth, vit_life, vit_class, vit_race,
    pow_base, pow_growth, pow_life, pow_class, pow_race,
    str_base, str_growth, str_life, str_class, str_race,
    int_base, int_growth, int_life, int_class, int_race,
    mag_base, mag_growth, mag_life, mag_class, mag_race,
    dex_base, dex_growth, dex_life, dex_class, dex_race,
    agi_base, agi_growth, agi_life, agi_class, agi_race,
    sns_base, sns_growth, sns_life, sns_class, sns_race,
    app_base, app_growth, app_life, app_class, app_race,
    luk_base, luk_growth, luk_life, luk_class, luk_race,
    fire_base, fire_title, fire_race,
    water_base, water_title, water_race,
    wind_base, wind_title, wind_race,
    electric_base, electric_title, electric_race,
    cold_base, cold_title, cold_race,
    soil_base, soil_title, soil_race,
    light_base, light_title, light_race,
    dark_base, dark_title, dark_race,
    level, exp, det,
    hp_rate, sp_rate, san_rate, mp_rate,
    title_list,
    class_list,
    applied_class_list,
    trait_list,
    skill_list)
    VALUES (@id, '', '', '',
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    0, 0, 1.0,
    1, 0, 0,
    1.0, 1.0, 1.0, 1.0,
    to_jsonb(ARRAY[@item_id]),
    '[]'::jsonb,
    '[]'::jsonb,
    '[]'::jsonb,
    '[]'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET {listName} = (
    SELECT COALESCE(jsonb_agg(elem), '[]'::jsonb)
    FROM jsonb_array_elements(character_status.{listName}) AS elem
    WHERE elem <> to_jsonb(@item_id)
);",
                parameters =>
                {
                    parameters.AddWithValue("item_id", texts[1]);
                    parameters.AddWithValue("id", currentChara);
                });

            await DisplayStatus(currentChara, message);
        });
    }

    private async Task DisplayStatus(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
@"WITH upsert AS (
    INSERT INTO character_status (
        id, race, gender, life_path,
        vit_base, vit_growth, vit_life, vit_class, vit_race,
        pow_base, pow_growth, pow_life, pow_class, pow_race,
        str_base, str_growth, str_life, str_class, str_race,
        int_base, int_growth, int_life, int_class, int_race,
        mag_base, mag_growth, mag_life, mag_class, mag_race,
        dex_base, dex_growth, dex_life, dex_class, dex_race,
        agi_base, agi_growth, agi_life, agi_class, agi_race,
        sns_base, sns_growth, sns_life, sns_class, sns_race,
        app_base, app_growth, app_life, app_class, app_race,
        luk_base, luk_growth, luk_life, luk_class, luk_race,
        fire_base, fire_title, fire_race,
        water_base, water_title, water_race,
        wind_base, wind_title, wind_race,
        electric_base, electric_title, electric_race,
        cold_base, cold_title, cold_race,
        soil_base, soil_title, soil_race,
        light_base, light_title, light_race,
        dark_base, dark_title, dark_race,
        level, exp, det,
        hp_rate, sp_rate, san_rate, mp_rate,
        title_list,
        class_list,
        applied_class_list,
        trait_list,
        skill_list)
        VALUES (@id, '', '', '',
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        0, 0, 1.0,
        1, 0, 0,
        1.0, 1.0, 1.0, 1.0,
        '[]'::jsonb,
        '[]'::jsonb,
        '[]'::jsonb,
        '[]'::jsonb,
        '[]'::jsonb
    )
    ON CONFLICT (id) DO NOTHING
)
SELECT race, gender, life_path,
vit_base, vit_growth, vit_life, vit_class, vit_race,
pow_base, pow_growth, pow_life, pow_class, pow_race,
str_base, str_growth, str_life, str_class, str_race,
int_base, int_growth, int_life, int_class, int_race,
mag_base, mag_growth, mag_life, mag_class, mag_race,
dex_base, dex_growth, dex_life, dex_class, dex_race,
agi_base, agi_growth, agi_life, agi_class, agi_race,
sns_base, sns_growth, sns_life, sns_class, sns_race,
app_base, app_growth, app_life, app_class, app_race,
luk_base, luk_growth, luk_life, luk_class, luk_race,
fire_base, fire_title, fire_race,
water_base, water_title, water_race,
wind_base, wind_title, wind_race,
electric_base, electric_title, electric_race,
cold_base, cold_title, cold_race,
soil_base, soil_title, soil_race,
light_base, light_title, light_race,
dark_base, dark_title, dark_race,
level, exp, det,
hp_rate, sp_rate, san_rate, mp_rate,
title_list,
class_list,
applied_class_list,
trait_list,
skill_list
FROM character_status
WHERE id = @id;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
            },
            async (reader) =>
            {
                var race = reader.GetString(reader.GetOrdinal("race"));
                var gender = reader.GetString(reader.GetOrdinal("gender"));
                var life_path = reader.GetString(reader.GetOrdinal("life_path"));
                var vit_base = reader.GetInt16(reader.GetOrdinal("vit_base"));
                var vit_growth = reader.GetInt16(reader.GetOrdinal("vit_growth"));
                var vit_life = reader.GetInt16(reader.GetOrdinal("vit_life"));
                var vit_class = reader.GetInt16(reader.GetOrdinal("vit_class"));
                var vit_race = reader.GetFloat(reader.GetOrdinal("vit_race"));
                var pow_base = reader.GetInt16(reader.GetOrdinal("pow_base"));
                var pow_growth = reader.GetInt16(reader.GetOrdinal("pow_growth"));
                var pow_life = reader.GetInt16(reader.GetOrdinal("pow_life"));
                var pow_class = reader.GetInt16(reader.GetOrdinal("pow_class"));
                var pow_race = reader.GetFloat(reader.GetOrdinal("pow_race"));
                var str_base = reader.GetInt16(reader.GetOrdinal("str_base"));
                var str_growth = reader.GetInt16(reader.GetOrdinal("str_growth"));
                var str_life = reader.GetInt16(reader.GetOrdinal("str_life"));
                var str_class = reader.GetInt16(reader.GetOrdinal("str_class"));
                var str_race = reader.GetFloat(reader.GetOrdinal("str_race"));
                var int_base = reader.GetInt16(reader.GetOrdinal("int_base"));
                var int_growth = reader.GetInt16(reader.GetOrdinal("int_growth"));
                var int_life = reader.GetInt16(reader.GetOrdinal("int_life"));
                var int_class = reader.GetInt16(reader.GetOrdinal("int_class"));
                var int_race = reader.GetFloat(reader.GetOrdinal("int_race"));
                var mag_base = reader.GetInt16(reader.GetOrdinal("mag_base"));
                var mag_growth = reader.GetInt16(reader.GetOrdinal("mag_growth"));
                var mag_life = reader.GetInt16(reader.GetOrdinal("mag_life"));
                var mag_class = reader.GetInt16(reader.GetOrdinal("mag_class"));
                var mag_race = reader.GetFloat(reader.GetOrdinal("mag_race"));
                var dex_base = reader.GetInt16(reader.GetOrdinal("dex_base"));
                var dex_growth = reader.GetInt16(reader.GetOrdinal("dex_growth"));
                var dex_life = reader.GetInt16(reader.GetOrdinal("dex_life"));
                var dex_class = reader.GetInt16(reader.GetOrdinal("dex_class"));
                var dex_race = reader.GetFloat(reader.GetOrdinal("dex_race"));
                var agi_base = reader.GetInt16(reader.GetOrdinal("agi_base"));
                var agi_growth = reader.GetInt16(reader.GetOrdinal("agi_growth"));
                var agi_life = reader.GetInt16(reader.GetOrdinal("agi_life"));
                var agi_class = reader.GetInt16(reader.GetOrdinal("agi_class"));
                var agi_race = reader.GetFloat(reader.GetOrdinal("agi_race"));
                var sns_base = reader.GetInt16(reader.GetOrdinal("sns_base"));
                var sns_growth = reader.GetInt16(reader.GetOrdinal("sns_growth"));
                var sns_life = reader.GetInt16(reader.GetOrdinal("sns_life"));
                var sns_class = reader.GetInt16(reader.GetOrdinal("sns_class"));
                var sns_race = reader.GetFloat(reader.GetOrdinal("sns_race"));
                var app_base = reader.GetInt16(reader.GetOrdinal("app_base"));
                var app_growth = reader.GetInt16(reader.GetOrdinal("app_growth"));
                var app_life = reader.GetInt16(reader.GetOrdinal("app_life"));
                var app_class = reader.GetInt16(reader.GetOrdinal("app_class"));
                var app_race = reader.GetFloat(reader.GetOrdinal("app_race"));
                var luk_base = reader.GetInt16(reader.GetOrdinal("luk_base"));
                var luk_growth = reader.GetInt16(reader.GetOrdinal("luk_growth"));
                var luk_life = reader.GetInt16(reader.GetOrdinal("luk_life"));
                var luk_class = reader.GetInt16(reader.GetOrdinal("luk_class"));
                var luk_race = reader.GetFloat(reader.GetOrdinal("luk_race"));

                var fire_base = reader.GetInt16(reader.GetOrdinal("fire_base"));
                var fire_title = reader.GetInt16(reader.GetOrdinal("fire_title"));
                var fire_race = reader.GetFloat(reader.GetOrdinal("fire_race"));
                var water_base = reader.GetInt16(reader.GetOrdinal("water_base"));
                var water_title = reader.GetInt16(reader.GetOrdinal("water_title"));
                var water_race = reader.GetFloat(reader.GetOrdinal("water_race"));
                var wind_base = reader.GetInt16(reader.GetOrdinal("wind_base"));
                var wind_title = reader.GetInt16(reader.GetOrdinal("wind_title"));
                var wind_race = reader.GetFloat(reader.GetOrdinal("wind_race"));
                var electric_base = reader.GetInt16(reader.GetOrdinal("electric_base"));
                var electric_title = reader.GetInt16(reader.GetOrdinal("electric_title"));
                var electric_race = reader.GetFloat(reader.GetOrdinal("electric_race"));
                var cold_base = reader.GetInt16(reader.GetOrdinal("cold_base"));
                var cold_title = reader.GetInt16(reader.GetOrdinal("cold_title"));
                var cold_race = reader.GetFloat(reader.GetOrdinal("cold_race"));
                var soil_base = reader.GetInt16(reader.GetOrdinal("soil_base"));
                var soil_title = reader.GetInt16(reader.GetOrdinal("soil_title"));
                var soil_race = reader.GetFloat(reader.GetOrdinal("soil_race"));
                var light_base = reader.GetInt16(reader.GetOrdinal("light_base"));
                var light_title = reader.GetInt16(reader.GetOrdinal("light_title"));
                var light_race = reader.GetFloat(reader.GetOrdinal("light_race"));
                var dark_base = reader.GetInt16(reader.GetOrdinal("dark_base"));
                var dark_title = reader.GetInt16(reader.GetOrdinal("dark_title"));
                var dark_race = reader.GetFloat(reader.GetOrdinal("dark_race"));

                var level = reader.GetInt16(reader.GetOrdinal("level"));
                var exp = reader.GetInt16(reader.GetOrdinal("exp"));
                var det = reader.GetInt16(reader.GetOrdinal("det"));

                var hp_rate = reader.GetFloat(reader.GetOrdinal("hp_rate"));
                var sp_rate = reader.GetFloat(reader.GetOrdinal("sp_rate"));
                var san_rate = reader.GetFloat(reader.GetOrdinal("san_rate"));
                var mp_rate = reader.GetFloat(reader.GetOrdinal("mp_rate"));

                var dict = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetFieldValue<string>(reader.GetOrdinal("title_list")));
                var title_list = string.Join(", ", dict);
                if (title_list.Length > 0) title_list += "\r\n";

                dict = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetFieldValue<string>(reader.GetOrdinal("class_list")));
                var class_list = string.Join(", ", dict);
                if (class_list.Length > 0) class_list += "\r\n";

                dict = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetFieldValue<string>(reader.GetOrdinal("applied_class_list")));
                var applied_class_list = string.Join(", ", dict);
                if (applied_class_list.Length > 0) applied_class_list += "\r\n";

                dict = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetFieldValue<string>(reader.GetOrdinal("trait_list")));
                var trait_list = string.Join(", ", dict);
                if (trait_list.Length > 0) trait_list += "\r\n";

                dict = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetFieldValue<string>(reader.GetOrdinal("skill_list")));
                var skill_list = string.Join(", ", dict);
                if (skill_list.Length > 0) skill_list += "\r\n";

                int vit = (int)(vit_race * (vit_base + vit_growth + vit_life + vit_class));
                int pow = (int)(pow_race * (pow_base + pow_growth + pow_life + pow_class));
                int str = (int)(str_race * (str_base + str_growth + str_life + str_class));
                int intS = (int)(int_race * (int_base + int_growth + int_life + int_class));
                int mag = (int)(mag_race * (mag_base + mag_growth + mag_life + mag_class));
                int dex = (int)(dex_race * (dex_base + dex_growth + dex_life + dex_class));
                int agi = (int)(agi_race * (agi_base + (agi_growth + agi_life + agi_class)));
                int sns = (int)(sns_race * (sns_base + sns_growth + sns_life + sns_class));
                int app = (int)(app_race * (app_base + app_growth + app_life + app_class));
                int luk = (int)(luk_race * (luk_base + luk_growth + luk_life + luk_class));

                int hp = vit * 5 + str * 2;
                int sp = (int)(0.1f * (str * 7 + agi * 3));
                int san = pow * 5 + intS * 2;
                int mp = (int)(0.05f * (mag * 7 + pow * 3));

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("```\r\n");
                stringBuilder.Append("●基本\r\n");
                stringBuilder.Append($"【名前】{currentChara}\r\n");
                stringBuilder.Append($"【種族】{race}\r\n");
                stringBuilder.Append($"【性別】{gender}\r\n");
                stringBuilder.Append($"【ライフパス】{life_path}\r\n");
                stringBuilder.Append("●能力値\r\n");
                stringBuilder.Append($"【生命】{vit}((基礎{vit_base}+成長{vit_growth}+ライフパス{vit_life}+クラス{vit_class})×種族{vit_race})\r\n");
                stringBuilder.Append($"【精神】{pow}((基礎{pow_base}+成長{pow_growth}+ライフパス{pow_life}+クラス{pow_class})×種族{pow_race})\r\n");
                stringBuilder.Append($"【筋力】{str}((基礎{str_base}+成長{str_growth}+ライフパス{str_life}+クラス{str_class})×種族{str_race})\r\n");
                stringBuilder.Append($"【知力】{intS}((基礎{int_base}+成長{int_growth}+ライフパス{int_life}+クラス{int_class})×種族{int_race})\r\n");
                stringBuilder.Append($"【魔力】{mag}((基礎{mag_base}+成長{mag_growth}+ライフパス{mag_life}+クラス{mag_class})×種族{mag_race})\r\n");
                stringBuilder.Append($"【器用】{dex}((基礎{dex_base}+成長{dex_growth}+ライフパス{dex_life}+クラス{dex_class})×種族{dex_race})\r\n");
                stringBuilder.Append($"【敏捷】{agi}((基礎{agi_base}+成長{agi_growth}+ライフパス{agi_life}+クラス{agi_class})×種族{agi_race})\r\n");
                stringBuilder.Append($"【感知】{sns}((基礎{sns_base}+成長{sns_growth}+ライフパス{sns_life}+クラス{sns_class})×種族{sns_race})\r\n");
                stringBuilder.Append($"【魅力】{app}((基礎{app_base}+成長{app_growth}+ライフパス{app_life}+クラス{app_class})×種族{app_race})\r\n");
                stringBuilder.Append($"【幸運】{luk}((基礎{luk_base}+成長{luk_growth}+ライフパス{luk_life}+クラス{luk_class})×種族{luk_race})\r\n");
                stringBuilder.Append("●属性値\r\n");
                stringBuilder.Append($"【火属性】{(int)(fire_race * (fire_base + fire_title))}((基礎{fire_base}+称号{fire_title})×種族{fire_race})\r\n");
                stringBuilder.Append($"【水属性】{(int)(water_race * (water_base + water_title))}((基礎{water_base}+称号{water_title})×種族{water_race})\r\n");
                stringBuilder.Append($"【風属性】{(int)(wind_race * (wind_base + wind_title))}((基礎{wind_base}+称号{wind_title})×種族{wind_race})\r\n");
                stringBuilder.Append($"【電属性】{(int)(electric_race * (electric_base + electric_title))}((基礎{electric_base}+称号{electric_title})×種族{electric_race})\r\n");
                stringBuilder.Append($"【冷属性】{(int)(cold_race * (cold_base + cold_title))}((基礎{cold_base}+称号{cold_title})×種族{cold_race})\r\n");
                stringBuilder.Append($"【土属性】{(int)(soil_race * (soil_base + soil_title))}((基礎{soil_base}+称号{soil_title})×種族{soil_race})\r\n");
                stringBuilder.Append($"【光属性】{(int)(light_race * (light_base + light_title))}((基礎{light_base}+称号{light_title})×種族{light_race})\r\n");
                stringBuilder.Append($"【闇属性】{(int)(dark_race * (dark_base + dark_title))}((基礎{dark_base}+称号{dark_title})×種族{dark_race})\r\n");
                stringBuilder.Append($"●リソース\r\n");
                stringBuilder.Append($"【HP最大値】{(int)(hp * hp_rate)}({hp}×特性{hp_rate})\r\n");
                stringBuilder.Append($"【SP最大値】{(int)(sp * sp_rate)}({sp}×特性{sp_rate})\r\n");
                stringBuilder.Append($"【SAN最大値】{(int)(san * san_rate)}({san}×特性{san_rate})\r\n");
                stringBuilder.Append($"【MP最大値】{(int)(mp * mp_rate)}({mp}×特性{mp_rate})\r\n");
                stringBuilder.Append("●成長\r\n");
                stringBuilder.Append($"【レベル】{level}\r\n");
                stringBuilder.Append($"【経験値】{exp}\r\n");
                stringBuilder.Append($"【決意】{det}\r\n");
                stringBuilder.Append($"●習得称号\r\n");
                stringBuilder.Append($"{title_list}");
                stringBuilder.Append($"●習得クラス\r\n");
                stringBuilder.Append($"{class_list}");
                stringBuilder.Append($"●設定クラス\r\n");
                stringBuilder.Append($"{applied_class_list}");
                stringBuilder.Append($"●習得特性\r\n");
                stringBuilder.Append($"{trait_list}");
                stringBuilder.Append($"●習得スキル\r\n");
                stringBuilder.Append($"{skill_list}");
                stringBuilder.Append("```");

                await message.Channel.SendMessageAsync(stringBuilder.ToString());

            },
            async () =>
            {

                await message.Channel.SendMessageAsync(@$"```
●基本
【名前】{currentChara}
【種族】
【性別】
【ライフパス】
●能力値
【生命】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【精神】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【筋力】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【知力】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【魔力】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【器用】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【敏捷】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【感知】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【魅力】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
【幸運】0((基礎0+成長0+ライフパス0+クラス0)×種族1)
●属性値
【火属性】0((基礎0+称号0)×種族1)
【水属性】0((基礎0+称号0)×種族1)
【風属性】0((基礎0+称号0)×種族1)
【電属性】0((基礎0+称号0)×種族1)
【冷属性】0((基礎0+称号0)×種族1)
【土属性】0((基礎0+称号0)×種族1)
【光属性】0((基礎0+称号0)×種族1)
【闇属性】0((基礎0+称号0)×種族1)
●リソース
【HP最大値】0(0×1)
【SP最大値】0(0×1)
【SAN最大値】0(0×1)
【MP最大値】0(0×1)
●成長
【レベル】1
【経験値】0
【決意】0
●習得称号
●習得クラス
●設定クラス
●習得特性
●習得スキル
```");
            });
    }
}
