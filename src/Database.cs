using Discord.WebSocket;
using Npgsql;

partial class Program
{
    public async Task ConnectDatabase(string sql, Action<NpgsqlParameterCollection> onCommand = null, Func<NpgsqlDataReader, Task> onResponce = null, Func<Task> onError = null)
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(url))
        {
            Console.WriteLine("DATABASE_URLが設定されていません");
            return;
        }

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require,
            TrustServerCertificate = false
        };

        await using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        onCommand?.Invoke(cmd.Parameters);

        if (onResponce == null)
        {
            await cmd.ExecuteNonQueryAsync();
        }
        else
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                onResponce?.Invoke(reader);
            }
            else
            {
                onError?.Invoke();
            }
        }
    }

    private async Task ShowData(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?show data ".Length);
        var texts = text.Split(" ");

        if (texts.Length < 1)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        await ConnectDatabase(
            @"SELECT text FROM database WHERE id = @id",
            parameters =>
            {
                parameters.AddWithValue("id", texts[0]);
            },
            async (reader) =>
            {
                await message.Channel.SendMessageAsync($"```{reader.GetString(0)}```");
            },
            async () =>
            {
                await message.Channel.SendMessageAsync("データが見つかりませんでした。");
            });
    }

    private async Task SetData(SocketMessage message, SocketGuild guild, SocketGuildUser user)
    {
        var text = message.Content.Substring("?set data ".Length);
        var texts = text.Split(" ");
        var mainText = text.Substring(texts[0].Length + 1);

        if (texts.Length < 2)
        {
            await message.Channel.SendMessageAsync("引数が変です。");
            return;
        }

        await ConnectDatabase(
            @"INSERT INTO database (id, text)" +
            @"VALUES (@id, @text)" +
            @"ON CONFLICT (id) DO UPDATE SET text = EXCLUDED.text;",
            parameters =>
            {
                parameters.AddWithValue("id", texts[0]);
                parameters.AddWithValue("text", mainText);
            });

        await message.Channel.SendMessageAsync($"```{mainText}```");
    }

    private async Task Create()
    {
        // var createSql = @"DROP TABLE IF EXISTS user_storage CASCADE";

        //        var createSql = @"
        //CREATE TABLE user_storage (
        //  id TEXT PRIMARY KEY,
        //  copper_coin INT DEFAULT 0,
        //  silver_coin INT DEFAULT 0,
        //  gold_coin INT DEFAULT 0,
        //  holly_coin INT DEFAULT 0,
        //  equipment_list JSONB DEFAULT '{}',
        //  valuable_list JSONB DEFAULT '{}',
        //  recipe_list JSONB DEFAULT '{}',
        //  tool_list JSONB DEFAULT '{}',
        //  material_list JSONB DEFAULT '{}',
        //  farm_list JSONB DEFAULT '{}'
        //);";

var createSql = @"
CREATE TABLE character_status (
    id TEXT PRIMARY KEY,
    race TEXT DEFAULT '',
    gender TEXT DEFAULT '',
    life_path TEXT DEFAULT '',
    vit_base INT DEFAULT 0,
    vit_growth INT DEFAULT 0,
    vit_life INT DEFAULT 0,
    pow_base INT DEFAULT 0,
    pow_growth INT DEFAULT 0,
    pow_life INT DEFAULT 0,
    pow_race FLOAT DEFAULT 1.0,
    str_base INT DEFAULT 0,
    str_growth INT DEFAULT 0,
    str_life INT DEFAULT 0,
    str_race FLOAT DEFAULT 1.0,
    int_base INT DEFAULT 0,
    int_growth INT DEFAULT 0,
    int_life INT DEFAULT 0,
    int_race FLOAT DEFAULT 1.0,
    mag_base INT DEFAULT 0,
    mag_growth INT DEFAULT 0,
    mag_life INT DEFAULT 0,
    mag_race FLOAT DEFAULT 1.0,
    dex_base INT DEFAULT 0,
    dex_growth INT DEFAULT 0,
    dex_life INT DEFAULT 0,
    dex_race FLOAT DEFAULT 1.0,
    agi_base INT DEFAULT 0,
    agi_growth INT DEFAULT 0,
    agi_life INT DEFAULT 0,
    agi_race FLOAT DEFAULT 1.0,
    sns_base INT DEFAULT 0,
    sns_growth INT DEFAULT 0,
    sns_life INT DEFAULT 0,
    sns_race FLOAT DEFAULT 1.0,
    app_base INT DEFAULT 0,
    app_growth INT DEFAULT 0,
    app_life INT DEFAULT 0,
    app_race FLOAT DEFAULT 1.0,
    luk_base INT DEFAULT 0,
    luk_growth INT DEFAULT 0,
    luk_life INT DEFAULT 0,
    luk_race FLOAT DEFAULT 1.0,
    fire_base INT DEFAULT 0,
    fire_title INT DEFAULT 0,
    fire_race FLOAT DEFAULT 1.0,
    water_base INT DEFAULT 0,
    water_title INT DEFAULT 0,
    water_race FLOAT DEFAULT 1.0,
    wing_base INT DEFAULT 0,
    wing_title INT DEFAULT 0,
    wing_race FLOAT DEFAULT 1.0,
    electric_base INT DEFAULT 0,
    electric_title INT DEFAULT 0,
    electric_race FLOAT DEFAULT 1.0,
    cold_base INT DEFAULT 0,
    cold_title INT DEFAULT 0,
    cold_race FLOAT DEFAULT 1.0,
    soil_base INT DEFAULT 0,
    soil_title INT DEFAULT 0,
    soil_race FLOAT DEFAULT 1.0,
    level INT DEFAULT 0,
    exp INT DEFAULT 0,
    dat INT DEFAULT 0,
    title TEXT DEFAULT '',
    class TEXT DEFAULT '',
    applied_class TEXT DEFAULT '',
    traits TEXT DEFAULT '',
    skill TEXT DEFAULT ''
);";

        await ConnectDatabase(createSql);

    }
}
