-- 2026-05-20 daily SQL delivery
-- SMSSystem first implementation slice: Mobile / Phonebook / Campaign schema.

IF OBJECT_ID(N'[dbo].[tbl_mobile]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_mobile] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [phone_number] NVARCHAR(32) NOT NULL DEFAULT N'',
    [status] NVARCHAR(7) NOT NULL DEFAULT N'Active',
    [last_sent_ts] DATETIME2 DEFAULT NULL,
    [last_ts] DATETIME2 DEFAULT NULL,
    [last_user] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 DEFAULT NULL,
    [insert_user] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_mobile_uniq_phone_number] UNIQUE ([phone_number]),
    CONSTRAINT [CK_tbl_mobile_status] CHECK ([status] IN (N'Active', N'Invalid', N'Opt-out'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_mobile]'))
    CREATE INDEX [idx_status] ON [dbo].[tbl_mobile] ([status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_last_sent_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_mobile]'))
    CREATE INDEX [idx_last_sent_ts] ON [dbo].[tbl_mobile] ([last_sent_ts]);

IF OBJECT_ID(N'[dbo].[tbl_phonebook]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_phonebook] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [title] NVARCHAR(191) NOT NULL DEFAULT N'',
    [remark] NVARCHAR(255) NOT NULL DEFAULT N'',
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [last_ts] DATETIME2 DEFAULT NULL,
    [last_user] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 DEFAULT NULL,
    [insert_user] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_phonebook_uniq_member_title] UNIQUE ([member_id],[title]),
    CONSTRAINT [CK_tbl_phonebook_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_phonebook]'))
    CREATE INDEX [idx_status] ON [dbo].[tbl_phonebook] ([status]);

IF OBJECT_ID(N'[dbo].[tbl_phonebook_mobile]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_phonebook_mobile] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [phonebook_id] INT NOT NULL DEFAULT 0,
    [mobile_id] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 DEFAULT NULL,
    [insert_user] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_phonebook_mobile_uniq_phonebook_mobile] UNIQUE ([phonebook_id],[mobile_id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_mobile_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_phonebook_mobile]'))
    CREATE INDEX [idx_mobile_id] ON [dbo].[tbl_phonebook_mobile] ([mobile_id]);

IF OBJECT_ID(N'[dbo].[tbl_campaign]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_campaign] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [phonebook_id] INT NOT NULL DEFAULT 0,
    [provider_policy] NVARCHAR(64) NOT NULL DEFAULT N'TW_TO_MITAKE_ELSE_AWS',
    [content] NVARCHAR(MAX) DEFAULT NULL,
    [scheduled_ts] DATETIME2 DEFAULT NULL,
    [status] NVARCHAR(15) NOT NULL DEFAULT N'Draft',
    [total_targets] INT NOT NULL DEFAULT 0,
    [sent_count] INT NOT NULL DEFAULT 0,
    [failed_count] INT NOT NULL DEFAULT 0,
    [last_ts] DATETIME2 DEFAULT NULL,
    [last_user] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 DEFAULT NULL,
    [insert_user] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [CK_tbl_campaign_status] CHECK ([status] IN (N'Draft', N'Queued', N'Processing', N'Completed', N'PartiallyFailed', N'Failed'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign]'))
    CREATE INDEX [idx_member_id] ON [dbo].[tbl_campaign] ([member_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_phonebook_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign]'))
    CREATE INDEX [idx_phonebook_id] ON [dbo].[tbl_campaign] ([phonebook_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status_scheduled_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign]'))
    CREATE INDEX [idx_status_scheduled_ts] ON [dbo].[tbl_campaign] ([status],[scheduled_ts]);

IF OBJECT_ID(N'[dbo].[tbl_campaign_log]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_campaign_log] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [campaign_id] INT NOT NULL DEFAULT 0,
    [member_id] INT NOT NULL DEFAULT 0,
    [phonebook_id] INT NOT NULL DEFAULT 0,
    [mobile_id] INT NOT NULL DEFAULT 0,
    [provider_alias] NVARCHAR(32) NOT NULL DEFAULT N'',
    [status] NVARCHAR(7) NOT NULL DEFAULT N'Pending',
    [error_message] NVARCHAR(255) DEFAULT NULL,
    [provider_message_id] NVARCHAR(191) DEFAULT NULL,
    [scheduled_ts] DATETIME2 DEFAULT NULL,
    [sent_ts] DATETIME2 DEFAULT NULL,
    [attempt_ts] DATETIME2 DEFAULT NULL,
    [last_ts] DATETIME2 DEFAULT NULL,
    [last_user] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 DEFAULT NULL,
    [insert_user] INT NOT NULL DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_campaign_log_uniq_campaign_mobile] UNIQUE ([campaign_id],[mobile_id]),
    CONSTRAINT [CK_tbl_campaign_log_status] CHECK ([status] IN (N'Pending', N'Sent', N'Failed', N'Skipped'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status_scheduled_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign_log]'))
    CREATE INDEX [idx_status_scheduled_ts] ON [dbo].[tbl_campaign_log] ([status],[scheduled_ts]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_mobile_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign_log]'))
    CREATE INDEX [idx_mobile_id] ON [dbo].[tbl_campaign_log] ([mobile_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_mobile_status_sent_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign_log]'))
    CREATE INDEX [idx_mobile_status_sent_ts] ON [dbo].[tbl_campaign_log] ([mobile_id],[status],[sent_ts]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_provider_alias' AND object_id = OBJECT_ID(N'[dbo].[tbl_campaign_log]'))
    CREATE INDEX [idx_provider_alias] ON [dbo].[tbl_campaign_log] ([provider_alias]);
