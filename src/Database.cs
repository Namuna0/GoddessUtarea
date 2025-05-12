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
            TrustServerCertificate = true
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

    //var createSql = @"CREATE TABLE IF NOT EXISTS user_status (
    //id TEXT PRIMARY KEY,
    //max_hp SMALLINT DEFAULT 0,
    //max_sp  SMALLINT DEFAULT 0,
    //max_san SMALLINT DEFAULT 0,
    //max_mp SMALLINT DEFAULT 0,
    //hp SMALLINT DEFAULT 0,
    //sp  SMALLINT DEFAULT 0,
    //san SMALLINT DEFAULT 0,
    //mp SMALLINT DEFAULT 0,
    //vit_b REAL DEFAULT 0.0,
    //pow_b REAL DEFAULT 0.0,
    //str_b REAL DEFAULT 0.0,
    //int_b REAL DEFAULT 0.0,
    //mag_b REAL DEFAULT 0.0,
    //dex_b REAL DEFAULT 0.0,
    //agi_b REAL DEFAULT 0.0,
    //sns_b REAL DEFAULT 0.0,
    //app_b REAL DEFAULT 0.0,
    //luk_b REAL DEFAULT 0.0,
    //wep_p TEXT DEFAULT '0',
    //created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP);";
}