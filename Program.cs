using Discord;
using Discord.WebSocket;
using dotenv.net;
using MathNet.Numerics.Random;
using Npgsql;

partial class Program
{
    private DiscordSocketClient? _client;
    private NpgsqlConnection? _connection;

    private Dictionary<ulong, string> _currentCharaDic = new Dictionary<ulong, string>();

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
            GatewayIntents = GatewayIntents.AllUnprivileged |
             GatewayIntents.MessageContent |
             GatewayIntents.GuildMessages
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
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message is SocketUserMessage userMessage && message.Channel is SocketGuildChannel guildChannel)
        {
            var guild = guildChannel.Guild;
            var user = guild.GetUser(message.Author.Id);
            var content = message.Content;

            if (content.StartsWith("?login "))
            {
                await Login(message, guild, user, content);
            }
            else if (content.StartsWith("?set bon "))
            {
                await SetBon(message, guild, user, content);
            }
            else if (content.StartsWith("?r"))
            {
                await SimpleRoll(message, guild, user, content);
            }
            else if (content.StartsWith("?r "))
            {
                await DiceRoll(message, guild, user, content);
            }
            else if (content.StartsWith("?set res "))
            {
                await SetRes(message, guild, user, content);
            }
            else if (content.StartsWith("?hp "))
            {
                await UpdateHp(message, guild, user, content);
            }
            else if (content.StartsWith("?sp "))
            {
                await UpdateSp(message, guild, user, content);
            }
            else if (content.StartsWith("?san "))
            {
                await UpdateSan(message, guild, user, content);
            }
            else if (content.StartsWith("?mp "))
            {
                await UpdateMp(message, guild, user, content);
            }
            else if (content.StartsWith("?set wep "))
            {
                await SetWep(message, guild, user, content);
            }
            else if(content.StartsWith("?"))
            {
                await message.Channel.SendMessageAsync(":eyes:それはコマンドですか？");
            }
        }
    }
}
