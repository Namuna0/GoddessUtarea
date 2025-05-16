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

        var createSql = @"DROP TABLE user_storage;";

        await ConnectDatabase(createSql);

    }
}
