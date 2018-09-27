
DELETE FROM t_homedef WHERE ID IS NOT NULL;

-- Delete account part
DELETE FROM t_fin_document WHERE HID = 1;
DELETE FROM t_fin_account_ext_as WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_account_ext_cc WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_account_ext_dp WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_account_ext_loan WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_account_ext_loan_h WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_controlcenter WHERE HID = 1;
DELETE FROM t_fin_order_srule WHERE ORDID IN (SELECT ID FROM t_fin_order WHERE HID = 1);
DELETE FROM t_fin_order WHERE HID = 1;
DELETE t_fin_tmpdoc_dp WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE t_fin_tmpdoc_loan WHERE ACCOUNTID IN (SELECT ID FROM t_fin_account WHERE HID = 1);
DELETE FROM t_fin_account WHERE HID = 1;

--DELETE FROM t_learn_award WHERE ID IS NOT NULL;
DELETE FROM t_learn_hist WHERE USERID IS NOT NULL;
DELETE FROM t_learn_obj WHERE ID IS NOT NULL;
--DELETE FROM t_learn_plan WHERE ID IS NOT NULL;

DELETE FROM [t_lib_book] WHERE [ID] IS NOT NULL;
DELETE FROM [t_lib_location] WHERE [ID] IS NOT NULL;
DELETE FROM [t_lib_location_detail] WHERE [ID] IS NOT NULL;
DELETE FROM [t_lib_book_ctgy] WHERE [ID] IS NOT NULL;
DELETE FROM [t_lib_person] WHERE [ID] IS NOT NULL;
