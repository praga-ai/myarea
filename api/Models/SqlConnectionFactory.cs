using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;

namespace MobileApp.Api.Models;

// Retrieves the connection string from Key Vault once per cold start, then reuses it.
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly SecretClient _kv;
    private readonly string _secretName;
    private string? _connectionString;

    public SqlConnectionFactory(SecretClient kv)
    {
        _kv = kv;
        _secretName = Environment.GetEnvironmentVariable("SqlConnectionStringSecretName")
            ?? "SqlConnectionString";
    }

    public async Task<SqlConnection> OpenAsync(CancellationToken ct = default)
    {
        _connectionString ??= (await _kv.GetSecretAsync(_secretName, cancellationToken: ct)).Value.Value;
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
