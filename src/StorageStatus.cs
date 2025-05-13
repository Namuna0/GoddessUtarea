using Discord.WebSocket;

partial class Program
{
    class StorageStatus
    {
        public int Gilda { get; set; } = 0;

        public Dictionary<string, int> EquipmentList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ValuableList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RecipeList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ToolList { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FarmList { get; set; } = new Dictionary<string, int>();
    }

    private async Task ShowStorage(string currentChara, CharacterStatus status, SocketMessage message)
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
            "●レシピ" +
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