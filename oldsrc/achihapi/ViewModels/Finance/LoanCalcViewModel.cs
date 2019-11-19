using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ViewModels
{
    public sealed class LoanCalcViewModel
    {
        public Decimal TotalAmount { get; set; }
        public Int16 TotalMonths { get; set; }
        public Decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public Boolean InterestFreeLoan { get; set; }
        public LoanRepaymentMethod RepaymentMethod { get; set; }
        public DateTime? EndDate { get; set; }
        public Int16? RepayDayInMonth { get; set; }
        public DateTime? FirstRepayDate { get; set; }

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
            if (StartDate == null)
                throw new Exception("Start date is must");
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
                var nInitDays = (int)(FirstRepayDate.Value.Date - StartDate.Date).TotalDays;
                // Check the dates
                if (nInitDays < 30 || nInitDays > 60)
                    throw new Exception("First repayment day is invalid");
            }
        }
    }

    public sealed class LoanCalcResult
    {
        public DateTime TranDate { get; set; }
        public Decimal TranAmount { get; set; }
        public Decimal InterestAmount { get; set; }
        public Decimal TotalAmount { get { return this.TranAmount + this.InterestAmount; } }
    }
}
