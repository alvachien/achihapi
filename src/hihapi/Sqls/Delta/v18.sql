

-- Update T_FIN_ACCOUNT status
UPDATE t_fin_account SET [STATUS] = 0 WHERE STATUS IS NULL;

-- Change table
ALTER TABLE t_fin_account ALTER COLUMN [STATUS] TINYINT NOT NULL;

-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (94, N'爱心捐赠', 1, 31, N'爱心捐赠支出');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (18,'2022.05.31');

-- The end.
