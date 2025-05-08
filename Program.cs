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

        public string VitB { get; set; }
        public string PowB { get; set; }
        public string StrB { get; set; }
        public string IntB { get; set; }
        public string MagB { get; set; }
        public string DexB { get; set; }
        public string AgiB { get; set; }
        public string SnsB { get; set; }
        public string AppB { get; set; }
        public string LukB { get; set; }

        public void SetResource(int hp, int sp, int san, int mp)
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

        public void SetBonus(string vitB, string powB, string strB, string intB, string magB, string dexB, string agiB, string snsB, string appB, string lukB)
        {
            VitB = vitB;
            PowB = powB;
            StrB = strB;
            IntB = intB;
            MagB = magB;
            DexB = dexB;
            AgiB = agiB;
            SnsB = snsB;
            AppB = appB;
            LukB = lukB;
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
            var guild = guildChannel.Guild;
            var user = guild.GetUser(message.Author.Id);
            var content = message.Content;

            if (content == "?こんにちは")
            {
                string displayName = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username; // ニックネームがない場合はユーザー名を使用

                Console.WriteLine($"Display Name: {displayName}");
                await message.Channel.SendMessageAsync($"こんにちは、{displayName}ちゃん！");
            }
            else if (content.StartsWith("?create "))
            {
                var text = content.Substring("?create ".Length);
                var texts = text.Split(" ");
                if (texts.Length < 1)
                {
                    await message.Channel.SendMessageAsync("引数が変です。");
                }
                else
                {
                    _userStatusDic[texts[0]] = new UserStatus();
                }
            }
            else if (content.StartsWith("?set bon "))
            {
                var text = content.Substring("?set bon ".Length);
                var texts = text.Split(" ");
                await Command(texts, 11, message, async (status) =>
                {
                    status.SetBonus(
                        texts[1], texts[2], texts[3], texts[4], texts[5],
                        texts[6], texts[7], texts[8], texts[9], texts[10]);
                    await ShowBonus(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?vit"))
            {
                var text = content.Substring("?set bon ".Length);
                var texts = text.Split(" ");
                await Command(texts, 11, message, async (status) =>
                {
                    status.SetBonus(
                        texts[1], texts[2], texts[3], texts[4], texts[5],
                        texts[6], texts[7], texts[8], texts[9], texts[10]);
                    await ShowBonus(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?set res "))
            {
                var text = content.Substring("?set res ".Length);
                var texts = text.Split(" ");
                await Command(texts, 5, message, async (status) =>
                {
                    status.SetResource(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]), int.Parse(texts[4]));
                    await ShowResource(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?hp "))
            {
                var text = content.Substring("?hp ".Length);
                var texts = text.Split(" ");
                await Command(texts, 2, message, async (status) =>
                {
                    status.Hp += int.Parse(texts[1]);
                    await ShowResource(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?sp "))
            {
                var text = content.Substring("?sp ".Length);
                var texts = text.Split(" ");
                await Command(texts, 2, message, async (status) =>
                {
                    status.Sp += int.Parse(texts[1]);
                    await ShowResource(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?san "))
            {
                var text = content.Substring("?san ".Length);
                var texts = text.Split(" ");
                await Command(texts, 2, message, async (status) =>
                {
                    status.San += int.Parse(texts[1]);
                    await ShowResource(texts[0], status, message);
                });
            }
            else if (content.StartsWith("?mp "))
            {
                var text = content.Substring("?mp ".Length);
                var texts = text.Split(" ");
                await Command(texts, 2, message, async (status) =>
                {
                    status.Mp += int.Parse(texts[1]);
                    await ShowResource(texts[0], status, message);
                });
            }
        }
    }

    private async Task Command(string[] texts, int length, SocketMessage message, Func<UserStatus, Task> onCompleted)
    {
        if (texts.Length < length)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
        }
        else if (!_userStatusDic.TryGetValue(texts[0], out var status))
        {
            await message.Channel.SendMessageAsync("「?create [キャラクター名]」を呼んでください。");
        }
        else
        {
            await onCompleted.Invoke(status);
        }
    }

    private async Task ShowBonus(string key, UserStatus status, SocketMessage message)
    {
        await message.Channel.SendMessageAsync(
            $"{key}\r\n" +
            "●能力値B\r\n" +
            $"{status.VitB} 生命, {status.PowB} 精神, {status.StrB} 筋力, {status.IntB} 知力, {status.MagB} 魔力\r\n" +
            $"{status.DexB} 器用, {status.AgiB} 敏捷, {status.SnsB} 感知, {status.AppB} 魅力, {status.LukB} 幸運");
    }

    private async Task ShowResource(string key, UserStatus status, SocketMessage message)
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