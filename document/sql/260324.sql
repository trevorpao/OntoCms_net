-- 2026-03-24 conversation mapping

DROP TABLE IF EXISTS [dbo].[tbl_conversation];
IF OBJECT_ID(N'[dbo].[tbl_conversation]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[tbl_conversation] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [user_id] NVARCHAR(64) NOT NULL DEFAULT N'',
    [thread_id] NVARCHAR(96) NOT NULL DEFAULT N'',
    [model] NVARCHAR(64) NOT NULL DEFAULT N'',
    [status] NVARCHAR(8) NOT NULL DEFAULT N'Active',
    [last_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_ts] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [insert_user] INT DEFAULT 0,
    [last_user] INT DEFAULT 0,
    PRIMARY KEY ([id]),
    CONSTRAINT [UQ_tbl_conversation_uniq_conversation_user] UNIQUE ([user_id]),
    CONSTRAINT [UQ_tbl_conversation_uniq_conversation_thread] UNIQUE ([thread_id]),
    CONSTRAINT [CK_tbl_conversation_status] CHECK ([status] IN (N'Active', N'Archived'))
);
END;
