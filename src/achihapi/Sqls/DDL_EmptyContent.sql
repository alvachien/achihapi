﻿DELETE FROM t_fin_account WHERE ID IS NOT NULL;
DELETE FROM t_fin_controlcenter WHERE ID IS NOT NULL;
DELETE FROM t_fin_order WHERE ID IS NOT NULL;
DELETE FROM t_fin_document WHERE ID IS NOT NULL;
DELETE FROM t_fin_exrate WHERE TRANDATE IS NOT NULL;
DELETE FROM t_fin_setting WHERE SETID IS NOT NULL;
DELETE FROM t_fin_tmpdoc_dp WHERE DOCID IS NOT NULL;
DELETE FROM t_learn_award WHERE ID IS NOT NULL;
DELETE FROM t_learn_hist WHERE USERID IS NOT NULL;
DELETE FROM t_learn_obj WHERE ID IS NOT NULL;
DELETE FROM t_learn_plan WHERE ID IS NOT NULL;