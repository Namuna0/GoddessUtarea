using Discord;
using Discord.WebSocket;
using dotenv.net;
using MathNet.Numerics.Random;
using System.Text.RegularExpressions;

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

    private MersenneTwister _ms = new MersenneTwister();

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

        long ticks = DateTime.Now.Ticks; // 100ナノ秒単位
        int seed = (int)(ticks & 0xFFFFFFFF); // 下位32ビットを使用
        _ms = new MersenneTwister(seed);

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
                    await message.Channel.SendMessageAsync($"{texts[0]}を登録しました！");
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
            else if (content.StartsWith("?r "))
            {
                var text = content.Substring("?r ".Length);
                var texts = text.Split(" ");

                if (texts.Length < 1)
                {
                    CalcFormula(texts[1], null, out string culcResult, out string showResult);

                    await message.Channel.SendMessageAsync(
                        $"<@{user.Id}> :game_die:\r\n" +
                        $"{showResult}=>{culcResult}");
                }
                else if (!_userStatusDic.TryGetValue(texts[0], out var status))
                {
                    await message.Channel.SendMessageAsync("「?create [キャラクター名]」を呼んでください。");
                }
                else
                {
                    CalcFormula(texts[1], status, out string culcResult, out string showResult);

                    await message.Channel.SendMessageAsync(
                        $"<@{user.Id}> :game_die:\r\n" +
                        $"{showResult}=>{culcResult}");
                }
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
            $"【HP】{status.Hp}/{status.MaxHp}【SP】{status.Sp}/{status.MaxSp}\r\n" +
            $"【SAN】{status.San}/{status.MaxSan}【MP】{status.Mp}/{status.MaxMp}\r\n" +
            $"【防御点】\r\n" +
            "●状態\r\n" +
            "●永続状態");
    }

    private void CalcFormula(string originalText, UserStatus status, out string culcResult, out string showResult)
    {
        string culcText = originalText;
        string showText = originalText;

        if (status != null)
        {
            CalcBonusDice(@"\[生命B\]", status.VitB, ref culcText, ref showText);
            CalcBonusDice("生命B", status.VitB, ref culcText, ref showText);
            CalcBonusDice(@"\[精神B\]", status.PowB, ref culcText, ref showText);
            CalcBonusDice("精神B", status.PowB, ref culcText, ref showText);
            CalcBonusDice(@"\[筋力B\]", status.StrB, ref culcText, ref showText);
            CalcBonusDice("筋力B", status.StrB, ref culcText, ref showText);
            CalcBonusDice(@"\[知力N\]", status.IntB, ref culcText, ref showText);
            CalcBonusDice("知力B", status.IntB, ref culcText, ref showText);
            CalcBonusDice(@"\[魔力B\]", status.MagB, ref culcText, ref showText);
            CalcBonusDice("魔力B", status.MagB, ref culcText, ref showText);
            CalcBonusDice(@"\[器用B\]", status.DexB, ref culcText, ref showText);
            CalcBonusDice("器用B", status.DexB, ref culcText, ref showText);
            CalcBonusDice(@"\[敏捷B\]", status.AgiB, ref culcText, ref showText);
            CalcBonusDice("敏捷B", status.AgiB, ref culcText, ref showText);
            CalcBonusDice(@"\[感知B\]", status.SnsB, ref culcText, ref showText);
            CalcBonusDice("感知B", status.SnsB, ref culcText, ref showText);
            CalcBonusDice(@"\[魅力B\]", status.AppB, ref culcText, ref showText);
            CalcBonusDice("魅力B", status.AppB, ref culcText, ref showText);
            CalcBonusDice(@"\[幸運B\]", status.LukB, ref culcText, ref showText);
            CalcBonusDice("幸運B", status.LukB, ref culcText, ref showText);
        }

        CalcDice(ref culcText, ref showText);

        var expr = new NCalc.Expression(culcText);

        showText = showText.Replace("*", @"\*");

        culcResult = float.Parse(expr.Evaluate()?.ToString() ?? "0").ToString("0.##");
        showResult = showText;
    }

    private void CalcDice(ref string culcText, ref string showText)
    {
        List<int> dices = new List<int>();

        showText = Regex.Replace(showText, @"(\d+)d(\d+)", match =>
        {
            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);

            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += _ms.Next(1, sides + 1);
            }
            dices.Add(total);

            return $"{count}d{sides}({total})";
        });

        int i = 0;
        culcText = Regex.Replace(culcText, @"(\d+)d(\d+)", match =>
        {
            var result = (dices[i]).ToString();

            i++;

            return result;
        });
    }

    private void CalcBonusDice(string text, string bonusText, ref string culcText, ref string showText)
    {
        List<int> dices = new List<int>();

        float culcBonus = float.Parse(bonusText);
        string showBonus = culcBonus.ToString("0.##");

        showText = Regex.Replace(showText, text, match =>
        {
            int dice = _ms.Next(1, 101);
            dices.Add(dice);
            return $"1d100({dice})*" + showBonus;
        });

        int i = 0;
        culcText = Regex.Replace(culcText, text, match =>
        {
            var result = (culcBonus * dices[i]).ToString();

            i++;

            return result;
        });
    }
}