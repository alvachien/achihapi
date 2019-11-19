
-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (92, N'资产出售费用', 1, 25, N'资产出售导致的资产');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (93, N'资产出售收益', 0, 5, N'资产出售所得收益');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;

INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (13, N'Sys.DocTy.AssetValChg', N'资产净值变动');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (14, N'Sys.DocTy.Insurance', N'保险');

SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Account category
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (12,N'Sys.AcntCty.Insurance',1,N'保险');

SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (10,'2018.11.3');

-- The end.