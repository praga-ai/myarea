using Microsoft.Data.SqlClient;

namespace MobileApp.Api.Models;

public interface IDbConnectionFactory
{
    Task<SqlConnection> OpenAsync(CancellationToken ct = default);
}
