using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;

namespace hihapi.test.UnitTests.Finance.Document
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
        public String? SecondCurrency { get; set; }
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
            String? secondcurr,
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
            SecondExchangeRateIsPlanned= other.SecondExchangeRateIsPlanned;
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
}
