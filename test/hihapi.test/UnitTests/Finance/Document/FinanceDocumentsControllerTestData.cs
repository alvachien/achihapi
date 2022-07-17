using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;
using hihapi.Models;

namespace hihapi.unittest.Finance
{
    public class FinanceDocumentsControllerTestData_DocItem
    {
        public Int32 ItemID { get; set; }
        public Int32 AccountID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        public Int32 TranType { get; set; }
        public Decimal Amount { get; set; }
        public String Desp { get; set; }
        public Boolean? UseCurr2 { get; set; }
    }

    public class FinanceDocumentsControllerTestData_NormalDoc : IXunitSerializable
    {
        public Int32 HomeID { get; set; }
        public String CurrentUser { get; set; }
        public String Currency { get; set; }
        public String SecondCurrency { get; set; }
        public Decimal? ExchangeRate { get; set; }
        public Boolean? ExchangeRateIsPlanned { get; set; }
        public Decimal? SecondExchangeRate { get; set; }
        public Boolean? SecondExchangeRateIsPlanned { get; set; }
        public String Desp { get; set; }
        public List<FinanceDocumentsControllerTestData_DocItem> DocItems { get; set; }

        public FinanceDocumentsControllerTestData_NormalDoc()
        {
            DocItems = new List<FinanceDocumentsControllerTestData_DocItem>();
        }

        public FinanceDocumentsControllerTestData_NormalDoc(
            String usr,
            int hid,
            String curr,
            String secondcurr,
            Decimal? exchangeRate,
            bool? exchangeRateIsPlanned,
            Decimal? exchangeRate2,
            bool? exchangeRate2IsPlanned,
            String desp,
            List<FinanceDocumentsControllerTestData_DocItem> items) : this()
        {
            CurrentUser = usr;
            HomeID = hid;
            Currency = curr;
            SecondCurrency = secondcurr;
            ExchangeRate = exchangeRate;
            ExchangeRateIsPlanned = exchangeRateIsPlanned;
            SecondExchangeRate = exchangeRate2;
            SecondExchangeRateIsPlanned = exchangeRate2IsPlanned;
            Desp = desp;
            if (items.Count > 0)
                DocItems.AddRange(items);
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_NormalDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_NormalDoc>(val);

            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            SecondCurrency = other.SecondCurrency;
            ExchangeRate = other.ExchangeRate;
            ExchangeRateIsPlanned = other.ExchangeRateIsPlanned;
            SecondExchangeRate = other.SecondExchangeRate;
            SecondExchangeRateIsPlanned = other.SecondExchangeRateIsPlanned;
            Desp = other.Desp;
            if (other.DocItems.Count > 0)
                DocItems.AddRange(other.DocItems);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_DPDoc : IXunitSerializable
    {
        public int HomeID { get; set; }
        public string CurrentUser { get; set; }
        public string Currency { get; set; }
        public DateTime TranDate { get; set; }
        public Decimal Amount { get; set; }
        public Int32 AccountID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency Frequency { get; set; }
        public String Comment { get; set; }
        public Int32? DPControlCenterID { get; set; }
        public Int32? DPOrderID { get; set; }
        public Int32 DPTranType { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_DPDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_DPDoc>(val);

            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            Amount = other.Amount;
            AccountID = other.AccountID;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;
            StartDate = other.StartDate;
            EndDate = other.EndDate;
            Comment = other.Comment;
            Frequency = other.Frequency;
            DPControlCenterID = other.DPControlCenterID;
            DPOrderID = other.DPOrderID;
            DPTranType = other.DPTranType;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_LoanDoc : IXunitSerializable
    {
        public int HomeID { get; set; }
        public string CurrentUser { get; set; }
        public string Currency { get; set; }
        public DateTime TranDate { get; set; }
        public Decimal Amount { get; set; }
        public Int32 AccountID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        public String AccountName { get; set; }
        public Decimal? AnnualRate { get; set; }
        public Boolean? InterestFree { get; set; }
        public LoanRepaymentMethod? RepaymentMethod { get; set; }
        public Int16? TotalMonths { get; set; }
        public String Others { get; set; }
        public Int32? PayingAccount { get; set; }
        public String Partner { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency Frequency { get; set; }
        public String Comment { get; set; }
        public Int32? TmpDocControlCenterID { get; set; }
        public Int32? TmpDocOrderID { get; set; }
        public Int32 TmpDocTranType { get; set; }

        public Boolean PostLoanTmpDocs { get; set; }
        public Int32 RepayAccountID { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_LoanDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_LoanDoc>(val);
            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            TranDate = other.TranDate;
            Amount = other.Amount;
            AccountID = other.AccountID;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;

            AccountName = other.AccountName;
            AnnualRate = other.AnnualRate;
            InterestFree = other.InterestFree;
            RepaymentMethod = other.RepaymentMethod;
            TotalMonths = other.TotalMonths;
            Others = other.Others;
            PayingAccount = other.PayingAccount;
            Partner = other.Partner;

            StartDate = other.StartDate;
            EndDate = other.EndDate;
            Frequency = other.Frequency;
            Comment = other.Comment;
            TmpDocControlCenterID = other.TmpDocControlCenterID;
            TmpDocOrderID = other.TmpDocOrderID;
            TmpDocTranType = other.TmpDocTranType;

            PostLoanTmpDocs = other.PostLoanTmpDocs;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_LegacyLoan : IXunitSerializable
    {
        public int HomeID { get; set; }
        public string CurrentUser { get; set; }
        public string Currency { get; set; }
        public DateTime TranDate { get; set; }
        public Decimal Amount { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        public String AccountName { get; set; }
        public Boolean? InterestFree { get; set; }
        public String Others { get; set; }
        public Int32? PayingAccount { get; set; }
        public String Partner { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RepeatFrequency Frequency { get; set; }
        public String Comment { get; set; }
        public Int32? TmpDocControlCenterID { get; set; }
        public Int32? TmpDocOrderID { get; set; }
        public Int32 TmpDocTranType { get; set; }

        public Boolean PostLoanTmpDocs { get; set; }
        public Int32 RepayAccountID { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_LoanDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_LoanDoc>(val);
            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            TranDate = other.TranDate;
            Amount = other.Amount;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;

            AccountName = other.AccountName;
            InterestFree = other.InterestFree;
            Others = other.Others;
            PayingAccount = other.PayingAccount;
            Partner = other.Partner;

            StartDate = other.StartDate;
            EndDate = other.EndDate;
            Frequency = other.Frequency;
            Comment = other.Comment;
            TmpDocControlCenterID = other.TmpDocControlCenterID;
            TmpDocOrderID = other.TmpDocOrderID;
            TmpDocTranType = other.TmpDocTranType;

            PostLoanTmpDocs = other.PostLoanTmpDocs;
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_AssetBuyDoc : IXunitSerializable
    {
        public int HomeID { get; set; }
        public string CurrentUser { get; set; }
        public string Currency { get; set; }
        public DateTime TranDate { get; set; }
        public Decimal Amount { get; set; }
        public Int32 AccountID { get; set; }
        public Int32? ControlCenterID { get; set; }
        public Int32? OrderID { get; set; }
        public bool IsLegacy { get; set; }
        public String Desp { get; set; }
        public FinanceAccountExtraAS AccountExtra { get; set; }
        public List<FinanceDocumentItem> Items { get; set; }

        public FinanceDocumentsControllerTestData_AssetBuyDoc()
        {
            this.Items = new List<FinanceDocumentItem>();
        }

        public virtual void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_AssetBuyDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_AssetBuyDoc>(val);
            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            TranDate = other.TranDate;
            Amount = other.Amount;
            AccountID = other.AccountID;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;
            Desp = other.Desp;
            IsLegacy = other.IsLegacy;

            AccountExtra = other.AccountExtra;
            this.Items.Clear();
            this.Items.AddRange(other.Items);
        }

        public virtual void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_AssetChangeDoc : FinanceDocumentsControllerTestData_AssetBuyDoc
    {
        public Decimal NewAmount { get; set; }

        public FinanceDocumentsControllerTestData_AssetChangeDoc(): base()
        {
        }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_AssetChangeDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_AssetChangeDoc>(val);
            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            TranDate = other.TranDate;
            Amount = other.Amount;
            AccountID = other.AccountID;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;
            Desp = other.Desp;
            IsLegacy = other.IsLegacy;

            NewAmount = other.NewAmount;

            AccountExtra = other.AccountExtra;
            this.Items.Clear();
            this.Items.AddRange(other.Items);
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

    public class FinanceDocumentsControllerTestData_AssetSellDoc : FinanceDocumentsControllerTestData_AssetBuyDoc
    {
        public Decimal NewAmount { get; set; }
        public int CashAccountID { get; set; }

        public FinanceDocumentsControllerTestData_AssetSellDoc() : base()
        {
        }

        public override void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentsControllerTestData_AssetSellDoc other = JsonSerializer.Deserialize<FinanceDocumentsControllerTestData_AssetSellDoc>(val);
            CurrentUser = other.CurrentUser;
            HomeID = other.HomeID;
            Currency = other.Currency;
            TranDate = other.TranDate;
            TranDate = other.TranDate;
            Amount = other.Amount;
            AccountID = other.AccountID;
            ControlCenterID = other.ControlCenterID;
            OrderID = other.OrderID;
            Desp = other.Desp;
            IsLegacy = other.IsLegacy;

            NewAmount = other.NewAmount;
            CashAccountID = other.CashAccountID;

            AccountExtra = other.AccountExtra;
            this.Items.Clear();
            this.Items.AddRange(other.Items);
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }

}
