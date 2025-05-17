using Discord.WebSocket;

partial class Program
{
    class StorageStatus
    {
        public int CopperCoin { get; set; } = 0;
        public int SilverCoin { get; set; } = 0;
        public int GoldCoin { get; set; } = 0;
        public int HollySilverCoin { get; set; } = 0;

        public Dictionary<string, int> EquipmentList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ValuableList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RecipeList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ToolList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MaterialList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FarmList { get; set; } = new Dictionary<string, int>();
    }

    private async Task AddStg(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?add stg ".Length);
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


            switch (texts[0])
            {
                case "装備":
                    listName = "equipment_list";
                    break;

                case "貴重品":
                    listName = "valuable_list";
                    break;

                case "レシピ":
                    listName = "recipe_list";
                    break;

                case "道具":
                    listName = "tool_list";
                    break;

                case "素材":
                    listName = "material_list";
                    break;

                case "農業":
                    listName = "farm_list";
                    break;
            }

            await ConnectDatabase(@$"
INSERT INTO user_storage (id, copper_coin, silver_coin, gold_coin, holly_silver_coin, equipment_list, valuable_list, recipe_list, tool_list, material_list, farm_list)
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
                var holly_silver_coin = reader.GetInt16(reader.GetOrdinal("holly_silver_coin"));
                var b = reader.GetFieldValue<string>(reader.GetOrdinal("farm_list"));

                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("equipment_list")));
                var equipment_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("valuable_list")));
                var valuable_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("recipe_list")));
                var recipe_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("tool_list")));
                var tool_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("material_list")));
                var material_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetFieldValue<string>(reader.GetOrdinal("farm_list")));
                var farm_list = string.Join(", ", dict.Select(kv => $"{kv.Key}×{kv.Value}"));

                await message.Channel.SendMessageAsync(
                    "```\r\n" +
                    $"【キャラクター】{currentChara}\r\n" +
                    $"【所持金】{holly_silver_coin * 1728 + gold_coin * 144 + silver_coin * 12 + copper_coin}ジルダ\r\n" +
                    $"《ジルダ銅貨》×{copper_coin}\r\n" +
                    $"《ジルダ銀貨》×{silver_coin}\r\n" +
                    $"《ジルダ金貨》×{gold_coin}\r\n" +
                    $"《ジルダ聖銀貨》×{holly_silver_coin}\r\n" +
                    "●装備\r\n" +
                    $"{equipment_list}\r\n" +
                    "●貴重品\r\n" +
                    $"{valuable_list}\r\n" +
                    "●レシピ\r\n" +
                    $"{recipe_list}\r\n" +
                    "●道具\r\n" +
                    $"{tool_list}\r\n" +
                    "●素材\r\n" +
                    $"{material_list}\r\n" +
                    "●農業\r\n" +
                    $"{farm_list}\r\n" +
                    "```");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("キャラクターが見つかりませんでした。");
            });
    }

    private async Task DisplayStorage(string currentChara, StorageStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            "```\r\n" +
            $"【キャラクター】{currentChara}\r\n" +
            $"【所持金】{status.HollySilverCoin * 1728 + status.GoldCoin * 144 + status.SilverCoin * 12 + status.CopperCoin}ジルダ\r\n" +
            $"《ジルダ銅貨》×{status.CopperCoin}\r\n" +
            $"《ジルダ銀貨》×{status.SilverCoin}\r\n" +
            $"《ジルダ金貨》×{status.GoldCoin}\r\n" +
            $"《ジルダ聖銀貨》×{status.HollySilverCoin}\r\n" +
            "●装備\r\n" +
            $"{0}\r\n" +
            "●貴重品\r\n" +
            $"{0}" +
            "●レシピ\r\n" +
            $"{0}\r\n" +
            "●道具\r\n" +
            $"{0}\r\n" +
            "●素材\r\n" +
            $"{0}\r\n" +
            "●農業\r\n" +
            $"{0}\r\n" +
            "```");
    }
}