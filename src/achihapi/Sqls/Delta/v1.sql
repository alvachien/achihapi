IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_fin_account_ext_loan' AND COLUMN_NAME = 'EndDate')
BEGIN

	ALTER TABLE [dbo].[t_fin_account_ext_loan] 
		ADD [EndDate] [date] NULL DEFAULT getdate();
	ALTER TABLE [dbo].[t_fin_account_ext_loan] 
		ADD [IsLendOut] [bit] NOT NULL DEFAULT 0;

END;

SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (82,N'起始负债',1,NULL,NULL);
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'投资、保险、博彩类收入', COMMENT = N'投资收入、保险收入、博彩收入等' WHERE [ID] = 5;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'彩票收益', COMMENT = N'彩票中奖类收益', [PARID] = 5 WHERE [ID] = 13;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'投资、保险、博彩类支出', COMMENT = N'投资支出、保险支出、博彩支出等' WHERE [ID] = 25;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'彩票支出', COMMENT = N'彩票投注等支出', [PARID] = 25 WHERE [ID] = 29;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'保单投保、续保支出', COMMENT = N'保单投保、保单续保等' WHERE [ID] = 34;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'银行利息支出', COMMENT = N'银行利息支出等' WHERE [ID] = 55;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'银行手续费支出', COMMENT = N'银行手续费支出等' WHERE [ID] = 56;
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24 WHERE [ID] = 81;
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (83,N'投资手续费支出',1,25,N'理财产品等投资手续费');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

INSERT INTO [dbo].[t_dbversion]
           ([VersionID]
           ,[ReleasedDate])
     VALUES
           (1
           ,'2018.07.04');
