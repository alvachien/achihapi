
-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24 WHERE [ID] = 82;
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 10 WHERE [ID] = 1;
INSERT INTO dbo.[t_fin_tran_type]
    ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
VALUES
    (84, N'房租收入', 0, 5, N'房租收入等');
INSERT INTO dbo.[t_fin_tran_type]
    ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
VALUES
    (85, N'房租支出', 1, 11, N'房租支出等');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Process loan account
WITH
    LOANACCOUNT
    AS
    (
        SELECT a.[ACCOUNTID]
      , a.[IsLendOut]
	  , c.[DOCID]
	  , c.TRANAMOUNT
	  , c.USECURR2
	  , c.CONTROLCENTERID
	  , c.ORDERID
	  , c.DESP
        FROM [dbo].[t_fin_account_ext_loan] as a
            LEFT OUTER JOIN dbo.t_fin_document as b ON a.REFDOCID = b.ID
            LEFT OUTER JOIN dbo.t_fin_document_item as c ON b.ID = c.DOCID
    )
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
           , 2
           , [ACCOUNTID]
           , CASE [IsLendOut] WHEN 1 THEN 1 ELSE 82 END
           , TRANAMOUNT
           , USECURR2
           , CONTROLCENTERID
           , ORDERID
           , DESP
FROM LOANACCOUNT;

-- Version
INSERT INTO [dbo].[t_dbversion]
    ([VersionID],[ReleasedDate])
VALUES
    (2, '2018.07.05');
