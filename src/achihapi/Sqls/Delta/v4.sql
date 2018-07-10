
-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (88,N'预付款支出',1,25,N'偿还借贷款');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (89,N'资产减值',1,25,N'资产贬值或减值');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (90,N'资产增值',0,5,N'资产增值');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Update the ADP document
UPDATE dbo.t_fin_document_item
SET TRANTYPE = 88
WHERE DOCID IN (select [REFDOCID] FROM dbo.t_fin_account_ext_dp);

-- Insert new item to ADP document
WITH ADPDOCS AS (SELECT a.[ACCOUNTID]
	  ,c.[DOCID]
	  ,c.TRANAMOUNT
	  ,c.USECURR2
	  ,c.CONTROLCENTERID
	  ,c.ORDERID
	  ,c.DESP
  FROM [dbo].[t_fin_account_ext_dp] as a
	LEFT OUTER JOIN dbo.t_fin_document as b ON a.REFDOCID = b.ID
	LEFT OUTER JOIN dbo.t_fin_document_item as c ON b.ID = c.DOCID)
	INSERT INTO [dbo].[t_fin_document_item]
           ([DOCID]
           ,[ITEMID]
           ,[ACCOUNTID]
           ,[TRANTYPE]
           ,[TRANAMOUNT]
           ,[USECURR2]
           ,[CONTROLCENTERID]
           ,[ORDERID]
           ,[DESP])
     SELECT 
           [DOCID]
           ,2
           ,[ACCOUNTID]
           ,1
           ,TRANAMOUNT
           ,USECURR2
           ,CONTROLCENTERID
           ,ORDERID
           ,DESP
		   FROM ADPDOCS;

-- Insert new item to Asset account
WITH ASSETDOCS AS (SELECT a.[ACCOUNTID]
	  ,c.[DOCID]
	  ,c.TRANAMOUNT
	  ,c.USECURR2
	  ,c.CONTROLCENTERID
	  ,c.ORDERID
	  ,c.DESP
  FROM [dbo].[t_fin_account_ext_as] as a
	LEFT OUTER JOIN dbo.t_fin_document as b ON a.REFDOC_BUY = b.ID
	LEFT OUTER JOIN dbo.t_fin_document_item as c ON b.ID = c.DOCID)
	INSERT INTO [dbo].[t_fin_document_item]
           ([DOCID]
           ,[ITEMID]
           ,[ACCOUNTID]
           ,[TRANTYPE]
           ,[TRANAMOUNT]
           ,[USECURR2]
           ,[CONTROLCENTERID]
           ,[ORDERID]
           ,[DESP])
     SELECT 
           [DOCID]
           ,2
           ,[ACCOUNTID]
           ,1
           ,TRANAMOUNT
           ,USECURR2
           ,CONTROLCENTERID
           ,ORDERID
           ,DESP
		   FROM ASSETDOCS;

-- Version
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (4,'2018.07.11');
