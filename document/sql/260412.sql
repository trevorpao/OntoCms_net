-- 2026-04-12 daily SQL delivery
-- Add schema changes and DBA-executed full-table SQL for this date in this file.

-- EventRuleEngine minimal owning-module baseline

IF OBJECT_ID(N'[dbo].[tbl_member]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_member] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [display_name] NVARCHAR(255) NOT NULL DEFAULT N'',
    [email] NVARCHAR(191) NOT NULL DEFAULT N'',
    [avatar] NVARCHAR(255) NOT NULL DEFAULT N'',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_member_uniq_email] UNIQUE ([email]),
    CONSTRAINT [CK_tbl_member_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_member]'))
    CREATE INDEX [idx_status] ON [dbo].[tbl_member] ([status]);

IF OBJECT_ID(N'[dbo].[tbl_member_oauth]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_member_oauth] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [provider] NVARCHAR(32) NOT NULL DEFAULT N'',
    [provider_uid] NVARCHAR(191) NOT NULL DEFAULT N'',
    [provider_email] NVARCHAR(191) NOT NULL DEFAULT N'',
    [provider_name] NVARCHAR(191) NOT NULL DEFAULT N'',
    [provider_avatar] NVARCHAR(255) NOT NULL DEFAULT N'',
    [raw_profile] NVARCHAR(MAX) DEFAULT NULL,
    [bind_status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [last_login_ts] DATETIME2 NULL DEFAULT NULL,
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_member_oauth_uniq_provider_uid] UNIQUE ([provider],[provider_uid]),
    CONSTRAINT [CK_tbl_member_oauth_bind_status] CHECK ([bind_status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_id' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_oauth]'))
    CREATE INDEX [idx_member_id] ON [dbo].[tbl_member_oauth] ([member_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_provider_email' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_oauth]'))
    CREATE INDEX [idx_provider_email] ON [dbo].[tbl_member_oauth] ([provider],[provider_email]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_bind_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_oauth]'))
    CREATE INDEX [idx_bind_status] ON [dbo].[tbl_member_oauth] ([bind_status]);

IF OBJECT_ID(N'[dbo].[tbl_duty]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_duty] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [slug] NVARCHAR(255) NOT NULL DEFAULT N'',
    [claim] NVARCHAR(MAX) DEFAULT NULL,
    [factor] NVARCHAR(MAX) DEFAULT NULL,
    [next] NVARCHAR(MAX) DEFAULT NULL,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_duty_uniq_slug] UNIQUE ([slug]),
    CONSTRAINT [CK_tbl_duty_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_duty]'))
    CREATE INDEX [idx_status] ON [dbo].[tbl_duty] ([status]);

IF OBJECT_ID(N'[dbo].[tbl_task]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_task] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [duty_id] INT NOT NULL DEFAULT 0,
    [member_id] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(7) NOT NULL DEFAULT N'New',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_task_uniq_duty_member] UNIQUE ([duty_id],[member_id]),
    CONSTRAINT [CK_tbl_task_status] CHECK ([status] IN (N'New', N'Claimed', N'Done', N'Invalid'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_task]'))
    CREATE INDEX [idx_member_status] ON [dbo].[tbl_task] ([member_id],[status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_duty_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_task]'))
    CREATE INDEX [idx_duty_status] ON [dbo].[tbl_task] ([duty_id],[status]);

IF OBJECT_ID(N'[dbo].[tbl_member_seen]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_member_seen] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [target] NVARCHAR(32) NOT NULL DEFAULT N'',
    [row_id] INT NOT NULL DEFAULT 0,
    [source] NVARCHAR(32) NOT NULL DEFAULT N'',
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_member_seen_uniq_member_target_row] UNIQUE ([member_id],[target],[row_id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_target_row_member' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_seen]'))
    CREATE INDEX [idx_target_row_member] ON [dbo].[tbl_member_seen] ([target],[row_id],[member_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_insert_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_seen]'))
    CREATE INDEX [idx_member_insert_ts] ON [dbo].[tbl_member_seen] ([member_id],[insert_ts]);

IF OBJECT_ID(N'[dbo].[tbl_task_log]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_task_log] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL DEFAULT 0,
    [action_code] NVARCHAR(64) NOT NULL DEFAULT N'',
    [old_state_code] NVARCHAR(64) DEFAULT NULL,
    [new_state_code] NVARCHAR(64) DEFAULT NULL,
    [remark] NVARCHAR(MAX) DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_parent_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_task_log]'))
    CREATE INDEX [idx_parent_ts] ON [dbo].[tbl_task_log] ([parent_id],[insert_ts]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_action_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_task_log]'))
    CREATE INDEX [idx_action_ts] ON [dbo].[tbl_task_log] ([action_code],[insert_ts]);

IF OBJECT_ID(N'[dbo].[tbl_heraldry]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_heraldry] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [slug] NVARCHAR(255) NOT NULL DEFAULT N'',
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_heraldry_uniq_slug] UNIQUE ([slug]),
    CONSTRAINT [CK_tbl_heraldry_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_status' AND object_id = OBJECT_ID(N'[dbo].[tbl_heraldry]'))
    CREATE INDEX [idx_status] ON [dbo].[tbl_heraldry] ([status]);

IF OBJECT_ID(N'[dbo].[tbl_member_heraldry]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_member_heraldry] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [heraldry_id] INT NOT NULL DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_member_heraldry_uniq_member_heraldry] UNIQUE ([member_id],[heraldry_id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_heraldry_member' AND object_id = OBJECT_ID(N'[dbo].[tbl_member_heraldry]'))
    CREATE INDEX [idx_heraldry_member] ON [dbo].[tbl_member_heraldry] ([heraldry_id],[member_id]);

IF OBJECT_ID(N'[dbo].[tbl_manaccount]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_manaccount] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [balance] INT NOT NULL DEFAULT 0,
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Enabled',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [last_user] INT DEFAULT 0,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_manaccount_uniq_member] UNIQUE ([member_id]),
    CONSTRAINT [CK_tbl_manaccount_status] CHECK ([status] IN (N'Enabled', N'Disabled'))
);
END;

IF OBJECT_ID(N'[dbo].[tbl_manaccount_log]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_manaccount_log] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [parent_id] INT NOT NULL DEFAULT 0,
    [action_code] NVARCHAR(64) NOT NULL DEFAULT N'',
    [delta_point] INT NOT NULL DEFAULT 0,
    [old_balance] INT NOT NULL DEFAULT 0,
    [new_balance] INT NOT NULL DEFAULT 0,
    [remark] NVARCHAR(MAX) DEFAULT NULL,
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_parent_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_manaccount_log]'))
    CREATE INDEX [idx_parent_ts] ON [dbo].[tbl_manaccount_log] ([parent_id],[insert_ts]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_action_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_manaccount_log]'))
    CREATE INDEX [idx_action_ts] ON [dbo].[tbl_manaccount_log] ([action_code],[insert_ts]);
