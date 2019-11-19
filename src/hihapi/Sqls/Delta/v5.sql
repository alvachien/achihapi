
-- Update table: t_fin_tmpdoc_loan 
IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_fin_tmpdoc_loan' AND COLUMN_NAME = 'TRANTYPE')
BEGIN

    ALTER TABLE [dbo].[t_fin_tmpdoc_loan] 
		DROP COLUMN TRANTYPE;

END;

-- Version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (5, '2018.08.02');

-- The End.