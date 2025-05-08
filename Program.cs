using Discord;
using Discord.WebSocket;
using dotenv.net;

class Program
{
    class UserStatus
    {
        public int MaxHp { get; set; }
        public int MaxSp { get; set; }
        public int MaxSan { get; set; }
        public int MaxMp { get; set; }
        public int Hp { get; set; }
        public int Sp { get; set; }
        public int San { get; set; }
        public int Mp { get; set; }

        public UserStatus(int hp, int sp, int san, int mp)
        {
            MaxHp = hp;
            MaxSp = sp;
            MaxSan = san;
            MaxMp = mp;
            Hp = hp;
            Sp = sp;
            San = san;
            Mp = mp;
        }
    }

    private DiscordSocketClient? _client;
    private Dictionary<string, UserStatus> _userStatusDic = new Dictionary<string, UserStatus>();

    static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
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
        else
        {
            Console.WriteLine($"{token}");
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1); // 永久に実行
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message is SocketUserMessage userMessage && message.Channel is SocketGuildChannel guildChannel)
        {
            var content = message.Content;
            var guild = guildChannel.Guild;
            var user = guild.GetUser(message.Author.Id);

            if (content == "?にゅ")
            {
                await message.Channel.SendMessageAsync("こんにちは！");
            }
            else if (content == "?こんにちは")
            {
                string displayName = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username; // ニックネームがない場合はユーザー名を使用

                Console.WriteLine($"Display Name: {displayName}");
                await message.Channel.SendMessageAsync($"こんにちは、{displayName}ちゃん！");
            }
            else if (content.StartsWith("?set "))
            {
                var text = content.Substring("?set ".Length);
                var texts = text.Split(" ");
                if (texts.Length >= 5)
                {
                    var status = new UserStatus(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]), int.Parse(texts[4]));
                    _userStatusDic[texts[0]] = status;

                    await ShowStatus(texts[0], status, message);
                }
                else
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
            }
            else if (content.StartsWith("?hp "))
            {
                var text = content.Substring("?hp ".Length);
                var texts = text.Split(" ");
                if (texts.Length < 2)
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
                else if (!_userStatusDic.TryGetValue(texts[0], out var status))
                {
                    await message.Channel.SendMessageAsync("「?set [キャラクター名]」を呼んでください。");
                }
                else
                {
                    status.Hp += int.Parse(texts[1]);
                    await ShowStatus(texts[0], status, message);
                }
            }
            else if (content.StartsWith("?sp "))
            {
                var text = content.Substring("?sp ".Length);
                var texts = text.Split(" ");
                if (texts.Length < 2)
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
                else if (!_userStatusDic.TryGetValue(texts[0], out var status))
                {
                    await message.Channel.SendMessageAsync("「?set [キャラクター名]」を呼んでください。");
                }
                else
                {
                    status.Sp += int.Parse(texts[1]);
                    await ShowStatus(texts[0], status, message);
                }
            }
            else if (content.StartsWith("?san "))
            {
                var text = content.Substring("?san ".Length);
                var texts = text.Split(" ");
                if (texts.Length < 2)
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
                else if (!_userStatusDic.TryGetValue(texts[0], out var status))
                {
                    await message.Channel.SendMessageAsync("「?set [キャラクター名]」を呼んでください。");
                }
                else
                {
                    status.San += int.Parse(texts[1]);
                    await ShowStatus(texts[0], status, message);
                }
            }
            else if (content.StartsWith("?mp "))
            {
                var text = content.Substring("?mp ".Length);
                var texts = text.Split(" ");
                if (texts.Length < 2)
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
                else if (!_userStatusDic.TryGetValue(texts[0], out var status))
                {
                    await message.Channel.SendMessageAsync("「?set [キャラクター名]」を呼んでください。");
                }
                else
                {
                    status.Mp += int.Parse(texts[1]);
                    await ShowStatus(texts[0], status, message);
                }
            }
        }
    }

    private async Task ShowStatus(string key, UserStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{key}\r\n" +
            "●リソース\r\n" +
            $"【HP】{status.Hp}/{status.MaxHp}\r\n" +
            $"【SP】{status.Sp}/{status.MaxSp}\r\n" +
            $"【SAN】{status.San}/{status.MaxSan}\r\n" +
            $"【MP】{status.Mp}/{status.MaxMp}\r\n" +
            $"【防御点】\r\n" +
            "●状態\r\n" +
            "●永続状態");
    }
}