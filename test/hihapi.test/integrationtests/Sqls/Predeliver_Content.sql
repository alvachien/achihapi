
-- Content for language, updated at 2017.9.9
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (4, N'zh-Hans', N'Chinese (Simplified)', N'简体中文', 1);
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (9, N'en', N'English', N'English', 1);
INSERT INTO [dbo].[t_language] ([LCID],[ISONAME],[ENNAME],[NAVNAME],[APPFLAG]) VALUES (17, N'ja', N'Japanese', N'日本语', 0);
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



-- Content for FIN account category, updated at 2017.10.6
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

DELETE FROM dbo.[t_fin_account_ctgy] WHERE [HID] IS NULL;

INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (1,N'Sys.AcntCty.Cash',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (2,N'Sys.AcntCty.DepositAccount',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (3,N'Sys.AcntCty.CreditCard',0,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (4,N'Sys.AcntCty.AccountPayable',0,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (5,N'Sys.AcntCty.AccountReceviable',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (6,N'Sys.AcntCty.VirtualAccount',1,N'如支付宝等');
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (7,N'Sys.AcntCty.AssetAccount',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (8,N'Sys.AcntCty.AdvancedPayment',1,NULL);
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (9,N'Sys.AcntCty.Loan',0,NULL);
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
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (6,N'Sys.DocTy.CreditCardRepay', N'信用卡还款');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (7,N'Sys.DocTy.AssetBuyIn', N'购入资产或大件家用器具');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (8,N'Sys.DocTy.AssetSoldOut', N'出售资产或大件家用器具');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (9,N'Sys.DocTy.Loan', N'贷款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (6,N'预收款', N'预收款');
--INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (8,N'借出款', N'借出款项');

SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Content for asset category
SET IDENTITY_INSERT dbo.[t_fin_asset_ctgy] ON;

DELETE FROM dbo.[t_fin_asset_ctgy] WHERE [HID] IS NULL;
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (1, N'Sys.AssCtgy.Apartment', N'公寓');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (2, N'Sys.AssCtgy.Automobile', N'机动车');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (3, N'Sys.AssCtgy.Furniture', N'家具');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (4, N'Sys.AssCtgy.HouseAppliances', N'家用电器');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (5, N'Sys.AssCtgy.Camera', N'相机');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (6, N'Sys.AssCtgy.Computer', N'计算机');
INSERT INTO [dbo].[t_fin_asset_ctgy] ([ID], [NAME],[DESP]) VALUES (7, N'Sys.AssCtgy.MobileDevice', N'移动设备');

SET IDENTITY_INSERT dbo.[t_fin_asset_ctgy] OFF;


-- Update at 2017.10.15
SET IDENTITY_INSERT dbo.[t_lib_book_ctgy] ON;

DELETE FROM dbo.[t_lib_book_ctgy] WHERE [HID] IS NULL;
INSERT INTO [dbo].[t_lib_book_ctgy] ([ID], [NAME],[PARID], [OTHERS]) VALUES (1, N'Sys.BkCtgy.Novel', NULL, N'小说');

SET IDENTITY_INSERT dbo.[t_lib_book_ctgy] OFF;


-- Update at 2017.10.23
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (1,N'起始资金',0,NULL,N'起始资金');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (69,N'公共交通类',1,NULL,N'公共交通类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (70,N'公交地铁等',1,69,N'短途交通类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (71,N'长途客车等',1,69,N'长途汽车类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (72,N'火车动车等',1,69,N'火车动车类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (73,N'飞机等',1,69,N'飞机类开支');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;


-- Update at 2017.10.24
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (74,N'出租车等',1,69,N'出租车类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (75,N'医疗保健',1,NULL,N'医疗保健类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (76,N'诊疗费',1,75,N'门诊、检查类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (77,N'医药费',1,75,N'药费类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (78,N'保健品费',1,75,N'保健品类开支');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (79,N'有线电视费',1,11,N'有线电视费');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;


-- Updated at 2017.11.07
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (80,N'贷款入账',0,10,N'收到贷款');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;


-- Updated at 2018.6.6
SET IDENTITY_INSERT [dbo].[t_lib_movie_genre] ON;

INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (1,NULL,N'Sys.MovGenre.ActAdv',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (2,NULL,N'Sys.MovGenre.Animation',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (3,NULL,N'Sys.MovGenre.Anime',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (4,NULL,N'Sys.MovGenre.Classics',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (5,NULL,N'Sys.MovGenre.Comedy',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (6,NULL,N'Sys.MovGenre.Documentary',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (7,NULL,N'Sys.MovGenre.Drama',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (8,NULL,N'Sys.MovGenre.ExerFit',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (9,NULL,N'Sys.MovGenre.FaithSpirit',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (10,NULL,N'Sys.MovGenre.ForeignLang',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (11,NULL,N'Sys.MovGenre.GayLesbian',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (12,NULL,N'Sys.MovGenre.Horror',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (13,NULL,N'Sys.MovGenre.IndianMv',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (14,NULL,N'Sys.MovGenre.IndieArt',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (15,NULL,N'Sys.MovGenre.KidsFamily',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (16,NULL,N'Sys.MovGenre.MVConcert',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (17,NULL,N'Sys.MovGenre.Romance',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (18,NULL,N'Sys.MovGenre.SciFiction',NULL,NULL);
INSERT INTO [dbo].[t_lib_movie_genre] ([ID], [HID],[Name],[ParID],[Others]) VALUES (19,NULL,N'Sys.MovGenre.Western',NULL,NULL);

SET IDENTITY_INSERT [dbo].[t_lib_movie_genre] OFF;

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2018.6.30
-- Fin: Account category
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

DELETE FROM dbo.[t_fin_account_ctgy] WHERE [ID] = 9;
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (9,N'Sys.AcntCty.BorrowFrom',0,N'借入款、贷款');
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (10,N'Sys.AcntCty.LendTo',1,N'借出款');

SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

-- Fin: Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
DELETE FROM dbo.[t_fin_doc_type] WHERE [ID] = 9;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (9,N'Sys.DocTy.BorrowFrom', N'借款、贷款等');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (10,N'Sys.DocTy.LendTo', N'借出款');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Fin: transaction type

UPDATE dbo.[t_fin_tran_type] SET  [NAME] = N'贷款入账', [COMMENT] = N'收到借入款、贷款'
	WHERE [ID] = 80;

SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (81,N'借出款项',0,10,N'借出款项');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;


-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2018.7.3
-- Tran type.
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (82,N'起始负债',1,NULL,NULL);
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'投资、保险、博彩类收入', COMMENT = N'投资收入、保险收入、博彩收入等' WHERE [ID] = 5;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'彩票收益', COMMENT = N'彩票中奖类收益', [PARID] = 5 WHERE [ID] = 13;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'投资、保险、博彩类支出', COMMENT = N'投资支出、保险支出、博彩支出等' WHERE [ID] = 25;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'彩票支出', COMMENT = N'彩票投注等支出', [PARID] = 25 WHERE [ID] = 29;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'保单投保、续保支出', COMMENT = N'保单投保、保单续保等' WHERE [ID] = 34;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'银行利息支出', COMMENT = N'银行利息支出等' WHERE [ID] = 55;
UPDATE dbo.[t_fin_tran_type] SET [NAME] = N'银行手续费支出', COMMENT = N'银行手续费支出等' WHERE [ID] = 56;
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24 WHERE [ID] = 81;
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (83,N'投资手续费支出',1,25,N'理财产品等投资手续费');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;


-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.7.4
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (1,'2018.07.04');


-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.7.5
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24 WHERE [ID] = 82;
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 10 WHERE [ID] = 1; 

SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (84,N'房租收入',0,5,N'房租收入等');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (85,N'房租支出',1,11,N'房租支出等');
SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (2,'2018.07.05');


-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.7.6
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (86,N'偿还借贷款',1,25,N'偿还借贷款');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (87,N'借贷还款收入',0,5,N'借出的款项返还');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (11,N'Sys.DocTy.Repay', N'借款、贷款等');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (3,'2018.07.10');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.7.11
-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (88,N'预付款支出',1,25,N'偿还借贷款');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (89,N'资产减值',1,25,N'资产贬值或减值');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (90,N'资产增值',0,5,N'资产增值');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Updated at 2017.7.24
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (4,'2018.07.11');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.8.4
-- No content update for v5.sql
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (5,'2018.08.04');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.8.5
UPDATE dbo.[t_fin_tran_type] SET [EXPENSE] = 1 WHERE [ID] = 81; 

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (6,'2018.08.05');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.9.11
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;
INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (11,N'Sys.AcntCty.AdvancedRecv',0,N'预收款');
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (12,N'Sys.DocTy.AdvancedRecv', N'预收款');
SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;


INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (7,'2018.10.10');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.9.15
-- Add new transaction type.
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT])
	VALUES (91, N'预收款收入', 0, 10, N'预收款收入');
SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Update transaction type.
UPDATE dbo.[t_fin_tran_type] SET [PARID] = 24, [COMMENT] = N'预付款支出' WHERE [ID] = 88; 

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (8,'2018.11.1');

-----------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.9.15

SET IDENTITY_INSERT dbo.[t_learn_ctgy] ON;

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (16,NULL,N'摄影',N'摄影相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (17,16,N'摄影前期',N'前期相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (18,16,N'摄影后期',N'后期相关');

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (19,NULL,N'IT',N'计算机相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (20,19,N'基础理论',N'基础理论');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (21,19,N'硬件',N'计算机硬件');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (22,19,N'软件',N'计算机软件');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (23,22,N'编程语言',N'编程语言');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (24,22,N'数据库',N'数据库类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (25,22,N'操作系统',N'操作系统类');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (26,19,N'网络',N'网络相关');

INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (27,NULL,N'财务类',N'财务相关');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (28,27,N'财务会计',N'财务会计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (29,27,N'成本会计',N'成本会计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (30,27,N'审计',N'审计');
INSERT INTO dbo.[t_learn_ctgy] ([ID],[PARID],[NAME],[COMMENT]) VALUES (31,27,N'税法',N'税法');

SET IDENTITY_INSERT dbo.[t_learn_ctgy] OFF;

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (9,'2018.11.2');


------------------------------------------------------------------------------------------------------------------------------------
-- Updated at 2017.10.13

-- Tran. type
SET IDENTITY_INSERT dbo.[t_fin_tran_type] ON;

INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (92, N'资产出售费用', 1, 25, N'资产出售导致的资产');
INSERT INTO dbo.[t_fin_tran_type] ([ID],[NAME],[EXPENSE],[PARID],[COMMENT]) VALUES (93, N'资产出售收益', 0, 5, N'资产出售所得收益');

SET IDENTITY_INSERT dbo.[t_fin_tran_type] OFF;

-- Doc type
SET IDENTITY_INSERT dbo.[t_fin_doc_type] ON;

INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (13, N'Sys.DocTy.AssetValChg', N'资产净值变动');
INSERT INTO dbo.[t_fin_doc_type] ([ID],[NAME],[COMMENT]) VALUES (14, N'Sys.DocTy.Insurance', N'保险');

SET IDENTITY_INSERT dbo.[t_fin_doc_type] OFF;

-- Account category
SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] ON;

INSERT INTO dbo.[t_fin_account_ctgy] ([ID],[NAME],[ASSETFLAG],[COMMENT]) VALUES (12,N'Sys.AcntCty.Insurance',1,N'保险');

SET IDENTITY_INSERT dbo.[t_fin_account_ctgy] OFF;

INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (10,'2018.11.3');

-------------------------------------------------------------------------------------------------------------------
-- Updated at 2018.12.20
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (11,'2018.12.20');

-------------------------------------------------------------------------------------------------------------------
-- Updated at 2019.4.20
INSERT INTO [dbo].[t_dbversion] ([VersionID],[ReleasedDate]) VALUES (12,'2019.4.20');

---------------------------------
-- TODO...


