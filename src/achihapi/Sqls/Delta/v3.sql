-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (86,N'偿还借贷款',1,25,N'偿还借贷款');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (87,N'借贷还款收入',0,5,N'借出的款项返还');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (11,N'Sys.DocTy.Repay', N'借款、贷款等');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (12,N'Sys.DocTy.LendTo', N'借出款');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;
