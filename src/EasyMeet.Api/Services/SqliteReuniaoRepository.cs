using System.Data;
using System.Globalization;
using EasyMeet.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EasyMeet.Api.Services;

public sealed class SqliteReuniaoRepository : IReuniaoRepository
{
    private readonly string _connectionString;

    public SqliteReuniaoRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("EasyMeet")
            ?? "Data Source=App_Data/easymeet.db";

        var builder = new SqliteConnectionStringBuilder(configuredConnectionString);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = builder.ToString();
    }

    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Reuniao (
                ID INTEGER NOT NULL CONSTRAINT PK_Reuniao PRIMARY KEY AUTOINCREMENT,
                Transcricao TEXT NOT NULL,
                Resumo TEXT NOT NULL,
                Confianca DECIMAL NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> AdicionarAsync(Reuniao reuniao, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Reuniao (Transcricao, Resumo, Confianca)
            VALUES ($transcricao, $resumo, $confianca);

            SELECT last_insert_rowid();
            """;
        command.Parameters.Add(CreateParameter(command, "$transcricao", reuniao.Transcricao));
        command.Parameters.Add(CreateParameter(command, "$resumo", reuniao.Resumo));
        command.Parameters.Add(CreateParameter(command, "$confianca", reuniao.Confianca, DbType.Decimal));

        var id = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(id, CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<Reuniao>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ID, Transcricao, Resumo, Confianca
            FROM Reuniao
            ORDER BY ID DESC;
            """;

        var reunioes = new List<Reuniao>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reunioes.Add(new Reuniao
            {
                Id = reader.GetInt32(0),
                Transcricao = reader.GetString(1),
                Resumo = reader.GetString(2),
                Confianca = ReadDecimal(reader.GetValue(3))
            });
        }

        return reunioes;
    }

    private static SqliteParameter CreateParameter(
        SqliteCommand command,
        string name,
        object value,
        DbType? dbType = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        if (dbType.HasValue)
        {
            parameter.DbType = dbType.Value;
        }

        return parameter;
    }

    private static decimal ReadDecimal(object value)
    {
        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue),
            float floatValue => Convert.ToDecimal(floatValue),
            long longValue => longValue,
            int intValue => intValue,
            string text when decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0m
        };
    }
}
