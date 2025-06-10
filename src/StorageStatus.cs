using Discord.WebSocket;
using System.Text.Json.Serialization;

partial class Program
{
    public class EquipmentData
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("durability")]
        public int Durability { get; set; }

        [JsonPropertyName("max_durability")]
        public int MaxDurability { get; set; }
    }

    private async Task UpdateStg(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?stg ".Length);
        var texts = text.Split(" ");
        await Command(texts, 133, message, user, async (currentChara) =>
        {
            string listName = string.Empty;
            if (texts[0] == "装備") listName = "equipment_list";
            else if (texts[0] == "貴重品") listName = "valuable_list";
            else if (texts[0] == "レシピ") listName = "recipe_list";
            else if (texts[0] == "道具") listName = "tool_list";
            else if (texts[0] == "素材") listName = "material_list";
            else if (texts[0] == "農業") listName = "farm_list";
            else
            {
                await message.Channel.SendMessageAsync("存在しないリストです。");
                return;
            }

            if (texts[0] == "装備")
            {
                await ConnectDatabase(@$"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin, equipment_list, valuable_list, recipe_list, tool_list, material_list, farm_list)
VALUES (
    @id,
    0, 0, 0, 0,
    jsonb_build_object(
        @item_id,
        jsonb_build_object(
            'count', GREATEST(@amount, 0),
            'durability', 5,
            'max_durability', 5
        )
    ),
    '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET {listName} = 
    CASE 
        WHEN COALESCE((user_storage.{listName}::jsonb->@item_id->>'count')::int, 0) + @amount <= 0 THEN 
            user_storage.{listName}::jsonb - @item_id
        ELSE 
            jsonb_set(
                user_storage.{listName}::jsonb,
                @path,
                jsonb_build_object(
                    'count', GREATEST(COALESCE((user_storage.{listName}->@item_id->>'count')::int, 0) + @amount, 0),
                    'durability', COALESCE((user_storage.{listName}->@item_id->>'durability')::int, 5),
                    'max_durability', COALESCE((user_storage.{listName}->@item_id->>'max_durability')::int, 5)
                ),
                true
            )
    END",
    parameters =>
    {
        parameters.AddWithValue("item_id", texts[1]);
        parameters.AddWithValue("amount", short.Parse(texts[2]));
        parameters.AddWithValue("id", currentChara);
        parameters.AddWithValue("path", new string[] { texts[1] });
    });
            }
            else
            {
                await ConnectDatabase(@$"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin, equipment_list, valuable_list, recipe_list, tool_list, material_list, farm_list)
VALUES (
    @id,
    0, 0, 0, 0,
    jsonb_build_object(@item_id, GREATEST(@amount, 0)), '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET {listName} = 
    CASE 
        WHEN COALESCE((user_storage.{listName}::jsonb->>@item_id)::int, 0) + @amount <= 0 THEN 
            user_storage.{listName}::jsonb - @item_id
        ELSE 
            jsonb_set(
                user_storage.{listName}::jsonb,
                @path,
                to_jsonb(COALESCE((user_storage.{listName}::jsonb->>@item_id)::int, 0) + @amount),
                true
            )
    END",
                    parameters =>
                    {
                        parameters.AddWithValue("item_id", texts[1]);
                        parameters.AddWithValue("amount", short.Parse(texts[2]));
                        parameters.AddWithValue("id", currentChara);
                        parameters.AddWithValue("path", new string[] { texts[1] });
                    });
            }

            await DisplayStorage(currentChara, message);
        });
    }

    private async Task UpdateDur(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?dur ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            await ConnectDatabase(@$"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin, equipment_list, valuable_list, recipe_list, tool_list, material_list, farm_list)
VALUES (
    @id,
    0, 0, 0, 0,
    jsonb_build_object(
        @item_id, 
        jsonb_build_object(
            'count', 1,
            'durability', GREATEST(@amount, 0),
            'max_durability', GREATEST(@amount, 0)
        )
    ), '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET equipment_list = jsonb_set(
    user_storage.equipment_list::jsonb,
    @path,
    jsonb_build_object(
        'count', COALESCE((user_storage.equipment_list->@item_id->>'count')::int, 1),
        'durability', LEAST(
            COALESCE((user_storage.equipment_list->@item_id->>'durability')::int, 0) + @amount,
            COALESCE((user_storage.equipment_list->@item_id->>'max_durability')::int, 0)
        ),
        'max_durability', COALESCE((user_storage.equipment_list->@item_id->>'max_durability')::int, 0)
    ),
    true
);",
            parameters =>
            {
                parameters.AddWithValue("item_id", texts[0]);
                parameters.AddWithValue("amount", short.Parse(texts[1]));
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("path", new string[] { texts[0] });
            });

            await DisplayStorage(currentChara, message);
        });
    }

    private async Task SetMaxDur(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set maxdur ".Length);
        var texts = text.Split(" ");
        await Command(texts, 13, message, user, async (currentChara) =>
        {
            await ConnectDatabase(@$"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin, equipment_list, valuable_list, recipe_list, tool_list, material_list, farm_list)
VALUES (
    @id,
    0, 0, 0, 0,
    jsonb_build_object(
        @item_id, 
        jsonb_build_object(
            'count', 1,
            'durability', GREATEST(@amount, 0),
            'max_durability', GREATEST(@amount, 0)
        )
    ), '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb, '{{}}'::jsonb
)
ON CONFLICT (id)
DO UPDATE SET equipment_list = jsonb_set(
user_storage.equipment_list::jsonb,
@path,
jsonb_build_object(
    'count', COALESCE((user_storage.equipment_list->@item_id->>'count')::int, 1),
    'durability', COALESCE((user_storage.equipment_list->@item_id->>'durability')::int, 0),
    'max_durability', @amount),
    true
);",
            parameters =>
            {
                parameters.AddWithValue("item_id", texts[0]);
                parameters.AddWithValue("amount", short.Parse(texts[1]));
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("path", new string[] { texts[0] });
            });

            await DisplayStorage(currentChara, message);
        });
    }

    private async Task UpdateCoin(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring($"?coin ".Length);
        var texts = text.Split(" ");

        await Command(texts, 1, message, user, async (currentChara) =>
        {
            int coin = short.Parse(texts[0]);
            int copperCoin = 0;
            int silverCoin = 0;
            int goldCoin = 0;
            int hollyCoin = 0;

            await ConnectDatabase(@"
WITH upsert AS (
  INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin)
  VALUES (@id, 0, 0, 0, 0)
  ON CONFLICT (id) DO NOTHING
)
SELECT copper_coin, silver_coin, gold_coin, holly_coin
FROM user_storage
WHERE id = @id;",
                parameters =>
                {
                    parameters.AddWithValue("id", currentChara);
                },
                async (reader) =>
                {
                    copperCoin = reader.GetInt16(0);
                    silverCoin = reader.GetInt16(1);
                    goldCoin = reader.GetInt16(2);
                    hollyCoin = reader.GetInt16(3);

                    await Task.Yield();
                },
                async () =>
                {
                    await Task.Yield();
                });

            coin += copperCoin + silverCoin * 12 + goldCoin * 144 + hollyCoin * 1728;

            copperCoin = coin % 12;
            silverCoin = coin % 144 / 12;
            goldCoin = coin % 1728 / 144;
            hollyCoin = coin / 1728;

            if (coin < 0)
            {
                await message.Channel.SendMessageAsync("お金が足りません。");
                return;
            }

            await ConnectDatabase($@"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_coin)
VALUES (@id, @copper_coin, @silver_coin, @gold_coin, @holly_coin)
ON CONFLICT (id) DO UPDATE
SET copper_coin = EXCLUDED.copper_coin,
silver_coin = EXCLUDED.silver_coin,
gold_coin = EXCLUDED.gold_coin,
holly_coin = EXCLUDED.holly_coin;",
            parameters =>
            {
                parameters.AddWithValue("id", currentChara);
                parameters.AddWithValue("copper_coin", copperCoin);
                parameters.AddWithValue("silver_coin", silverCoin);
                parameters.AddWithValue("gold_coin", goldCoin);
                parameters.AddWithValue("holly_coin", hollyCoin);
            });

            await DisplayStorage(currentChara, message);
        });
    }

    private async Task ShowStg(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        if (!_currentCharaDic.TryGetValue(user.Id, out var currentChara))
        {
            await message.Channel.SendMessageAsync("「?login [キャラクター名]」を呼んでください。");
            return;
        }

        await DisplayStorage(currentChara, message);
    }

    private async Task DisplayStorage(string currentChara, SocketMessage message)
    {
        await ConnectDatabase("SELECT * FROM user_storage WHERE id = @id;",
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