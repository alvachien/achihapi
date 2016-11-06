
-- Content for learn category
SET IDENTITY_INSERT dbo.[t_learn_ctgy] ON;

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (1,NULL,N'语文',N'语文',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (2,1,N'诗词',N'唐诗宋词等',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (3,1,N'识字',N'拼音认读和笔画等',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (4,1,N'文言文',N'文言文等',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (5,1,N'古典名著',N'古典名著等',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (6,NULL,N'数学',N'数学类',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (7,6,N'算术',N'加减法',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (8,6,N'代数',N'代数',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (9,6,N'几何',N'几何类',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (10,NULL,N'英语',N'英语类',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (11,10,N'词汇',N'英语词汇',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (12,10,N'语法',N'英语语法',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (13,NULL,N'日语',N'日语类',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (14,13,N'词汇',N'日语词汇',1);
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT],[SYSFLAG]) VALUES (15,13,N'语法',N'日语语法',1);

SET IDENTITY_INSERT dbo.[t_learn_ctgy] OFF;

-- Content for Currency

-- Content for FIN account category
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (1,N'现金',1,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (2,N'存储卡',1,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (3,N'信用卡',1,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (4,N'应付账款',0,N'如贷款等',1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (5,N'应收账款',1,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (6,N'金融账户',1,N'支付宝等',1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (7,N'重大资产',1,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (8,N'预付款账户',2,NULL,1);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT],[SYSFLAG]) VALUES (9,N'预收款账户',3,NULL,1);

SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Content for FIN  Doc Type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;

INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (1,N'普通收支',N'普通', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (2,N'转账', N'转账', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (3,N'货币兑换', N'兑换不同的货币', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (4,N'分期付款', N'分期付款', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (5,N'预付款', N'预付款', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (6,N'预收款', N'预收款', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (7,N'借入款', N'借入款项', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (8,N'借出款', N'借出款项', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (9,N'资产购入', N'重大资产购入类', 1);
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT],[SYSFLAG]) VALUES (10,N'资产售出', N'重大资产售出类', 1);

SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Content for FIN Tran Type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (2,N'主业收入',0,NULL,N'主业收入', 1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (3,N'工资',0,2,N'工资',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (4,N'奖金',0,2,N'奖金',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (5,N'投资类收入',0,NULL,N'投资类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (6,N'股票收益',0,5,N'股票收益',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (7,N'基金收益',0,5,N'基金类收益',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (8,N'利息收入',0,5,N'银行利息收入',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (9,N'生活类开支',1,NULL,N'生活类开支',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (10,N'其它收入',0,NULL,N'其它收入',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (11,N'物业类支出',1,9,N'物业类支出',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (12,N'私家车支出',1,NULL,N'私家车支出',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (13,N'彩票奖金',0,10,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (14,N'小区物业费',1,11,N'小区的物业费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (15,N'水费',1,11,N'水费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (16,N'电费',1,11,N'电费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (17,N'天然气费',1,11,N'天然气费用',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (18,N'物业维修费',1,11,N'物业维修费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (19,N'车辆保养',1,12,N'保养',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (20,N'汽油费',1,12,N'汽车加油',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (21,N'车辆保险费',1,12,N'保险',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (22,N'停车费',1,12,N'停车费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (23,N'车辆维修',1,12,N'车辆维修',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (24,N'其它支出',1,NULL,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (25,N'保险类',1,NULL,N'保险类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (26,N'通讯费',1,9,N'通讯费用',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (27,N'固定电话/宽带',1,26,N'固定电话和宽带',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (28,N'手机费',1,26,N'手机、流量等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (29,N'彩票支出',1,24,N'彩票',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (30,N'人情交往类',0,NULL,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (31,N'人际交往',1,NULL,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (32,N'红包支出',1,31,N'红包支出',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (33,N'红包收入',0,30,N'红包收入',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (34,N'保单缴费',1,25,N'保险保单缴费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (35,N'津贴',0,2,N'津贴类，如加班等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (36,N'保险报销收入',0,5,N'保险报销收入',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (37,N'转账收入',0,10,N'转账收入',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (38,N'衣服饰品',1,9,N'衣服饰品等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (39,N'食品酒水',1,9,N'食品酒水类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (40,N'衣服鞋帽',1,38,N'衣服鞋帽',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (41,N'化妆饰品',1,38,N'化妆品等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (42,N'水果类',1,39,N'水果',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (43,N'零食类',1,39,N'零食类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (44,N'烟酒茶类',1,39,N'烟酒茶等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (45,N'咖啡外卖类',1,39,N'咖啡与快餐',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (46,N'早中晚餐',1,39,N'正餐类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (47,N'请客送礼',1,31,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (48,N'孝敬家长',1,31,NULL,1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (49,N'休闲娱乐',1,9,N'休闲娱乐类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (50,N'旅游度假',1,49,N'旅游度假',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (51,N'电影演出',1,49,N'看电影，看演出等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (52,N'摄影外拍类',1,49,N'摄影外拍',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (53,N'腐败聚会类',1,49,N'KTV之类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (54,N'学习进修',1,9,N'学习进修',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (55,N'银行利息',1,25,N'银行利息',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (56,N'银行手续费',1,25,N'银行手续费',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (57,N'违章付款类',1,12,N'违章付款等',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (58,N'书刊杂志',1,54,N'书刊和杂志',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (59,N'培训进修',1,54,N'培训进修类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (60,N'转账支出',1,24,N'转账',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (61,N'日常用品',1,9,N'日常用品',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (62,N'日用品',1,61,N'日用品类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (63,N'电子产品类',1,61,N'电子产品类',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (64,N'厨房用具',1,61,N'厨房用具',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (65,N'洗涤用品',1,61,N'洗涤用品',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (66,N'大家电类',1,61,N'大家电',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (67,N'保健护理用品',1,61,N'保健护理',1);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT],[SYSFLAG]) VALUES (68,N'喂哺用品',1,61,NULL,1);

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;
