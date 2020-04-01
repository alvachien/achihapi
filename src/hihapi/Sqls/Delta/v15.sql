-- Update account status
UPDATE [dbo].[t_fin_account]
	SET [Status] = 0
	WHERE [Status] IS NULL;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (15,'2020.4.1');

-- The end.