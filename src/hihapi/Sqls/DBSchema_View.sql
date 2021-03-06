﻿/**
  * Database views
  */

/****** Object:  View [dbo].[v_homemember]    Script Date: 2017-09-06 11:25:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Old style codes, keep it as reference
--IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.VIEWS
--    WHERE TABLE_NAME = 'v_homemember')
--BEGIN
--	DROP VIEW [dbo].[v_homemember];
--END
--GO

DROP VIEW IF EXISTS [dbo].[v_homemember]
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

DROP VIEW IF EXISTS [dbo].[v_fin_order_srule]
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

DROP VIEW IF EXISTS [dbo].[V_FIN_DOCUMENT_ITEM]
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
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * [T_FIN_DOCUMENT].[EXGRATE] / 100 * -1)
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * [T_FIN_DOCUMENT].[EXGRATE] / 100
					END)
                ELSE (
                CASE
					WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * -1)
					WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT]
				END)
                END)
        ELSE ( CASE WHEN ([T_FIN_DOCUMENT].[EXGRATE2] IS NOT NULL) THEN (
					CASE
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 1) THEN ([T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * [T_FIN_DOCUMENT].[EXGRATE2] / 100 * -1)
						WHEN ([T_FIN_TRAN_TYPE].[EXPENSE] = 0) THEN [T_FIN_DOCUMENT_ITEM].[TRANAMOUNT] * [T_FIN_DOCUMENT].[EXGRATE2] / 100
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

DROP VIEW IF EXISTS [dbo].[V_FIN_DOCUMENT_ITEM1];
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

DROP VIEW IF EXISTS [dbo].[v_fin_document];
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

-- Updated at 29.09.2017
/****** Object:  View [dbo].[v_fin_grp_acnt]    Script Date: 2017-04-22 11:27:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt]
GO

create view [dbo].[v_fin_grp_acnt]
as
select 
		[hid] as [hid],
        [accountid] AS [accountid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
    from
        [v_fin_document_item]
		group by [hid], [accountid];
GO

/****** Object:  View [dbo].[v_fin_grp_acnt2]    Script Date: 2017-04-21 6:58:01 PM ******/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt2]
GO

create view [dbo].[v_fin_grp_acnt2]
as
select 
		[v_fin_grp_acnt].[hid] AS [hid],
        [v_fin_grp_acnt].[accountid] AS [accountid],
		[t_fin_account_ctgy].ID AS [categoryid],
		[t_fin_account_ctgy].ASSETFLAG AS [categoryassetflag],
        "debitbalance" = 
		case [t_fin_account_ctgy].ASSETFLAG
            when 1 then [v_fin_grp_acnt].[balance_lc]
            else 0
        end,
        "creditbalance" = 
		case [t_fin_account_ctgy].ASSETFLAG
            when  0 then -(1) * [v_fin_grp_acnt].[balance_lc]
            else 0
        end,
		[v_fin_grp_acnt].[balance_lc] AS [balance_lc]
    from
        [v_fin_grp_acnt]
		join [t_fin_account] ON [v_fin_grp_acnt].[accountid] = [t_fin_account].ID
        join [t_fin_account_ctgy] ON [t_fin_account].CTGYID = t_fin_account_ctgy.ID

GO

/****** Object:  View [dbo].[v_fin_grp_acnt_tranexp]    Script Date: 2017-04-22 11:28:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_acnt_tranexp]
GO

create view [dbo].[v_fin_grp_acnt_tranexp]
as
with docitem_curr1_basecurr as 
(
	select a.HID,
		b.ACCOUNTID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from t_fin_document as a
		left outer join t_fin_document_item as b
			on a.ID = b.DOCID and ( b.USECURR2 is null or b.USECURR2 = '')
		inner join t_homedef as c
			on a.HID = c.ID
			and a.TRANCURR = c.BASECURR
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID		
	),
docitem_curr1_foreigncurr as 
(
	select a.HID,
		b.ACCOUNTID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE] / 100
        END as tranamount_lc
	from (select HID, a1.ID, EXGRATE from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR <> c.BASECURR 
			and a1.EXGRATE IS NOT NULL) as a
		left outer join t_fin_document_item as b
			on a.ID = b.DOCID and ( b.USECURR2 is null or b.USECURR2 = '')
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_basecurr as 
(
	select a.HID,
		b.ACCOUNTID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from t_fin_document as a
		inner join t_homedef as c
			on a.HID = c.ID
			and a.TRANCURR2 = c.BASECURR		
		left outer join t_fin_document_item as b
			on a.ID = b.DOCID and b.USECURR2 is not null and b.USECURR2 <> ''

		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_foreigncurr as 
(
	select a.HID,
		b.ACCOUNTID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE2] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE2] / 100
        END as tranamount_lc
	from (select HID, a1.ID, EXGRATE2 from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR2 IS NOT NULL AND a1.TRANCURR2 <> c.BASECURR 
			and a1.EXGRATE2 IS NOT NULL) as a
		left outer join t_fin_document_item as b
			on a.ID = b.DOCID and b.USECURR2 is NOT null AND b.USECURR2 <> ''
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitems_all as (
	select hid, accountid, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_basecurr
	group by hid, ACCOUNTID, EXPENSE

	union all
	select hid, accountid, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_basecurr
	group by hid, ACCOUNTID, EXPENSE

	union all 
	select hid, accountid, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_foreigncurr
	group by hid, ACCOUNTID, EXPENSE

	union all 
	select hid, accountid, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_foreigncurr
	group by hid, ACCOUNTID, EXPENSE
	)

select hid, accountid, expense as TRANTYPE_EXP, round(sum([balance_lc]), 2) AS [balance_lc] from
docitems_all
group by hid, accountid, expense;

GO


/****** Object:  View [dbo].[v_fin_report_bs]    Script Date: 2017-04-21 9:04:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_report_bs]
GO

CREATE VIEW [dbo].[v_fin_report_bs]
AS 
	SELECT e.HID, e.ACCOUNTID, e.DEBIT_BALANCE, e.CREDIT_BALANCE, 
        e.DEBIT_BALANCE - e.CREDIT_BALANCE AS BALANCE
    FROM (
        SELECT c.HID, 
            c.ACCOUNTID,
            c.DEBIT_BALANCE,
            CASE WHEN d.BALANCE_LC IS NULL THEN 0 ELSE -1 * d.BALANCE_LC END AS CREDIT_BALANCE
        FROM
        ( SELECT a.HID, a.ID AS ACCOUNTID, 
                CASE WHEN b.BALANCE_LC IS NULL THEN 0 ELSE b.BALANCE_LC END AS DEBIT_BALANCE
            FROM T_FIN_ACCOUNT as a 
            LEFT OUTER JOIN (SELECT HID, ACCOUNTID, BALANCE_LC FROM V_FIN_GRP_ACNT_TRANEXP WHERE TRANTYPE_EXP = 0) as b
			    ON a.ID = b.ACCOUNTID AND a.HID = b.HID 
	    ) as c
	    LEFT OUTER JOIN (SELECT HID, ACCOUNTID, BALANCE_LC FROM V_FIN_GRP_ACNT_TRANEXP WHERE TRANTYPE_EXP = 1) as d
		    ON c.HID = d.HID AND c.ACCOUNTID = d.ACCOUNTID 
	    ) as e
GO


/****** Object:  View [dbo].[v_fin_grp_cc]    Script Date: 2017-04-22 8:05:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_cc]
GO

CREATE VIEW [dbo].[v_fin_grp_cc]
AS
SELECT  [hid],
        [controlcenterid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [controlcenterid] IS NOT NULL
		GROUP BY [hid], [controlcenterid];

GO

/****** Object:  View [dbo].[v_fin_grp_cc_tranexp]    Script Date: 2017-04-22 8:06:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_cc_tranexp]
GO

CREATE VIEW [dbo].[v_fin_grp_cc_tranexp]
AS
with docitem_curr1_basecurr as 
(
	select a.HID,
		b.CONTROLCENTERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from (select HID, a1.ID from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR = c.BASECURR  ) as a
		left outer join (select DOCID, TRANTYPE, CONTROLCENTERID, TRANAMOUNT FROM t_fin_document_item 
		WHERE CONTROLCENTERID IS NOT NULL AND ( USECURR2 is null or USECURR2 = '')
		) as b
			on a.ID = b.DOCID
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID	
	),
docitem_curr1_foreigncurr as 
(
	select a.HID,
		b.CONTROLCENTERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE] / 100
        END as tranamount_lc
	from  (select HID, a1.ID, EXGRATE from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR <> c.BASECURR 
			and a1.EXGRATE IS NOT NULL) as a
		left outer join (select DOCID, TRANTYPE, CONTROLCENTERID, TRANAMOUNT FROM t_fin_document_item 
		WHERE CONTROLCENTERID IS NOT NULL AND ( USECURR2 is null or USECURR2 = '')
		)  as b
			on a.ID = b.DOCID 
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_basecurr as 
(
	select a.HID,
		b.CONTROLCENTERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from (select HID, a1.ID, EXGRATE2 from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR2 = c.BASECURR ) as a
		left outer join ( select DOCID, TRANTYPE, CONTROLCENTERID, TRANAMOUNT FROM t_fin_document_item 
			WHERE CONTROLCENTERID IS NOT NULL and USECURR2 is NOT null AND USECURR2 <> '' 
		) as b
			on a.ID = b.DOCID
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_foreigncurr as 
(
	select a.HID,
		b.CONTROLCENTERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE2] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE2] / 100
        END as tranamount_lc
	from (select HID, a1.ID, EXGRATE2 from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR2 IS NOT NULL AND a1.TRANCURR2 <> c.BASECURR 
			and a1.EXGRATE2 IS NOT NULL) as a
		left outer join ( select DOCID, TRANTYPE, CONTROLCENTERID, TRANAMOUNT FROM t_fin_document_item 
			WHERE CONTROLCENTERID IS NOT NULL and USECURR2 is NOT null AND USECURR2 <> '' 
		) as b			
			on a.ID = b.DOCID 
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitems_all as (
	select hid, CONTROLCENTERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_basecurr
	group by hid, CONTROLCENTERID, EXPENSE

	union all
	select hid, CONTROLCENTERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_basecurr
	group by hid, CONTROLCENTERID, EXPENSE

	union all 
	select hid, CONTROLCENTERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_foreigncurr
	group by hid, CONTROLCENTERID, EXPENSE

	union all 
	select hid, CONTROLCENTERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_foreigncurr
	group by hid, CONTROLCENTERID, EXPENSE
	)

select hid, CONTROLCENTERID, expense as TRANTYPE_EXP, round(sum([balance_lc]), 2) AS [balance_lc] from
docitems_all
group by hid, CONTROLCENTERID, expense;
GO

/****** Object:  View [dbo].[v_fin_report_cc]    Script Date: 2017-04-22 8:11:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_report_cc]
GO

create view [dbo].[v_fin_report_cc]
AS 
SELECT e.HID, e.CONTROLCENTERID, e.DEBIT_BALANCE, e.CREDIT_BALANCE, 
    e.DEBIT_BALANCE - e.CREDIT_BALANCE AS BALANCE
FROM (
    SELECT c.HID, 
        c.CONTROLCENTERID,
        c.DEBIT_BALANCE,
        CASE WHEN d.BALANCE_LC IS NULL THEN 0 ELSE -1 * d.BALANCE_LC END AS CREDIT_BALANCE
    FROM
    ( SELECT a.HID, a.ID AS CONTROLCENTERID, 
            CASE WHEN b.BALANCE_LC IS NULL THEN 0 ELSE b.BALANCE_LC END AS DEBIT_BALANCE
        FROM T_FIN_CONTROLCENTER as a 
        LEFT OUTER JOIN (SELECT HID, CONTROLCENTERID, BALANCE_LC FROM V_FIN_GRP_CC_TRANEXP WHERE TRANTYPE_EXP = 0) as b
			ON a.ID = b.CONTROLCENTERID AND a.HID = b.HID 
	) as c
	LEFT OUTER JOIN (SELECT HID, CONTROLCENTERID, BALANCE_LC FROM V_FIN_GRP_CC_TRANEXP WHERE TRANTYPE_EXP = 1) as d
		ON c.HID = d.HID AND c.CONTROLCENTERID = d.CONTROLCENTERID
	) as e

GO

/****** Object:  View [dbo].[v_fin_grp_ord]    Script Date: 2017-04-22 8:46:17 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_grp_ord]
GO

CREATE VIEW [dbo].[v_fin_grp_ord]
AS
SELECT  [hid],
        [orderid],
		round(sum([tranamount_lc]), 2) AS [balance_lc]
FROM
        [v_fin_document_item]
		WHERE [orderid] IS NOT NULL
		GROUP BY [hid], [orderid];

GO

/****** Object:  View [dbo].[V_FIN_GRP_ORD_TRANEXP]    Script Date: 2017-04-22 8:47:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[V_FIN_GRP_ORD_TRANEXP]
GO

CREATE VIEW [dbo].[V_FIN_GRP_ORD_TRANEXP]
AS
with docitem_curr1_basecurr as 
(
	select a.HID,
		b.ORDERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from (select HID, a1.ID from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR = c.BASECURR  ) as a
		left outer join (select DOCID, TRANTYPE, ORDERID, TRANAMOUNT FROM t_fin_document_item 
		WHERE ORDERID IS NOT NULL AND ( USECURR2 is null or USECURR2 = '')
		) as b
			on a.ID = b.DOCID
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID	
	),
docitem_curr1_foreigncurr as 
(
	select a.HID,
		b.ORDERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE] / 100
        END as tranamount_lc
	from  (select HID, a1.ID, EXGRATE from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR <> c.BASECURR 
			and a1.EXGRATE IS NOT NULL) as a
		left outer join (select DOCID, TRANTYPE, ORDERID, TRANAMOUNT FROM t_fin_document_item 
		WHERE ORDERID IS NOT NULL AND ( USECURR2 is null or USECURR2 = '')
		)  as b
			on a.ID = b.DOCID 
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_basecurr as 
(
	select a.HID,
		b.ORDERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT]
			END as tranamount_lc
	from (select HID, a1.ID, EXGRATE2 from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR2 = c.BASECURR ) as a
		left outer join ( select DOCID, TRANTYPE, ORDERID, TRANAMOUNT FROM t_fin_document_item 
			WHERE ORDERID IS NOT NULL and USECURR2 is NOT null AND USECURR2 <> '' 
		) as b
			on a.ID = b.DOCID
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitem_curr2_foreigncurr as 
(
	select a.HID,
		b.ORDERID,
		d.Expense,
		CASE WHEN (d.[EXPENSE] = 1) THEN (b.[TRANAMOUNT] * a.[EXGRATE2] / 100 * -1)
			 WHEN (d.[EXPENSE] = 0) THEN b.[TRANAMOUNT] * a.[EXGRATE2] / 100
        END as tranamount_lc
	from (select HID, a1.ID, EXGRATE2 from t_fin_document as a1
		inner join t_homedef as c
			on a1.HID = c.ID
			and a1.TRANCURR2 IS NOT NULL AND a1.TRANCURR2 <> c.BASECURR 
			and a1.EXGRATE2 IS NOT NULL) as a
		left outer join ( select DOCID, TRANTYPE, ORDERID, TRANAMOUNT FROM t_fin_document_item 
			WHERE ORDERID IS NOT NULL and USECURR2 is NOT null AND USECURR2 <> '' 
		) as b			
			on a.ID = b.DOCID 
		inner join t_fin_tran_type as d
			on b.TRANTYPE = d.ID
	),
docitems_all as (
	select hid, ORDERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_basecurr
	group by hid, ORDERID, EXPENSE

	union all
	select hid, ORDERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_basecurr
	group by hid, ORDERID, EXPENSE

	union all 
	select hid, ORDERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr1_foreigncurr
	group by hid, ORDERID, EXPENSE

	union all 
	select hid, ORDERID, expense, round(sum([tranamount_lc]), 2) AS [balance_lc] 
	from docitem_curr2_foreigncurr
	group by hid, ORDERID, EXPENSE
	)

select hid, ORDERID, expense as TRANTYPE_EXP, round(sum([balance_lc]), 2) AS [balance_lc] from
docitems_all
group by hid, ORDERID, expense;

GO

/****** Object:  View [dbo].[v_fin_report_order]    Script Date: 2017-04-22 8:47:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_report_order]
GO

create view [dbo].[v_fin_report_order]
AS 
SELECT tab_a.[ORDERID],
	   tab_a.[HID],
	   tab_a.[ORDERNAME],
       tab_a.[balance_lc] AS [debit_balance],
       tab_b.[balance_lc] AS [credit_balance],
        (tab_a.[balance_lc] - tab_b.[balance_lc]) AS [balance]
 FROM 
	(SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[HID] AS [HID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc]
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 0 ) tab_a

	JOIN 

	( SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc] * -1
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 1 ) tab_b

	ON tab_a.[ORDERID] = tab_b.[ORDERID]

GO

-- Update at 2017.9.30
/****** Object:  View [dbo].[v_fin_report_trantype]    Script Date: 2017-09-30 10:27:58 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_report_trantype]
GO

create view [dbo].[v_fin_report_trantype]
AS 
select taba.HID, taba.TRANDATE, taba.TRANTYPE, tabb.NAME, tabb.EXPENSE, taba.tranamount
from 
(select hid, trandate, trantype, sum(tranamount_lc) as tranamount from [V_FIN_DOCUMENT_ITEM]
	group by hid, trandate, trantype) taba
	inner join t_fin_tran_type tabb on taba.TRANTYPE = tabb.ID
GO

-- Updated at 2017.10.02
/****** Object:  View [dbo].[v_lrn_usrlrndate]    Script Date: 2017-10-02 10:35:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_lrn_usrlrndate];
GO

create view [dbo].[v_lrn_usrlrndate]
as
select taba.HID, taba.USERID, tabb.[DISPLAYAS], taba.[LEARNDATE], taba.learncount
from
(select HID, USERID, [LEARNDATE], count(*) as learncount from t_learn_hist
	group by hid, userid, [LEARNDATE] ) taba
	left outer join t_homemem tabb
	on taba.hid = tabb.HID AND taba.userid = tabb.[USER];
	
GO


/****** Object:  View [dbo].[v_lrn_ctgylrndate]    Script Date: 2017-10-02 10:44:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_lrn_ctgylrndate];
GO

create view [dbo].[v_lrn_ctgylrndate]
as
select HID, CATEGORY, LEARNDATE, COUNT(*) as learncount
from (
select taba.HID, taba.USERID, tabb.CATEGORY, taba.LEARNDATE from t_learn_hist taba
	left outer join t_learn_obj tabb on taba.objectid = tabb.ID ) tabc
	group by HID, CATEGORY, LEARNDATE;
GO

-- Updated at 2017.10.24
/****** Object:  View [dbo].[v_fin_report_order]    Script Date: 2017-10-24 10:04:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_fin_report_order]
GO

create view [dbo].[v_fin_report_order]
AS 
SELECT tab_a.[ORDERID],
	   tab_a.[HID],
	   tab_a.[ORDERNAME],
	   tab_a.ORDERVALID_FROM,
	   tab_a.ORDERVALID_TO,
       tab_a.[balance_lc] AS [debit_balance],
       tab_b.[balance_lc] AS [credit_balance],
        (tab_a.[balance_lc] - tab_b.[balance_lc]) AS [balance]
 FROM 
	(SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[HID] AS [HID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		[t_fin_order].[VALID_FROM] AS [ORDERVALID_FROM],
		[t_fin_order].[VALID_TO] AS [ORDERVALID_TO],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc]
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 0 ) tab_a

	JOIN 

	( SELECT [t_fin_order].[ID] AS [ORDERID],
		[t_fin_order].[NAME] AS [ORDERNAME],
		(case
            when ([v_fin_grp_order_tranexp].[balance_lc] is not null) then [v_fin_grp_order_tranexp].[balance_lc] * -1
            else 0.0
        end) AS [balance_lc]
	FROM [dbo].[t_fin_order]
	LEFT OUTER JOIN [dbo].[v_fin_grp_order_tranexp] ON [t_fin_order].[ID] = [v_fin_grp_order_tranexp].orderid
		AND [v_fin_grp_order_tranexp].[trantype_exp] = 1 ) tab_b

	ON tab_a.[ORDERID] = tab_b.[ORDERID]

GO

-- Updated at 2017.10.29
/****** Object:  View [dbo].[v_lrn_qtnbank]    Script Date: 2017-10-29 12:26:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP VIEW IF EXISTS [dbo].[v_lrn_qtnbank]
GO

create view [dbo].[v_lrn_qtnbank]
as
select taba.ID, taba.HID, taba.[Type], taba.Question, tabb.SUBITEM, tabb.DETAIL,  
	taba.BriefAnswer, taba.CREATEDBY, taba.CREATEDAT, taba.UPDATEDBY, taba.UPDATEDAT,
	tabb.OTHERS
	 from t_learn_qtn_bank taba
	left outer join t_learn_qtn_bank_sub tabb
		on taba.ID = tabb.QTNID
GO

-- Updated at 2018.3.14
/****** Object:  View [dbo].[v_event_habitdetail]    Script Date: 2018-03-14 6:58:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_event_habitdetail]
AS
SELECT t_event_habit.ID, t_event_habit.HID, t_event_habit.Name, t_event_habit.StartDate, t_event_habit.EndDate, t_event_habit.RPTTYPE, 
	t_event_habit.IsPublic, t_event_habit.Content,
	t_event_habit.Count, t_event_habit.Assignee, t_event_habit_detail.ID AS DetailID, t_event_habit_detail.StartDate AS DetailStartDate, 
	t_event_habit_detail.EndDate AS DetailEndDate, t_event_habit.CREATEDBY, 
    t_event_habit.CREATEDAT, t_event_habit.UPDATEDBY, t_event_habit.UPDATEDAT
FROM  t_event_habit LEFT OUTER JOIN
		t_event_habit_detail ON t_event_habit.ID = t_event_habit_detail.HabitID
GO

-- Updated at 2018.5.24
/****** Object:  View [dbo].[v_event_habitdetail_checkin_stat]    Script Date: 2018-05-24 6:58:39 PM ******/
-- It's an internal view
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[v_event_habitdetail_checkin_stat]
as 
SELECT taba.HabitID, taba.StartDate, taba.EndDate, COUNT(*) AS CHECKINAMT, AVG(tabb.SCORE) AS ASCORE 
	FROM t_event_habit_detail as taba
	INNER JOIN t_event_habit_checkin as tabb
	ON taba.HabitID = tabb.HabitID
	AND tabb.TranDate >= taba.StartDate
	AND tabb.TranDate < taba.EndDate
	GROUP BY taba.HabitID, taba.StartDate, taba.EndDate
GO

/****** Object:  View [dbo].[v_event_habitdetail_withcheckin]    Script Date: 2018-05-24 6:58:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create view [dbo].[v_event_habitdetail_withcheckin]
as
SELECT        c.HID, c.ID AS HabitID, c.Count AS ExpAmount, b.CHECKINAMT, b.ASCORE, a.StartDate, a.EndDate, c.[Name], c.Assignee, c.IsPublic
FROM            dbo.t_event_habit_detail AS a INNER JOIN
                         dbo.t_event_habit AS c ON a.HabitID = c.ID LEFT OUTER JOIN
                         dbo.v_event_habitdetail_checkin_stat AS b ON a.HabitID = b.HabitID
GO
