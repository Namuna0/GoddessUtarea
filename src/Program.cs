using Discord;
using Discord.WebSocket;
using dotenv.net;
using MathNet.Numerics.Random;

partial class Program
{
    private DiscordSocketClient? _client;

    private Dictionary<ulong, string> _currentCharaDic = new Dictionary<ulong, string>();
    private Dictionary<string, NpcStatus> _npcStatus = new Dictionary<string, NpcStatus>();

    private MersenneTwister _ms = new MersenneTwister();

    static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        await ConnectServer();

        await Task.Delay(-1); // 永久に実行
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task ConnectServer()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        _client.MessageReceived += MessageReceivedAsync;

        DotEnv.Load();
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Discordトークンが見つかりません。");
            return;
        }

        long ticks = DateTime.Now.Ticks; // 100ナノ秒単位
        int seed = (int)(ticks & 0xFFFFFFFF); // 下位32ビットを使用
        _ms = new MersenneTwister(seed);

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        //await Create();
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message is SocketUserMessage userMessage && message.Channel is SocketGuildChannel guildChannel)
        {
            var guild = guildChannel.Guild;
            var user = guild.GetUser(message.Author.Id);
            var content = message.Content;

            if (content.StartsWith("?show data ")) await ShowData(message, guild, user);
            else if (content.StartsWith("?set data")) await SetData(message, guild, user);

            // キャラクター
            else if (content.StartsWith("?login ")) await Login(message, guild, user);
            else if (content == "?show sta") await ShowSta(message, guild, user);
            else if (content.StartsWith("?set sta")) await SetSta(message, guild, user);
            else if (content.StartsWith("?set vit ")) await SetAbi("vit", message, guild, user);
            else if (content.StartsWith("?set pow ")) await SetAbi("pow", message, guild, user);
            else if (content.StartsWith("?set str ")) await SetAbi("str", message, guild, user);
            else if (content.StartsWith("?set int ")) await SetAbi("int", message, guild, user);
            else if (content.StartsWith("?set mag ")) await SetAbi("mag", message, guild, user);
            else if (content.StartsWith("?set dex ")) await SetAbi("dex", message, guild, user);
            else if (content.StartsWith("?set agi ")) await SetAbi("agi", message, guild, user);
            else if (content.StartsWith("?set sns ")) await SetAbi("sns", message, guild, user);
            else if (content.StartsWith("?set app ")) await SetAbi("app", message, guild, user);
            else if (content.StartsWith("?set luk ")) await SetAbi("luk", message, guild, user);
            else if (content.StartsWith("?set fire ")) await SetEle("fire", message, guild, user);
            else if (content.StartsWith("?set water ")) await SetEle("water", message, guild, user);
            else if (content.StartsWith("?set wind ")) await SetEle("wind", message, guild, user);
            else if (content.StartsWith("?set electric ")) await SetEle("electric", message, guild, user);
            else if (content.StartsWith("?set cold ")) await SetEle("cold", message, guild, user);
            else if (content.StartsWith("?set soil ")) await SetEle("soil", message, guild, user);

            else if (content.StartsWith("?level ")) await UpdateLevel(message, guild, user);
            else if (content.StartsWith("?exp ")) await UpdateExp(message, guild, user);
            else if (content.StartsWith("?det ")) await UpdateDet(message, guild, user);
            else if (content.StartsWith("?e&d ")) await UpdateExpDet(message, guild, user);
            else if (content.StartsWith("?set resrate")) await SetResRate(message, guild, user);
            else if (content.StartsWith("?add sta ")) await AddSta(message, guild, user);
            else if (content.StartsWith("?remove sta ")) await RemoveSta(message, guild, user);

            // 所有品
            else if (content == "?show stg") await ShowStg(message, guild, user);
            else if (content.StartsWith("?stg ")) await UpdateStg(message, guild, user);
            else if (content.StartsWith("?dur ")) await UpdateDur(message, guild, user);
            else if (content.StartsWith("?set maxdur ")) await SetMaxDur(message, guild, user);
            else if (content.StartsWith("?coin ")) await UpdateCoin(message, guild, user);

            // 装備
            else if (content == "?show res") await ShowRes(message, guild, user);
            else if (content.StartsWith("?set res ")) await SetRes(message, guild, user);
            else if (content == "?reset res") await ResetRes(message, guild, user);
            else if (content.StartsWith("?hp ")) await UpdateRes("hp", message, guild, user);
            else if (content.StartsWith("?sp ")) await UpdateRes("sp", message, guild, user);
            else if (content.StartsWith("?san ")) await UpdateRes("san", message, guild, user);
            else if (content.StartsWith("?mp ")) await UpdateRes("mp", message, guild, user);
            else if (content == "?show bon") await ShowBon(message, guild, user);
            else if (content.StartsWith("?set res bon ")) await SetResBon(message, guild, user);
            else if (content.StartsWith("?set ele bon ")) await SetEleBon(message, guild, user);
            else if (content.StartsWith("?set wep ")) await SetWep(message, guild, user);

            else if (content == "?show master res") await ShowMasterRes(message, guild, user);
            else if (content.StartsWith("?master hp ")) await UpdateMasterRes("hp", message, guild, user);
            else if (content.StartsWith("?master sp ")) await UpdateMasterRes("sp", message, guild, user);
            else if (content.StartsWith("?master san ")) await UpdateMasterRes("san", message, guild, user);
            else if (content.StartsWith("?master mp ")) await UpdateMasterRes("mp", message, guild, user);

            // バトル
            else if (content == "?r") await SimpleRoll(message, guild, user);
            else if (content.StartsWith("?r ")) await DiceRoll(message, guild, user);
            else if (content == "?durr") await RollDurability(message, guild, user);

            else if (content.StartsWith("?show npc res")) await ShowNpcRes(message, guild, user);
            else if (content.StartsWith("?set npc res ")) await SetNpcRes(message, guild, user);
            else if (content.StartsWith("?npc hp ")) await UpdateNpcHp(message, guild, user);
            else if (content.StartsWith("?show npc bon ")) await ShowNpcBon(message, guild, user);
            else if (content.StartsWith("?set npc bon ")) await SetNpcBon(message, guild, user);
            else if (content.StartsWith("?npc r ")) await NpcDiceRoll(message, guild, user);
        }
    }

    public async Task Command(string[] texts, long flag, SocketMessage message, SocketGuildUser user, Func<string, Task> onCompleted)
    {
        if (!GetPaseFlag(texts, flag))
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

    private bool GetPaseFlag(string[] texts, long target)
    {
        if (texts.Length != target.ToString().Length)
        {
            return false;
        }

        long digit = 1;
        foreach (string text in texts)
        {
            int flag = 0;

            if (int.TryParse(text, out _)) flag = 1;
            else if (float.TryParse(text, out _)) flag = 2;
            else flag = 3;

            long num = target % (digit * 10) / digit;
            if (flag > num) return false;

            digit *= 10;
        }

        return true;
    }

}
