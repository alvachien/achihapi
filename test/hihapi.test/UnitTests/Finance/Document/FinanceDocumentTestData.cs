using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.Json;
using hihapi.Models;

namespace hihapi.test.UnitTests.Finance
{
    public class FinanceDocumentTestData : IXunitSerializable
    {
        public FinanceDocumentTestData()
        {
            Items = new List<FinanceDocumentItem>();
        }

        public Int32 ID { get; set; }
        public Int32 HomeID { get; set; }
        public Int16 DocType { get; set; }
        public DateTime TranDate { get; set; }
        public String TranCurr { get; set; }
        public String Desp { get; set; }
        public Decimal? ExgRate { get; set; }
        public Boolean? ExgRate_Plan { get; set; }
        public String? TranCurr2 { get; set; }
        public Decimal? ExgRate2 { get; set; }
        public Boolean? ExgRate_Plan2 { get; set; }
        public List<FinanceDocumentItem> Items { get; set; }
        public bool ExpectedIsValidResult { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            String val = info.GetValue<String>("Value");
            FinanceDocumentTestData other = JsonSerializer.Deserialize<FinanceDocumentTestData>(val);
            HomeID = other.HomeID;
            ID = other.ID;
            DocType = other.DocType;
            TranDate = other.TranDate;
            TranCurr = other.TranCurr;
            Desp = other.Desp;
            ExgRate = other.ExgRate;
            ExgRate_Plan = other.ExgRate_Plan;
            ExgRate2 = other.ExgRate2;
            ExgRate_Plan2 = other.ExgRate_Plan2;
            ExpectedIsValidResult = other.ExpectedIsValidResult;
            Items.Clear();
            Items.AddRange(other.Items);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            String val = JsonSerializer.Serialize(this);
            info.AddValue("Value", val, typeof(String));
        }
    }
}
