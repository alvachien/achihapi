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

-- Changed at 2017.9.15
/****** Object:  View [dbo].[v_fin_document_item1]    Script Date: 2017-04-21 10:40:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [DBO].[V_FIN_DOCUMENT_ITEM1]
AS 
SELECT 
        [V_FIN_DOCUMENT_ITEM].[DOCID],
        [V_FIN_DOCUMENT_ITEM].[ITEMID],
		[V_FIN_DOCUMENT_ITEM].[HID],
		[V_FIN_DOCUMENT_ITEM].[TRANDATE],
		[V_FIN_DOCUMENT_ITEM].[DOCDESP],
        [V_FIN_DOCUMENT_ITEM].[ACCOUNTID],
		[T_FIN_ACCOUNT].[NAME] AS [ACCOUNTNAME],
        [V_FIN_DOCUMENT_ITEM].[TRANTYPE],
		[V_FIN_DOCUMENT_ITEM].[TRANTYPENAME],
		[V_FIN_DOCUMENT_ITEM].[TRANTYPE_EXP],
		[V_FIN_DOCUMENT_ITEM].[USECURR2],
        [V_FIN_DOCUMENT_ITEM].[TRANCURR],
        [V_FIN_DOCUMENT_ITEM].[TRANAMOUNT_ORG],
        [V_FIN_DOCUMENT_ITEM].[TRANAMOUNT],
        [V_FIN_DOCUMENT_ITEM].[TRANAMOUNT_LC],
        [V_FIN_DOCUMENT_ITEM].[CONTROLCENTERID],
		[T_FIN_CONTROLCENTER].[NAME] AS [CONTROLCENTERNAME],
        [V_FIN_DOCUMENT_ITEM].[ORDERID],
		[T_FIN_ORDER].[NAME] AS [ORDERNAME],
        [V_FIN_DOCUMENT_ITEM].[DESP]
    FROM
        [V_FIN_DOCUMENT_ITEM]
		LEFT OUTER JOIN [T_FIN_ACCOUNT] ON [V_FIN_DOCUMENT_ITEM].[ACCOUNTID] = [T_FIN_ACCOUNT].[ID]
		LEFT OUTER JOIN [T_FIN_CONTROLCENTER] ON [V_FIN_DOCUMENT_ITEM].[CONTROLCENTERID] = [T_FIN_CONTROLCENTER].[ID]
		LEFT OUTER JOIN [T_FIN_ORDER] ON [V_FIN_DOCUMENT_ITEM].[ORDERID] = [T_FIN_ORDER].[ID];
GO

/****** Object:  View [dbo].[v_fin_document]    Script Date: 2017-02-20 12:14:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


create view [dbo].[v_fin_document]
as
 select 
        [t_fin_document].[ID] AS [ID],
		[t_fin_document].[HID] AS [HID],
        [t_fin_document].[DOCTYPE] AS [DOCTYPE],
		[t_fin_doc_type].[NAME] AS [DOCTYPENAME],
        [t_fin_document].[TRANDATE] AS [TRANDATE],
        [t_fin_document].[TRANCURR] AS [TRANCURR],
        [t_fin_document].[DESP] AS [DESP],
        [t_fin_document].[EXGRATE] AS [EXGRATE],
        [t_fin_document].[EXGRATE_PLAN] AS [EXGRATE_PLAN],
        [t_fin_document].[TRANCURR2] AS [TRANCURR2],
        [t_fin_document].[EXGRATE2] AS [EXGRATE2],
        [t_fin_document].[EXGRATE_PLAN2] AS [EXGRATE_PLAN2],
		[t_fin_document].[CREATEDBY],
		[t_fin_document].[CREATEDAT],
		[t_fin_document].[UPDATEDBY],
		[t_fin_document].[UPDATEDAT],
        [item_table].[TRANAMOUNT]
    from
        [t_fin_document]
		left outer join [t_fin_doc_type] on [t_fin_document].[DOCTYPE] = [t_fin_doc_type].[ID]
        left outer join (select [DOCID] AS [id], sum([tranamount_lc]) AS [tranamount] from [v_fin_document_item1] 
			group by [DOCID]
		) as item_table on ([t_fin_document].[ID] = [item_table].[id])  
    where [t_fin_document].[DOCTYPE] != 3 AND [t_fin_document].[DOCTYPE] != 2    
    
    union all
    
    select 
        [t_fin_document].[ID] AS [ID],
		[t_fin_document].[HID] AS [HID],
        [t_fin_document].[DOCTYPE] AS [DOCTYPE],
		[t_fin_doc_type].[NAME] AS [DOCTYPENAME],
        [t_fin_document].[TRANDATE] AS [TRANDATE],
        [t_fin_document].[TRANCURR] AS [TRANCURR],
        [t_fin_document].[DESP] AS [DESP],
        [t_fin_document].[EXGRATE] AS [EXGRATE],
        [t_fin_document].[EXGRATE_PLAN] AS [EXGRATE_PLAN],
        [t_fin_document].[TRANCURR2] AS [TRANCURR2],
        [t_fin_document].[EXGRATE2] AS [EXGRATE2],
        [t_fin_document].[EXGRATE_PLAN2] AS [EXGRATE_PLAN2],
		[t_fin_document].[CREATEDBY],
		[t_fin_document].[CREATEDAT],
		[t_fin_document].[UPDATEDBY],
		[t_fin_document].[UPDATEDAT],
        0 AS [TRANAMOUNT]
    from
        [t_fin_document]
		left outer  join [t_fin_doc_type] on [t_fin_document].[DOCTYPE] = [t_fin_doc_type].[ID]
    where [t_fin_document].[DOCTYPE] = 3 OR [t_fin_document].[DOCTYPE] = 2; -- Transfer doc and exchange rate

GO
