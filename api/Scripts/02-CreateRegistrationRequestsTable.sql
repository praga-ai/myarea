-- Create RegistrationRequests table for email verification workflow

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistrationRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegistrationRequests] (
        [RegistrationId] NVARCHAR(36) NOT NULL PRIMARY KEY,
        [Email] NVARCHAR(255) NOT NULL UNIQUE,
        [FullName] NVARCHAR(255) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [RoleId] INT NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending-verification',
        [VerificationCode] NVARCHAR(10) NULL,
        [VerificationCodeExpiry] DATETIME NULL,
        [EmailVerifiedAt] DATETIME NULL,
        [ApprovedAt] DATETIME NULL,
        [ApprovedBy] NVARCHAR(MAX) NULL,
        [RejectedAt] DATETIME NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),

        -- Foreign key to Role table
        CONSTRAINT FK_RegistrationRequests_Role FOREIGN KEY ([RoleId])
            REFERENCES [dbo].[Role]([RoleId])
    );

    -- Create indexes for better query performance
    CREATE INDEX IX_RegistrationRequests_Email ON [dbo].[RegistrationRequests]([Email]);
    CREATE INDEX IX_RegistrationRequests_Status ON [dbo].[RegistrationRequests]([Status]);
    CREATE INDEX IX_RegistrationRequests_CreatedAt ON [dbo].[RegistrationRequests]([CreatedAt]);

    PRINT 'RegistrationRequests table created successfully';
END
ELSE
BEGIN
    PRINT 'RegistrationRequests table already exists';
END;

-- Verify the table was created
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistrationRequests]') AND type in (N'U'))
BEGIN
    SELECT 'RegistrationRequests table created successfully' AS [Status];
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RegistrationRequests';
END;
