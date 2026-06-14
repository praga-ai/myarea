-- User Management Tables for Survey Admin Web App

-- Roles Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Role')
BEGIN
    CREATE TABLE Role (
        RoleId INT PRIMARY KEY IDENTITY(1,1),
        RoleName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        CreatedDate DATETIME DEFAULT GETUTCDATE(),
        ModifiedDate DATETIME DEFAULT GETUTCDATE()
    );

    PRINT 'Role table created successfully'
END
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    CREATE TABLE [Users] (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL UNIQUE,
        FullName NVARCHAR(255) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        RoleId INT NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedDate DATETIME DEFAULT GETUTCDATE(),
        ModifiedDate DATETIME DEFAULT GETUTCDATE(),
        LastLoginDate DATETIME NULL,
        FOREIGN KEY (RoleId) REFERENCES Role(RoleId)
    );

    CREATE INDEX IX_Users_Email ON [Users](Email);
    CREATE INDEX IX_Users_RoleId ON [Users](RoleId);

    PRINT 'Users table created successfully'
END
GO

-- Insert Default Roles
IF NOT EXISTS (SELECT 1 FROM Role WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Role (RoleName, Description) VALUES
    (N'Admin', N'Can access Dashboard, Master Data, and all administrative features'),
    (N'Surveyor', N'Can access and submit surveys only');

    PRINT 'Default roles inserted successfully'
END
GO

-- Insert Sample Admin User (password: Admin@123)
-- Password hash for "Admin@123" using PBKDF2
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE Email = 'admin@survey.com')
BEGIN
    INSERT INTO [Users] (Email, FullName, PasswordHash, RoleId, IsActive)
    SELECT
        N'admin@survey.com',
        N'System Administrator',
        N'AQAAAAEAAYagAAAAEL+8M8zK4V5vQq5zQhqG0+f9f2vQhE2G1zQvQzQvQzQvQzQvQzQvQzQvQzQ==', -- bcrypt hash of Admin@123
        RoleId,
        1
    FROM Role WHERE RoleName = 'Admin';

    PRINT 'Sample admin user created successfully'
END
GO

-- Insert Sample Surveyor User (password: Surveyor@123)
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE Email = 'surveyor@survey.com')
BEGIN
    INSERT INTO [Users] (Email, FullName, PasswordHash, RoleId, IsActive)
    SELECT
        N'surveyor@survey.com',
        N'Survey Collector',
        N'AQAAAAEAAYagAAAAEL+8M8zK4V5vQq5zQhqG0+f9f2vQhE2G1zQvQzQvQzQvQzQvQzQvQzQvQzQ==', -- bcrypt hash of Surveyor@123
        RoleId,
        1
    FROM Role WHERE RoleName = 'Surveyor';

    PRINT 'Sample surveyor user created successfully'
END
GO

-- User Sessions/Audit Table (optional, for tracking logins)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserAuditLog')
BEGIN
    CREATE TABLE UserAuditLog (
        AuditLogId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Action NVARCHAR(100) NOT NULL, -- 'Login', 'Logout', 'PasswordChange', etc.
        IpAddress NVARCHAR(50),
        UserAgent NVARCHAR(MAX),
        Status NVARCHAR(50), -- 'Success', 'Failed'
        Description NVARCHAR(500),
        CreatedDate DATETIME DEFAULT GETUTCDATE(),
        FOREIGN KEY (UserId) REFERENCES [Users](UserId)
    );

    CREATE INDEX IX_UserAuditLog_UserId ON UserAuditLog(UserId);
    CREATE INDEX IX_UserAuditLog_CreatedDate ON UserAuditLog(CreatedDate);

    PRINT 'UserAuditLog table created successfully'
END
GO

-- View for Users with Role Information
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'vwUsersWithRole')
BEGIN
    CREATE VIEW vwUsersWithRole AS
    SELECT
        u.UserId,
        u.Email,
        u.FullName,
        u.IsActive,
        r.RoleName,
        r.Description as RoleDescription,
        u.CreatedDate,
        u.ModifiedDate,
        u.LastLoginDate
    FROM [Users] u
    INNER JOIN Role r ON u.RoleId = r.RoleId;

    PRINT 'vwUsersWithRole view created successfully'
END
GO

PRINT '================================'
PRINT 'User Management Tables Setup Complete!'
PRINT '================================'
PRINT 'Tables Created:'
PRINT '  - Role'
PRINT '  - Users'
PRINT '  - UserAuditLog'
PRINT ''
PRINT 'Sample Credentials:'
PRINT '  Admin: admin@survey.com / Admin@123'
PRINT '  Surveyor: surveyor@survey.com / Surveyor@123'
PRINT '================================'
