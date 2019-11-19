-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type]
    ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
VALUES
    (86, N'偿还借贷款', 1, 25, N'偿还借贷款');
INSERT INTO dbo.[t_fin_tran_type]
    ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
VALUES
    (87, N'借贷还款收入', 0, 5, N'借出的款项返还');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type]
    ([ID],[NAME],[COMMENT])
VALUES
    (11, N'Sys.DocTy.Repay', N'借款、贷款等');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Process the documents posted to the loan
WITH
    LOANACCOUNT
    AS
    (
        SELECT DISTINCT
            c.[DOCID]
        FROM [dbo].[t_fin_account_ext_loan] as a
            LEFT OUTER JOIN dbo.t_fin_document_item as c ON c.ACCOUNTID = a.ACCOUNTID
    )
	UPDATE [dbo].[t_fin_document]
		SET [DOCTYPE] = 11
		WHERE [ID] IN (  SELECT
    [DOCID] AS [ID]
FROM LOANACCOUNT );

-- Process the tmp. documents posted to Loan
UPDATE [dbo].[t_fin_tmpdoc_loan]
SET [dbo].[t_fin_tmpdoc_loan].[TRANTYPE] = CASE [dbo].[t_fin_account_ext_loan].[IsLendOut] WHEN 1 THEN 87 ELSE 86 END
FROM [dbo].[t_fin_tmpdoc_loan]
    INNER JOIN [dbo].[t_fin_account_ext_loan]
    ON [dbo].[t_fin_tmpdoc_loan].ACCOUNTID = [dbo].[t_fin_account_ext_loan].ACCOUNTID;

---- Process posted loan document item
UPDATE [dbo].[t_fin_document_item]
SET [dbo].[t_fin_document_item].[TRANTYPE] = CASE [dbo].[t_fin_account_ext_loan].[IsLendOut] WHEN 1 THEN 86 ELSE 87 END
FROM [dbo].[t_fin_document_item]
    INNER JOIN [dbo].[t_fin_tmpdoc_loan]
    ON [dbo].[t_fin_document_item].DOCID = [dbo].[t_fin_tmpdoc_loan].REFDOCID
    INNER JOIN [dbo].[t_fin_account_ext_loan]
    ON [dbo].[t_fin_tmpdoc_loan].ACCOUNTID = [dbo].[t_fin_account_ext_loan].ACCOUNTID;

-- Add new item to posted loan repayment document
WITH
    LOANDOCITEM
    AS
    (
        SELECT
            a.[DOCID]
    , a.[ACCOUNTID]
	, a.[TRANTYPE]
	, a.TRANAMOUNT
	, a.USECURR2
	, a.CONTROLCENTERID
	, a.ORDERID
	, a.DESP
	, c.IsLendOut
        FROM [dbo].[t_fin_document_item] as a
            INNER JOIN [dbo].[t_fin_tmpdoc_loan] as b
            ON b.REFDOCID = a.DOCID
            INNER JOIN [dbo].[t_fin_account_ext_loan] as c
            ON c.ACCOUNTID = b.ACCOUNTID
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
           , CASE [IsLendOut] WHEN 1 THEN 86 ELSE 87 END
           , TRANAMOUNT
           , USECURR2
           , CONTROLCENTERID
           , ORDERID
           , DESP
FROM LOANDOCITEM;

-- Version
INSERT INTO [dbo].[t_dbversion]
    ([VersionID],[ReleasedDate])
VALUES
    (3, '2018.07.10');

-- The End.