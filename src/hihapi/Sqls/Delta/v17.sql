
-- Update T_HOMEMEM by adding child flag
IF NOT EXISTS( select * from INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'T_HOMEMEM'
	AND COLUMN_NAME = 'ISCHILD' )
BEGIN
	ALTER TABLE dbo.t_homemem
	 ADD ISCHILD bit NULL;
END;


-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (17,'2020.09.12');

-- The end.
