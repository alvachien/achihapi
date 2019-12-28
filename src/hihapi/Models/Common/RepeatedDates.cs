using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace hihapi.Models
{
    public class RepeatedDates
    {
        [DataType(DataType.Date)]
        public Date StartDate { get; set; }
        [DataType(DataType.Date)]
        public Date EndDate { get; set; }
    }

    public enum RepeatFrequency : Byte
    {
        Month = 0,
        Fortnight = 1,
        Week = 2,
        Day = 3,
        Quarter = 4,
        HalfYear = 5,
        Year = 6,
        Manual = 7,
    }

    public class RepeatDatesCalculationInput
    {
        [DataType(DataType.Date)]
        public Date StartDate { get; set; }
        [DataType(DataType.Date)]
        public Date EndDate { get; set; }
        public RepeatFrequency RepeatType { get; set; }
    }

    public class RepeatDatesWithAmountCalculationInput : RepeatDatesCalculationInput
    {
        public Decimal TotalAmount { get; set; }
        public String Desp { get; set; }
    }

    public class RepeatedDatesWithAmount
    {
        [DataType(DataType.Date)]
        public Date TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public String Desp { get; set; }
    }

    public class RepeatDatesWithAmountAndInterestCalInput
    {
        public Decimal TotalAmount { get; set; }
        public Int16 TotalMonths { get; set; }
        public Decimal InterestRate { get; set; }
        [DataType(DataType.Date)]
        public Date StartDate { get; set; }
        public Boolean InterestFreeLoan { get; set; }
        public LoanRepaymentMethod RepaymentMethod { get; set; }
        [DataType(DataType.Date)]
        public Date? EndDate { get; set; }
        public Int16? RepayDayInMonth { get; set; }
        [DataType(DataType.Date)]
        public Date? FirstRepayDate { get; set; }

        public void doVerify()
        {
            if (InterestFreeLoan && InterestRate != 0)
                throw new Exception("Cannot input interest rate for Interest-Free loan");
            if (InterestRate < 0)
                throw new Exception("Interest rate can not be negative");
            if (TotalAmount <= 0)
                throw new Exception("Total amount must large than zero!");
            if (RepaymentMethod == LoanRepaymentMethod.EqualPrincipal
                || RepaymentMethod == LoanRepaymentMethod.EqualPrincipalAndInterset)
            {
                if (TotalMonths <= 0)
                    throw new Exception("Total months must large than zero");
            }
            else if (RepaymentMethod == LoanRepaymentMethod.DueRepayment)
            {
                if (!EndDate.HasValue)
                    throw new Exception("End date must input");
            }
            else
                throw new Exception("Not supported method");
            if (FirstRepayDate.HasValue && RepayDayInMonth.HasValue)
            {
                if (FirstRepayDate.Value.Day != RepayDayInMonth.Value)
                    throw new Exception("Inconsistency in first payment data and repay day");
            }
            if (RepayDayInMonth.HasValue)
            {
                if (RepayDayInMonth.Value <= 0 || RepayDayInMonth.Value >= 29)
                    throw new Exception("Invalid repay. date");
            }
            if (FirstRepayDate.HasValue)
            {
                var nInitDays = (int)((DateTime)FirstRepayDate.Value - (DateTime)StartDate).TotalDays;
                // Check the dates
                if (nInitDays < 30 || nInitDays > 60)
                    throw new Exception("First repayment day is invalid");
            }
        }
    }

    public sealed class RepeatedDatesWithAmountAndInterest
    {
        [DataType(DataType.Date)]
        public Date TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public Decimal InterestAmount { get; set; }
        public Decimal TotalAmount { get { return this.TranAmount + this.InterestAmount; } }
    }
}
