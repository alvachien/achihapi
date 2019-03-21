using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using achihapi.ViewModels;

namespace achihapi.test.ViewModels
{
    [TestClass]
    public class FinanceDocumentViewModelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WorkoutDeltaForHeaderUpdate_NullInput()
        {
            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WorkoutDeltaForHeaderUpdate_SameInstance()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WorkoutDeltaForHeaderUpdate_DifferentDocID()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 2;
            doc2.HID = 1;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WorkoutDeltaForHeaderUpdate_DifferentHID()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 2;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WorkoutDeltaForHeaderUpdate_DifferentDocType()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Transfer;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
        }

        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_Desp()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
        }
        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_Sql_Desp()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdateSqlString(doc1, doc2);
            Assert.IsTrue(rst.Length > 0);
            Assert.IsTrue(rst == "UPDATE [dbo].[t_fin_document] SET [Desp] = N'Test2' WHERE [ID] = 1");
        }

        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_DespAndDate()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today.AddDays(1);
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
            Assert.AreEqual(2, rst.Count);
            Assert.IsTrue(rst.ContainsKey("Desp"));
            Assert.IsTrue(String.CompareOrdinal(rst["Desp"] as String, "Test2") == 0);
            Assert.IsTrue(rst.ContainsKey("TranDate"));
            Assert.IsTrue(rst["TranDate"] is DateTime);
        }
        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_Sql_DespAndDate()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test2";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today.AddDays(1);
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdateSqlString(doc1, doc2);
            var todaystr = doc2.TranDate.ToString("yyyy-MM-dd");
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_document] SET [Desp] = N'Test2',[TranDate] = '" + todaystr + "' WHERE [ID] = 1", rst);
        }

        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_CurrAndExchangeRate()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "USD";
            doc2.ExgRate = 645.23M;
            doc2.ExgRate_Plan = true;
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc1, doc2);
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("TranCurr"));
            Assert.IsTrue(String.CompareOrdinal(rst["TranCurr"] as String, "USD") == 0);
            Assert.IsTrue(rst.ContainsKey("ExgRate"));
            Assert.IsTrue(rst.ContainsKey("ExgRate_Plan"));
        }
        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_Sql_CurrAndExchangeRate()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "USD";
            doc2.ExgRate = 645.23M;
            doc2.ExgRate_Plan = true;
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdateSqlString(doc1, doc2);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_document] SET [ExgRate] = 645.23,[ExgRate_Plan] = 1,[TranCurr] = N'USD' WHERE [ID] = 1", rst);
        }

        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_CurrAndExchangeRate2()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "USD";
            doc2.ExgRate = 645.23M;
            doc2.ExgRate_Plan = true;
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdate(doc2, doc1);
            Assert.AreEqual(3, rst.Count);
            Assert.IsTrue(rst.ContainsKey("TranCurr"));
            Assert.IsTrue(String.CompareOrdinal(rst["TranCurr"] as String, "CNY") == 0);
            Assert.IsTrue(rst.ContainsKey("ExgRate"));
            Assert.IsTrue(rst.ContainsKey("ExgRate_Plan"));
        }
        [TestMethod]
        public void WorkoutDeltaForHeaderUpd_Sql_CurrAndExchangeRate2()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "USD";
            doc2.ExgRate = 645.23M;
            doc2.ExgRate_Plan = true;
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc2.Items.Add(item1);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForHeaderUpdateSqlString(doc2, doc1);
            Assert.IsTrue(rst.Length > 0);
            Assert.AreEqual("UPDATE [dbo].[t_fin_document] SET [ExgRate] = NULL,[ExgRate_Plan] = NULL,[TranCurr] = N'CNY' WHERE [ID] = 1", rst);
        }

        [TestMethod]
        public void DeltaUpdateForItemUpdate_AddItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;
            doc2.Items.Add(item2);
            item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 2";
            item2.ItemID = 2;
            item2.TranAmount = 200;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForItemUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey(2));
            Assert.IsTrue(rst[2] is FinanceDocumentItemUIViewModel);
        }

        [TestMethod]
        public void DeltaUpdateForItemUpdate_RemoveItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 100;
            doc2.Items.Add(item2);
            item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 2";
            item2.ItemID = 2;
            item2.TranAmount = 200;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForItemUpdate(doc2, doc1);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey(2));
            Assert.IsNull(rst[2]);
        }

        [TestMethod]
        public void DeltaUpdateForItemUpdate_ChangeItem()
        {
            var doc1 = new FinanceDocumentUIViewModel();
            doc1.Desp = "Test1";
            doc1.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc1.TranCurr = "CNY";
            doc1.TranDate = DateTime.Today;
            doc1.ID = 1;
            doc1.HID = 1;
            var item1 = new FinanceDocumentItemUIViewModel();
            item1.DocID = 1;
            item1.AccountID = 2;
            item1.ControlCenterID = 3;
            item1.TranType = 5;
            item1.Desp = "Item 1";
            item1.ItemID = 1;
            item1.TranAmount = 100;
            doc1.Items.Add(item1);

            var doc2 = new FinanceDocumentUIViewModel();
            doc2.Desp = "Test1";
            doc2.DocType = FinanceDocTypeViewModel.DocType_Normal;
            doc2.TranCurr = "CNY";
            doc2.TranDate = DateTime.Today;
            doc2.ID = 1;
            doc2.HID = 1;
            var item2 = new FinanceDocumentItemUIViewModel();
            item2.DocID = 1;
            item2.AccountID = 2;
            item2.ControlCenterID = 3;
            item2.TranType = 5;
            item2.Desp = "Item 1";
            item2.ItemID = 1;
            item2.TranAmount = 110;
            doc2.Items.Add(item2);

            var rst = FinanceDocumentUIViewModel.WorkoutDeltaForItemUpdate(doc1, doc2);
            Assert.AreEqual(1, rst.Count);
            Assert.IsTrue(rst.ContainsKey(1));
            Assert.IsTrue(rst[1] is Dictionary<String, Object>);
        }
    }
}

