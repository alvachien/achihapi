
-- Add new transaction type.
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
	VALUES (91, N'预收款收入', 0, 10, N'预收款收入');
SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Update transaction type.
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24, [COMMENT] = N'预付款支出' WHERE [ID] = 88; 

-- New version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (8,'2018.11.1');

-- The end.