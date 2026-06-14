using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using MobileApp.Api.Models;

namespace MobileApp.Api.Services;

public class AuthService
{
    private readonly string _connectionString;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public AuthService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Hash password using PBKDF2
    public string HashPassword(string password)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var saltBytes = new byte[SaltSize];
            rng.GetBytes(saltBytes);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(HashSize);
                var hashBytes = new byte[SaltSize + HashSize];

                Array.Copy(saltBytes, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    // Verify password
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            var saltBytes = new byte[SaltSize];
            Array.Copy(hashBytes, 0, saltBytes, 0, SaltSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                var computedHash = pbkdf2.GetBytes(HashSize);

                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != computedHash[i])
                        return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Register new user
    public async Task<(bool success, string message, UserDto? user)> RegisterAsync(RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "Email is required", null);

        if (string.IsNullOrWhiteSpace(request.FullName))
            return (false, "Full name is required", null);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return (false, "Password must be at least 8 characters", null);

        if (request.Password != request.ConfirmPassword)
            return (false, "Passwords do not match", null);

        // Email format validation
        try
        {
            var addr = new System.Net.Mail.MailAddress(request.Email);
        }
        catch
        {
            return (false, "Invalid email format", null);
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if email already exists
            using (var command = new SqlCommand("SELECT UserId FROM [Users] WHERE Email = @email", connection))
            {
                command.Parameters.AddWithValue("@email", request.Email.ToLower());
                var result = await command.ExecuteScalarAsync();
                if (result != null)
                    return (false, "Email already registered", null);
            }

            // Verify role exists
            using (var command = new SqlCommand("SELECT RoleId FROM Role WHERE RoleId = @roleId", connection))
            {
                command.Parameters.AddWithValue("@roleId", request.RoleId);
                var result = await command.ExecuteScalarAsync();
                if (result == null)
                    return (false, "Invalid role selected", null);
            }

            // Create new user
            var passwordHash = HashPassword(request.Password);

            using (var command = new SqlCommand(
                @"INSERT INTO [Users] (Email, FullName, PasswordHash, RoleId, IsActive)
                  VALUES (@email, @fullName, @passwordHash, @roleId, 1);
                  SELECT SCOPE_IDENTITY();",
                connection))
            {
                command.Parameters.AddWithValue("@email", request.Email.ToLower());
                command.Parameters.AddWithValue("@fullName", request.FullName);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@roleId", request.RoleId);

                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                // Get the created user with role info
                var user = await GetUserByIdAsync(userId);
                if (user != null)
                {
                    return (true, "User registered successfully", user);
                }
            }
        }

        return (false, "Failed to create user", null);
    }

    // Login user
    public async Task<(bool success, string message, string token, UserDto? user)> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (false, "Email and password are required", string.Empty, null);

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Get user with role
            using (var command = new SqlCommand(
                @"SELECT u.UserId, u.Email, u.FullName, u.PasswordHash, u.IsActive, r.RoleName
                  FROM [Users] u
                  INNER JOIN Role r ON u.RoleId = r.RoleId
                  WHERE u.Email = @email",
                connection))
            {
                command.Parameters.AddWithValue("@email", request.Email.ToLower());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var userId = (int)reader["UserId"];
                        var email = reader["Email"].ToString() ?? string.Empty;
                        var fullName = reader["FullName"].ToString() ?? string.Empty;
                        var passwordHash = reader["PasswordHash"].ToString() ?? string.Empty;
                        var isActive = (bool)reader["IsActive"];
                        var roleName = reader["RoleName"].ToString() ?? string.Empty;

                        if (!isActive)
                        {
                            await LogAuditAsync(userId, "Login", "Failed", "Account inactive", ipAddress, userAgent);
                            return (false, "User account is inactive", string.Empty, null);
                        }

                        // Verify password
                        if (!VerifyPassword(request.Password, passwordHash))
                        {
                            await LogAuditAsync(userId, "Login", "Failed", "Invalid password", ipAddress, userAgent);
                            return (false, "Invalid email or password", string.Empty, null);
                        }

                        // Update last login
                        await UpdateLastLoginAsync(userId);

                        // Audit successful login
                        await LogAuditAsync(userId, "Login", "Success", "Login successful", ipAddress, userAgent);

                        // Generate token
                        var token = GenerateJwtToken(userId, email, roleName);

                        var user = new UserDto
                        {
                            UserId = userId,
                            Email = email,
                            FullName = fullName,
                            RoleName = roleName,
                            IsActive = isActive,
                            LastLoginDate = DateTime.UtcNow
                        };

                        return (true, "Login successful", token, user);
                    }
                }
            }
        }

        return (false, "Invalid email or password", string.Empty, null);
    }

    // Generate simple JWT token
    public string GenerateJwtToken(int userId, string email, string roleName)
    {
        var header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
        var payload = JsonSerializer.Serialize(new
        {
            sub = userId.ToString(),
            email = email,
            role = roleName,
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()
        });

        var headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));

        var signature = GenerateSignature($"{headerEncoded}.{payloadEncoded}");

        return $"{headerEncoded}.{payloadEncoded}.{signature}";
    }

    // Verify JWT token
    public (bool valid, int userId, string email, string role) VerifyToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return (false, 0, string.Empty, string.Empty);

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using (var doc = JsonDocument.Parse(payloadJson))
            {
                var root = doc.RootElement;

                if (root.TryGetProperty("exp", out var expProp) &&
                    expProp.GetInt64() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return (false, 0, string.Empty, string.Empty);
                }

                var userId = int.Parse(root.GetProperty("sub").GetString() ?? "0");
                var email = root.GetProperty("email").GetString() ?? string.Empty;
                var role = root.GetProperty("role").GetString() ?? string.Empty;

                return (true, userId, email, role);
            }
        }
        catch
        {
            return (false, 0, string.Empty, string.Empty);
        }
    }

    // Get user by ID
    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                @"SELECT u.UserId, u.Email, u.FullName, u.IsActive, r.RoleName, u.LastLoginDate
                  FROM [Users] u
                  INNER JOIN Role r ON u.RoleId = r.RoleId
                  WHERE u.UserId = @userId",
                connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserDto
                        {
                            UserId = (int)reader["UserId"],
                            Email = reader["Email"].ToString() ?? string.Empty,
                            FullName = reader["FullName"].ToString() ?? string.Empty,
                            RoleName = reader["RoleName"].ToString() ?? string.Empty,
                            IsActive = (bool)reader["IsActive"],
                            LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                ? (DateTime)reader["LastLoginDate"]
                                : DateTime.MinValue
                        };
                    }
                }
            }
        }

        return null;
    }

    // Update last login timestamp
    private async Task UpdateLastLoginAsync(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                "UPDATE [Users] SET LastLoginDate = GETUTCDATE() WHERE UserId = @userId",
                connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    // Write an entry to UserAuditLog. Best-effort: never throws, so it can't break the calling operation.
    public async Task LogAuditAsync(int userId, string action, string status,
        string? description = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"INSERT INTO UserAuditLog (UserId, Action, IpAddress, UserAgent, Status, Description)
                      VALUES (@userId, @action, @ipAddress, @userAgent, @status, @description)",
                    connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@action", Trunc(action, 100) ?? string.Empty);
                    command.Parameters.AddWithValue("@ipAddress", (object?)Trunc(ipAddress, 50) ?? DBNull.Value);
                    command.Parameters.AddWithValue("@userAgent", (object?)userAgent ?? DBNull.Value);
                    command.Parameters.AddWithValue("@status", Trunc(status, 50) ?? string.Empty);
                    command.Parameters.AddWithValue("@description", (object?)Trunc(description, 500) ?? DBNull.Value);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // Audit logging must never break the primary operation (login, approval, etc.)
            Console.WriteLine($"[Audit] Failed to write UserAuditLog: {ex.Message}");
        }
    }

    private static string? Trunc(string? s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length > max ? s.Substring(0, max) : s);

    // Helper methods for token encoding
    private string GenerateSignature(string message)
    {
        var key = Encoding.UTF8.GetBytes("your-secret-key-change-this-in-production");
        using (var hmac = new System.Security.Cryptography.HMACSHA256(key))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Base64UrlEncode(hash);
        }
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return output;
    }

    private byte[] Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2: output += "=="; break;
            case 3: output += "="; break;
        }
        return Convert.FromBase64String(output);
    }

    // Generate verification code
    public string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    // Register request (email verification pending)
    public async Task<(bool success, string message, string registrationId)> RegisterRequestAsync(RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "Email is required", string.Empty);

        if (string.IsNullOrWhiteSpace(request.FullName))
            return (false, "Full name is required", string.Empty);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return (false, "Password must be at least 8 characters", string.Empty);

        if (request.Password != request.ConfirmPassword)
            return (false, "Passwords do not match", string.Empty);

        // Email format validation
        try
        {
            var addr = new System.Net.Mail.MailAddress(request.Email);
        }
        catch
        {
            return (false, "Invalid email format", string.Empty);
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if email already exists in Users or RegistrationRequests
            using (var command = new SqlCommand(
                @"SELECT UserId FROM [Users] WHERE Email = @email
                  UNION
                  SELECT RegistrationId FROM RegistrationRequests WHERE Email = @email AND Status IN ('pending-verification', 'email-verified')",
                connection))
            {
                command.Parameters.AddWithValue("@email", request.Email.ToLower());
                var result = await command.ExecuteScalarAsync();
                if (result != null)
                    return (false, "Email already registered or pending verification", string.Empty);
            }

            // Verify role exists
            using (var command = new SqlCommand("SELECT RoleId FROM Role WHERE RoleId = @roleId", connection))
            {
                command.Parameters.AddWithValue("@roleId", request.RoleId);
                var result = await command.ExecuteScalarAsync();
                if (result == null)
                    return (false, "Invalid role selected", string.Empty);
            }

            // Create registration request
            var passwordHash = HashPassword(request.Password);
            var verificationCode = GenerateVerificationCode();
            var registrationId = Guid.NewGuid().ToString();

            using (var command = new SqlCommand(
                @"INSERT INTO RegistrationRequests (RegistrationId, Email, FullName, PasswordHash, RoleId, Status, VerificationCode, VerificationCodeExpiry, CreatedAt)
                  VALUES (@registrationId, @email, @fullName, @passwordHash, @roleId, 'pending-verification', @verificationCode, @verificationCodeExpiry, GETUTCDATE());
                  SELECT RegistrationId FROM RegistrationRequests WHERE RegistrationId = @registrationId",
                connection))
            {
                command.Parameters.AddWithValue("@registrationId", registrationId);
                command.Parameters.AddWithValue("@email", request.Email.ToLower());
                command.Parameters.AddWithValue("@fullName", request.FullName);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@roleId", request.RoleId);
                command.Parameters.AddWithValue("@verificationCode", verificationCode);
                command.Parameters.AddWithValue("@verificationCodeExpiry", DateTime.UtcNow.AddHours(24));

                await command.ExecuteScalarAsync();
            }

            return (true, "Registration request submitted. Verification code sent to email.", registrationId);
        }
    }

    // Verify email
    public async Task<(bool success, string message)> VerifyEmailAsync(string email, string verificationCode)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(verificationCode))
            return (false, "Email and verification code are required");

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check registration request
            using (var command = new SqlCommand(
                @"SELECT RegistrationId, Status, VerificationCode, VerificationCodeExpiry
                  FROM RegistrationRequests
                  WHERE Email = @email AND Status IN ('pending-verification', 'email-verified')",
                connection))
            {
                command.Parameters.AddWithValue("@email", email.ToLower());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var registrationId = reader["RegistrationId"].ToString() ?? string.Empty;
                        var status = reader["Status"].ToString() ?? string.Empty;
                        var storedCode = reader["VerificationCode"].ToString() ?? string.Empty;
                        var expiry = (DateTime)reader["VerificationCodeExpiry"];

                        // Check expiry
                        if (DateTime.UtcNow > expiry)
                            return (false, "Verification code has expired");

                        // Check code
                        if (storedCode != verificationCode)
                            return (false, "Invalid verification code");

                        // Update status
                        using (var updateCommand = new SqlCommand(
                            @"UPDATE RegistrationRequests
                              SET Status = 'email-verified', EmailVerifiedAt = GETUTCDATE()
                              WHERE RegistrationId = @registrationId",
                            connection))
                        {
                            updateCommand.Parameters.AddWithValue("@registrationId", registrationId);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        return (true, "Email verified successfully");
                    }
                }
            }
        }

        return (false, "Registration request not found");
    }

    // Get pending registrations (email-verified but not approved)
    public async Task<List<RegistrationRequestDto>> GetPendingRegistrationsAsync()
    {
        var registrations = new List<RegistrationRequestDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                @"SELECT r.RegistrationId, r.Email, r.FullName, r.RoleId, ro.RoleName, r.CreatedAt, r.EmailVerifiedAt, r.Status
                  FROM RegistrationRequests r
                  INNER JOIN Role ro ON r.RoleId = ro.RoleId
                  WHERE r.Status = 'email-verified'
                  ORDER BY r.EmailVerifiedAt DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        registrations.Add(new RegistrationRequestDto
                        {
                            Email = reader["Email"].ToString() ?? string.Empty,
                            FullName = reader["FullName"].ToString() ?? string.Empty,
                            RoleId = (int)reader["RoleId"],
                            RoleName = reader["RoleName"].ToString() ?? string.Empty,
                            CreatedAt = (DateTime)reader["CreatedAt"],
                            EmailVerified = true,
                            VerifiedAt = reader["EmailVerifiedAt"] != DBNull.Value ? (DateTime)reader["EmailVerifiedAt"] : (DateTime?)null
                        });
                    }
                }
            }
        }

        return registrations;
    }

    // Approve registration and create user
    public async Task<(bool success, string message, UserDto? user)> ApproveRegistrationAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required", null);

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Get registration request
            int registrationRoleId = 0;
            string passwordHash = string.Empty;
            string fullName = string.Empty;

            using (var command = new SqlCommand(
                @"SELECT RegistrationId, FullName, RoleId, PasswordHash, Status
                  FROM RegistrationRequests
                  WHERE Email = @email AND Status = 'email-verified'",
                connection))
            {
                command.Parameters.AddWithValue("@email", email.ToLower());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        registrationRoleId = (int)reader["RoleId"];
                        passwordHash = reader["PasswordHash"].ToString() ?? string.Empty;
                        fullName = reader["FullName"].ToString() ?? string.Empty;
                    }
                    else
                    {
                        return (false, "Registration request not found or not email-verified", null);
                    }
                }
            }

            // Create user account
            int userId = 0;
            using (var command = new SqlCommand(
                @"INSERT INTO [Users] (Email, FullName, PasswordHash, RoleId, IsActive, CreatedDate)
                  VALUES (@email, @fullName, @passwordHash, @roleId, 1, GETUTCDATE());
                  SELECT SCOPE_IDENTITY();",
                connection))
            {
                command.Parameters.AddWithValue("@email", email.ToLower());
                command.Parameters.AddWithValue("@fullName", fullName);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@roleId", registrationRoleId);

                userId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            // Update registration status
            using (var command = new SqlCommand(
                @"UPDATE RegistrationRequests
                  SET Status = 'approved', ApprovedAt = GETUTCDATE(), ApprovedBy = @approvedBy
                  WHERE Email = @email",
                connection))
            {
                command.Parameters.AddWithValue("@email", email.ToLower());
                command.Parameters.AddWithValue("@approvedBy", "system-admin");
                await command.ExecuteNonQueryAsync();
            }

            // Audit the account creation
            await LogAuditAsync(userId, "AccountApproved", "Success", "Account created via registration approval");

            // Get the created user
            var user = await GetUserByIdAsync(userId);
            return (true, "Registration approved. User account created.", user);
        }
    }

    // Reject registration
    public async Task<(bool success, string message)> RejectRegistrationAsync(string email, string rejectionReason = "")
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(
                @"UPDATE RegistrationRequests
                  SET Status = 'rejected', RejectedAt = GETUTCDATE(), RejectionReason = @rejectionReason
                  WHERE Email = @email AND Status = 'email-verified'",
                connection))
            {
                command.Parameters.AddWithValue("@email", email.ToLower());
                command.Parameters.AddWithValue("@rejectionReason", rejectionReason ?? string.Empty);
                var result = await command.ExecuteNonQueryAsync();

                if (result == 0)
                    return (false, "Registration request not found or not email-verified");
            }
        }

        return (true, "Registration rejected");
    }

    // Get all users (Admin only)
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = new List<UserDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                @"SELECT u.UserId, u.Email, u.FullName, u.IsActive, r.RoleName, u.LastLoginDate
                  FROM [Users] u
                  INNER JOIN Role r ON u.RoleId = r.RoleId
                  ORDER BY u.CreatedDate DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new UserDto
                        {
                            UserId = (int)reader["UserId"],
                            Email = reader["Email"].ToString() ?? string.Empty,
                            FullName = reader["FullName"].ToString() ?? string.Empty,
                            RoleName = reader["RoleName"].ToString() ?? string.Empty,
                            IsActive = (bool)reader["IsActive"],
                            LastLoginDate = reader["LastLoginDate"] != DBNull.Value
                                ? (DateTime)reader["LastLoginDate"]
                                : DateTime.MinValue
                        });
                    }
                }
            }
        }

        return users;
    }
}
