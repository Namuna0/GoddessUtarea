using Discord.WebSocket;
using Npgsql;

partial class Program
{
    Dictionary<string, StorageStatus> UserStrages;

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
        public Dictionary<string, int> FarmList { get; set; } = new Dictionary<string, int>();
    }

    private async Task AddStg(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        string[] listName =
        {
            "equipment_data",
            "",
            "",
        };

        var text = message.Content.Substring("?add stg ".Length);
        var texts = text.Split(" ");
        await Command(texts, 133, message, user, async (currentChara) =>
        {
            await ConnectDatabase(@"
UPDATE user_storage
SET equipment_data = jsonb_set(
    equipment_data::jsonb,
    ARRAY[@itemId],
    to_jsonb(
        COALESCE((equipment_data->>@itemId)::int, 0) + @amount
    )
)
WHERE id = @id;",
                parameters =>
                {
                    parameters.AddWithValue("itemId", texts[1]);
                    parameters.AddWithValue("amount", int.Parse(texts[2]));
                    parameters.AddWithValue("id", currentChara);
                });

            //await DisplayStorage(currentChara, message);
        });
    }

    //private async Task DisplayStorage(string currentChara, SocketMessage message)
    //{
    //    await ConnectDatabase("SELECT storage_data FROM user_storage WHERE discord_id = @id",
    //        parameters =>
    //        {
    //            parameters.AddWithValue("path", $"{{EquipmentList,{texts[1]}}}");
    //            parameters.AddWithValue("itemId", texts[1]);
    //            parameters.AddWithValue("amount", int.Parse(texts[2]));
    //            parameters.AddWithValue("id", currentChara);
    //        });

    //    await message.Channel.SendMessageAsync(
    //        "```\r\n" +
    //        $"【キャラクター】{currentChara}\r\n" +
    //        $"【所持金】{status.HollySilverCoin * 1728 + status.GoldCoin * 144 + status.SilverCoin * 12 + status.CopperCoin}ジルダ\r\n" +
    //        $"《ジルダ銅貨》×{status.CopperCoin}\r\n" +
    //        $"《ジルダ銀貨》×{status.SilverCoin}\r\n" +
    //        $"《ジルダ金貨》×{status.GoldCoin}\r\n" +
    //        $"《ジルダ聖銀貨》×{status.HollySilverCoin}\r\n" +
    //        "●装備\r\n" +
    //        $"{0}\r\n" +
    //        "●貴重品\r\n" +
    //        $"{0}" +
    //        "●レシピ\r\n" +
    //        $"{0}\r\n" +
    //        "●道具\r\n" +
    //        $"{0}\r\n" +
    //        "●素材\r\n" +
    //        $"{0}\r\n" +
    //        "●農業\r\n" +
    //        $"{0}\r\n" +
    //        "```");
    //    var cmd = new NpgsqlCommand("SELECT storage_data FROM user_storage WHERE discord_id = @id", conn);
    //    cmd.Parameters.AddWithValue("id", currentChara);
    //}

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