using Discord.WebSocket;
using System.IO;

partial class Program
{
    Dictionary<string, StorageStatus> UserStrages;

    class StorageStatus
    {
        public int CopperCoin { get; set; } = 0;

        public Dictionary<string, int> EquipmentList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ValuableList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RecipeList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ToolList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FarmList { get; set; } = new Dictionary<string, int>();
    }

    private async Task AddStg(SocketMessage message, SocketGuild guild, SocketGuildUser user, string content)
    {
        var text = content.Substring("?add stg ".Length);
        var texts = text.Split(" ");
        await Command(texts, 133, message, user, async (currentChara) =>
        {
            var sql = @"
UPDATE user_storage
SET storage_data = jsonb_set(
    storage_data::jsonb,
    @path::text[],
    to_jsonb(
        COALESCE((storage_data->'EquipmentList'->>@itemId)::int, 0) + @amount
    )
)
WHERE id = @id;";

            await ConnectDatabase(
                sql,
                parameters =>
                {
                    parameters.AddWithValue("path", $"{{EquipmentList,{texts[1]}}}");
                    parameters.AddWithValue("itemId", texts[1]);
                    parameters.AddWithValue("amount", int.Parse(texts[2]));
                    parameters.AddWithValue("id", currentChara);
                });

            await DisplayStorage(currentChara, message);
        });
    }

    private async Task DisplayStorage(string currentChara, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            "```\r\n" +
            $"【キャラクター】{currentChara}\r\n" +
            $"【所持金】{0}ジルダ\r\n" +
            $"《ジルダ銅貨》×{0}\r\n" +
            $"《ジルダ銀貨》×{0}\r\n" +
            $"《ジルダ金貨》×{0}\r\n" +
            $"《ジルダ聖銀貨》×{0}\r\n" +
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