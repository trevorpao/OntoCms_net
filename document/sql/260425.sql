-- 2026-04-25 daily SQL delivery
-- SeenTargetTaskCompletion: first live entity seen table slice for Press.

IF OBJECT_ID(N'[dbo].[tbl_press_seen]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_press_seen] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [row_id] INT NOT NULL DEFAULT 0,
    [source] NVARCHAR(32) NOT NULL DEFAULT N'',
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_press_seen_uniq_member_row] UNIQUE ([member_id],[row_id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_row_member' AND object_id = OBJECT_ID(N'[dbo].[tbl_press_seen]'))
    CREATE INDEX [idx_row_member] ON [dbo].[tbl_press_seen] ([row_id],[member_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_insert_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_press_seen]'))
    CREATE INDEX [idx_member_insert_ts] ON [dbo].[tbl_press_seen] ([member_id],[insert_ts]);

IF OBJECT_ID(N'[dbo].[tbl_post_seen]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_post_seen] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [member_id] INT NOT NULL DEFAULT 0,
    [row_id] INT NOT NULL DEFAULT 0,
    [source] NVARCHAR(32) NOT NULL DEFAULT N'',
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_post_seen_uniq_member_row] UNIQUE ([member_id],[row_id])
);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_row_member' AND object_id = OBJECT_ID(N'[dbo].[tbl_post_seen]'))
    CREATE INDEX [idx_row_member] ON [dbo].[tbl_post_seen] ([row_id],[member_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'idx_member_insert_ts' AND object_id = OBJECT_ID(N'[dbo].[tbl_post_seen]'))
    CREATE INDEX [idx_member_insert_ts] ON [dbo].[tbl_post_seen] ([member_id],[insert_ts]);
