SET NOCOUNT ON;

DECLARE @AboutPostId INT = (
    SELECT TOP (1) [id]
    FROM [dbo].[tbl_post]
    WHERE [slug] = N'about'
    ORDER BY [id]
);

IF @AboutPostId IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_post]
    SET [status] = N'Enabled',
        [layout] = N'normal',
        [cover] = N'',
        [last_ts] = SYSUTCDATETIME(),
        [last_user] = 1
    WHERE [id] = @AboutPostId;

    MERGE [dbo].[tbl_post_lang] AS target
    USING (
        VALUES
            (N'tw', N'關於我們', N'<p>我們用 Post 來承接首頁內容，保留 CMS 對內容編修與多語系的既有路徑。</p><p>這裡先提供中文與英文兩種語系，讓首頁能直接作為 About Us 的內容入口。</p>'),
            (N'en', N'About Us', N'<p>This home page is now driven by Post content so the CMS can keep using the same editing and multilingual path.</p><p>The first slice provides both Traditional Chinese and English, turning the home page into a focused About Us entry.</p>')
    ) AS source ([lang], [title], [content])
        ON target.[parent_id] = @AboutPostId
       AND target.[lang] = source.[lang]
    WHEN MATCHED THEN
        UPDATE SET target.[from_ai] = N'No',
                   target.[title] = source.[title],
                   target.[content] = source.[content],
                   target.[last_ts] = SYSUTCDATETIME(),
                   target.[last_user] = 1
    WHEN NOT MATCHED THEN
        INSERT ([from_ai], [lang], [parent_id], [title], [content], [last_ts], [last_user], [insert_ts], [insert_user])
        VALUES (N'No', source.[lang], @AboutPostId, source.[title], source.[content], SYSUTCDATETIME(), 1, SYSUTCDATETIME(), 1);
END;