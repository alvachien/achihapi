

-- Change table T_FIN_ACCOUNT_EXT_AS
ALTER TABLE T_FIN_ACCOUNT_EXT_AS ADD [BOUGHT_DATE] DATE NULL;
ALTER TABLE T_FIN_ACCOUNT_EXT_AS ADD [EXPIRED_DATE] DATE NULL;
ALTER TABLE T_FIN_ACCOUNT_EXT_AS ADD [RESIDUAL_VALUE] DECIMAL(17, 2) NULL;

-- Add content
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (15,N'Sys.DocTy.AssetDeprec', N'资产折旧');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Set the version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (19, '2022.10.31');

-- The end.
