
-- Content for language, updated at 2017.9.9
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (4, N'zh-Hans', N'Chinese (Simplified)', N'简体中文', 1);
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (9, N'en', N'English', N'English', 1);
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (17, N'ja', N'Japanese', N'日本语', 1);
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (31748, N'zh-Hant', N'Chinese (Traditional)', N'繁體中文', 0);

-- Content for currency, updated at 2017.9.9
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'CNY', N'Sys.Currency.CNY', N'¥');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'EUR', N'Sys.Currency.EUR', N'€');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'HKD', N'Sys.Currency.HKD', N'HK$');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'JPY', N'Sys.Currency.JPY', N'¥');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'KRW', N'Sys.Currency.KRW', N'₩');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'TWD', N'Sys.Currency.TWD', N'TW$');
INSERT INTO [dbo].[t_fin_currency] ([CURR],[NAME],[SYMBOL]) VALUES (N'USD', N'Sys.Currency.USD', N'$');

-- Content for learn category, updated at 2017.9.9
SET IDENTITY_INSERT dbo.[t_learn_ctgy] ON;

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (1,NULL,N'语文',N'语文');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (2,1,N'诗词',N'唐诗宋词等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (3,1,N'识字',N'拼音认读和笔画等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (4,1,N'文言文',N'文言文等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (5,1,N'古典名著',N'古典名著等');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (6,NULL,N'数学',N'数学类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (7,6,N'算术',N'加减法');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (8,6,N'代数',N'代数');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (9,6,N'几何',N'几何类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (10,NULL,N'英语',N'英语类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (11,10,N'词汇',N'英语词汇');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (12,10,N'语法',N'英语语法');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (13,NULL,N'日语',N'日语类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (14,13,N'词汇',N'日语词汇');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (15,13,N'语法',N'日语语法');

SET IDENTITY_INSERT dbo.[t_learn_ctgy] OFF;

-- Content for FIN account category, updated at 2017.9.10
--SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (1,N'现金',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (2,N'存储卡',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (3,N'信用卡',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (4,N'应付账款',0,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (5,N'应收账款',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (6,N'金融账户',1,N'如支付宝等');
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (7,N'重大资产',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (8,N'预付款账户',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (9,N'预收款账户',1,NULL);

--SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Content for FIN  Doc Type, updated at 2017.9.10
--SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;

--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (1,N'普通收支',N'普通');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (2,N'转账', N'转账');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (3,N'货币兑换', N'兑换不同的货币');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (4,N'分期付款', N'分期付款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (5,N'预付款', N'预付款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (6,N'预收款', N'预收款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (7,N'借入款', N'借入款项');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (8,N'借出款', N'借出款项');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (9,N'资产购入', N'重大资产购入类');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (10,N'资产售出', N'重大资产售出类');

--SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;


-- Content for FIN Tran Type, updated 2017.9.10
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (2,N'主业收入',0,NULL,N'主业收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (3,N'工资',0,2,N'工资');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (4,N'奖金',0,2,N'奖金');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (5,N'投资类收入',0,NULL,N'投资类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (6,N'股票收益',0,5,N'股票收益');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (7,N'基金收益',0,5,N'基金类收益');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (8,N'利息收入',0,5,N'银行利息收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (9,N'生活类开支',1,NULL,N'生活类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (10,N'其它收入',0,NULL,N'其它收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (11,N'物业类支出',1,9,N'物业类支出');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (12,N'私家车支出',1,NULL,N'私家车支出');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (13,N'彩票奖金',0,10,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (14,N'小区物业费',1,11,N'小区的物业费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (15,N'水费',1,11,N'水费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (16,N'电费',1,11,N'电费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (17,N'天然气费',1,11,N'天然气费用');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (18,N'物业维修费',1,11,N'物业维修费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (19,N'车辆保养',1,12,N'保养');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (20,N'汽油费',1,12,N'汽车加油');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (21,N'车辆保险费',1,12,N'保险');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (22,N'停车费',1,12,N'停车费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (23,N'车辆维修',1,12,N'车辆维修');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (24,N'其它支出',1,NULL,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (25,N'保险类',1,NULL,N'保险类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (26,N'通讯费',1,9,N'通讯费用');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (27,N'固定电话/宽带',1,26,N'固定电话和宽带');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (28,N'手机费',1,26,N'手机、流量等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (29,N'彩票支出',1,24,N'彩票');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (30,N'人情交往类',0,NULL,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (31,N'人际交往',1,NULL,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (32,N'红包支出',1,31,N'红包支出');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (33,N'红包收入',0,30,N'红包收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (34,N'保单缴费',1,25,N'保险保单缴费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (35,N'津贴',0,2,N'津贴类，如加班等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (36,N'保险报销收入',0,5,N'保险报销收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (37,N'转账收入',0,10,N'转账收入');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (38,N'衣服饰品',1,9,N'衣服饰品等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (39,N'食品酒水',1,9,N'食品酒水类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (40,N'衣服鞋帽',1,38,N'衣服鞋帽');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (41,N'化妆饰品',1,38,N'化妆品等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (42,N'水果类',1,39,N'水果');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (43,N'零食类',1,39,N'零食类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (44,N'烟酒茶类',1,39,N'烟酒茶等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (45,N'咖啡外卖类',1,39,N'咖啡与快餐');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (46,N'早中晚餐',1,39,N'正餐类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (47,N'请客送礼',1,31,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (48,N'孝敬家长',1,31,NULL);
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (49,N'休闲娱乐',1,9,N'休闲娱乐类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (50,N'旅游度假',1,49,N'旅游度假');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (51,N'电影演出',1,49,N'看电影，看演出等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (52,N'摄影外拍类',1,49,N'摄影外拍');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (53,N'腐败聚会类',1,49,N'KTV之类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (54,N'学习进修',1,9,N'学习进修');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (55,N'银行利息',1,25,N'银行利息');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (56,N'银行手续费',1,25,N'银行手续费');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (57,N'违章付款类',1,12,N'违章付款等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (58,N'书刊杂志',1,54,N'书刊和杂志');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (59,N'培训进修',1,54,N'培训进修类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (60,N'转账支出',1,24,N'转账');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (61,N'日常用品',1,9,N'日常用品');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (62,N'日用品',1,61,N'日用品类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (63,N'电子产品类',1,61,N'电子产品类');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (64,N'厨房用具',1,61,N'厨房用具');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (65,N'洗涤用品',1,61,N'洗涤用品');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (66,N'大家电类',1,61,N'大家电');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (67,N'保健护理用品',1,61,N'保健护理');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (68,N'喂哺用品',1,61,NULL);

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;



-- Update at 2017.9.30
-- Content for FIN account category, updated at 2017.9.10
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

DELETE FROM dbo.[t_fin_account_ctgy] WHERE [HID] IS NULL;

INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (1,N'Sys.AcntCty.Cash',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (2,N'Sys.AcntCty.DepositAccount',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (3,N'Sys.AcntCty.CreditCard',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (4,N'Sys.AcntCty.AccountPayable',0,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (5,N'Sys.AcntCty.AccountReceivable',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (6,N'Sys.AcntCty.VirtualAccount',1,N'如支付宝等');
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (7,N'Sys.AcntCty.AssetAccount',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (8,N'Sys.AcntCty.AdvancedPayment',1,NULL);
--INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (9,N'预收款账户',1,NULL);

SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Content for doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;

DELETE FROM dbo.[t_fin_doc_type] WHERE [HID] IS NULL;

INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (1,N'Sys.DocTy.Normal',N'普通');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (2,N'Sys.DocTy.Transfer', N'转账');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (3,N'Sys.DocTy.CurrExg', N'兑换不同的货币');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (4,N'Sys.DocTy.Installment', N'分期付款');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (5,N'Sys.DocTy.AdvancedPayment', N'预付款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (6,N'预收款', N'预收款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (7,N'借入款', N'借入款项');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (8,N'借出款', N'借出款项');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (9,N'资产购入', N'重大资产购入类');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (10,N'资产售出', N'重大资产售出类');

SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;




























---------------------------------
-- TODO...


