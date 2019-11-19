-- Update table t_fin_account_ext_loan
IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 't_fin_account_ext_loan' AND COLUMN_NAME = 'IsLendOut')
BEGIN

	DECLARE @cname nvarchar(50);

    SELECT @cname = d.name
		FROM sys.default_constraints AS d
			INNER JOIN sys.columns AS c
			ON d.parent_column_id = c.column_id
			WHERE d.parent_object_id = OBJECT_ID(N't_fin_account_ext_loan', N'U')
				AND c.name = 'IsLendOut';
	
	EXEC('ALTER TABLE [dbo].[t_fin_account_ext_loan] DROP CONSTRAINT ' + @cname);
	EXEC('ALTER TABLE [dbo].[t_fin_account_ext_loan] DROP COLUMN IsLendOut');

    ALTER TABLE [dbo].[t_fin_account_ext_loan]
		ADD [PAYINGACCOUNT] int NULL;

	ALTER TABLE [dbo].[t_fin_account_ext_loan]
		ADD [PARTNER] nvarchar(50) NULL;
END;

-- Update the tran. type
UPDATE dbo.[t_fin_tran_type] SET [EXPENSE] = 1 WHERE [ID] = 81; 

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (6,'2018.08.05');

-- The end.