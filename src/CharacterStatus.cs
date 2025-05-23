﻿using Discord.WebSocket;
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

    private async Task ShowSta(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await DisplayStatus(currentChara, message);
    }

    private async Task DisplayStatus(string currentChara, SocketMessage message)
    {
        await ConnectDatabase(
@"WITH upsert AS (
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
    wing_base, wing_title, wing_race,
    electric_base, electric_title, electric_race,
    cold_base, cold_title, cold_race,
    soil_base, soil_title, soil_race,
    level, exp, det,
    title,
    class,
    applied_class,
    traits,
    skill)
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
                var copper_coin = reader.GetInt16(reader.GetOrdinal("copper_coin"));
                var silver_coin = reader.GetInt16(reader.GetOrdinal("silver_coin"));
                var gold_coin = reader.GetInt16(reader.GetOrdinal("gold_coin"));
                var holly_coin = reader.GetInt16(reader.GetOrdinal("holly_coin"));
                var b = reader.GetFieldValue<string>(reader.GetOrdinal("farm_list"));

                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, EquipmentData>>(reader.GetFieldValue<string>(reader.GetOrdinal("equipment_list")));
                var equipment_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value.Count} {kv.Value.Durability}/{kv.Value.MaxDurability}"));
                if (equipment_list.Length > 0) equipment_list += "\r\n";

                var dict2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("valuable_list")));
                var valuable_list = string.Join(", ", dict2.Select(kv => $"{kv.Key}×{kv.Value}"));
                if (valuable_list.Length > 0) valuable_list += "\r\n";

                dict2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("recipe_list")));
                var recipe_list = string.Join(", ", dict2.Select(kv => $"{kv.Key}×{kv.Value}"));
                if (recipe_list.Length > 0) recipe_list += "\r\n";

                dict2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("tool_list")));
                var tool_list = string.Join(", ", dict2.Select(kv => $"{kv.Key}×{kv.Value}"));
                if (tool_list.Length > 0) tool_list += "\r\n";

                dict2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("material_list")));
                var material_list = string.Join(", ", dict2.Select(kv => $"{kv.Key}×{kv.Value}"));
                if (material_list.Length > 0) material_list += "\r\n";

                dict2 = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("farm_list")));
                var farm_list = string.Join(", ", dict2.Select(kv => $"{kv.Key}×{kv.Value}"));
                if (farm_list.Length > 0) farm_list += "\r\n";

                await message.Channel.SendMessageAsync(
                    "```\r\n" +
                    $"【キャラクター】{currentChara}\r\n" +
                    $"【所持金】{holly_coin * 1728 + gold_coin * 144 + silver_coin * 12 + copper_coin}ジルダ\r\n" +
                    $"《ジルダ銅貨》×{copper_coin}\r\n" +
                    $"《ジルダ銀貨》×{silver_coin}\r\n" +
                    $"《ジルダ金貨》×{gold_coin}\r\n" +
                    $"《ジルダ聖銀貨》×{holly_coin}\r\n" +
                    "●装備\r\n" +
                    $"{equipment_list}" +
                    "●貴重品\r\n" +
                    $"{valuable_list}" +
                    "●レシピ\r\n" +
                    $"{recipe_list}" +
                    "●道具\r\n" +
                    $"{tool_list}" +
                    "●素材\r\n" +
                    $"{material_list}" +
                    "●農業\r\n" +
                    $"{farm_list}" +
                    "```");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
            });
    }
}
