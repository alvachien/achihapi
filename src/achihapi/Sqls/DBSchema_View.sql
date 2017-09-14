/**
  * Database views
  */

/****** Object:  View [dbo].[v_homemember]    Script Date: 2017-09-06 11:25:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_homemember]
AS
	SELECT t_homedef.[ID], t_homedef.[NAME], t_homedef.[HOST], t_homedef.[BASECURR], t_homedef.[DETAILS], t_homemem.[USER], t_homemem.[DISPLAYAS],
		t_homedef.[CREATEDBY], t_homedef.[CREATEDAT], t_homedef.[UPDATEDBY], t_homedef.[UPDATEDAT]
	 FROM dbo.t_homedef
		LEFT OUTER JOIN dbo.t_homemem ON t_homedef.[ID] = t_homemem.[HID]

GO

-- Changed at 2017.9.14
/****** Object:  View [dbo].[v_fin_order_srule]    Script Date: 2017-02-02 4:53:55 PM ******/
DROP VIEW IF EXISTS [dbo].[v_fin_order_srule]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_fin_order_srule]
AS
SELECT  t_fin_order.ID, t_fin_order.HID, t_fin_order.NAME, t_fin_order.VALID_FROM, t_fin_order.VALID_TO, t_fin_order.COMMENT, t_fin_order.CREATEDBY, t_fin_order.CREATEDAT, 
        t_fin_order.UPDATEDBY, t_fin_order.UPDATEDAT, t_fin_order_srule.RULEID, t_fin_order_srule.CONTROLCENTERID, t_fin_controlcenter.NAME AS CONTROLCENTERNAME, 
        t_fin_order_srule.PRECENT
FROM    dbo.t_fin_order LEFT OUTER JOIN
			dbo.t_fin_order_srule ON dbo.t_fin_order.ID = dbo.t_fin_order_srule.ORDID INNER JOIN
            dbo.t_fin_controlcenter ON dbo.t_fin_order_srule.CONTROLCENTERID = dbo.t_fin_controlcenter.ID

GO

/****** Object:  View [dbo].[v_fin_document_item]    Script Date: 2017-04-22 10:49:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [DBO].[V_FIN_DOCUMENT_ITEM]
AS 
SELECT 
        [T_FIN_DOCUMENT_ITEM].[DOCID],
        [T_FIN_DOCUMENT_ITEM].[ITEMID],
		[T_FIN_DOCUMENT].[HID],
		[T_FIN_DOCUMENT].[TRANDATE],
		[T_FIN_DOCUMENT].[DESP] AS [DOCDESP],
        [T_FIN_DOCUMENT_ITEM].[ACCOUNTID],
        [T_FIN_DOCUMENT_ITEM].[TRANTYPE],
		[T_FIN_TRAN_TYPE].[NAME] AS [TRANTYPENAME],
		[T_FIN_TRAN_TYPE].[EXPENSE] AS [TRANTYPE_EXP],
		[T_FIN_DOCUMENT_ITEM].[USECURR2],
        (CASE WHEN ([T_FIN_DOCUMENT_ITEM].[USECURR2] IS NULL OR [T_FIN_DOCUMENT_ITEM].[USECURR2] = '') THEN ([T_FIN_DOCUMENT].[TRANCURR])
        ELSE ([T_FIN_DOCUMENT].[TRANCURR2])
        END) AS [TRANCURR],
        [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] AS [TRANAMOUNT_ORG],
        (CASE
            WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * -1)
            WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT]
        END) AS [TRANAMOUNT],
        (CASE WHEN ([T_FIN_DOCUMENT_ITEM].[USECURR2] IS NULL OR [T_FIN_DOCUMENT_ITEM].[USECURR2] = '') 
			THEN (
                CASE WHEN ([T_FIN_DOCUMENT].[EXGRATE] IS NOT NULL) THEN (
					CASE
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] / [T_FIN_DOCUMENT].[EXGRATE]  * -1)
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] / [T_FIN_DOCUMENT].[EXGRATE]
					END)
                ELSE (
                CASE
					WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * -1)
					WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT]
				END)
                END)
        ELSE ( CASE WHEN ([T_FIN_DOCUMENT].[EXGRATE2] IS NOT NULL) THEN (
					CASE
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] / [T_FIN_DOCUMENT].[EXGRATE2] * -1)
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] / [T_FIN_DOCUMENT].[EXGRATE2]
					END)
                ELSE (
					CASE
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * -1)
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT]
					END)
                END)
        END) AS [TRANAMOUNT_LC],
        [T_FIN_DOCUMENT_ITEM].[CONTROLCENTERID],
        [T_FIN_DOCUMENT_ITEM].[ORDERID],
        [T_FIN_DOCUMENT_ITEM].[DESP]
    FROM
        [T_FIN_DOCUMENT_ITEM]
		JOIN [T_FIN_TRAN_TYPE] ON [T_FIN_DOCUMENT_ITEM].[TRANTYPE] = [T_FIN_TRAN_TYPE].[ID]
        LEFT OUTER JOIN [T_FIN_DOCUMENT] ON [T_FIN_DOCUMENT_ITEM].[DOCID] = [T_FIN_DOCUMENT].[ID]

GO
