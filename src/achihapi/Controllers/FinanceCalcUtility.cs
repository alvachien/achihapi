using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using achihapi.ViewModels;

namespace achihapi.Controllers
{
    public class FinanceCalcUtility
    {
        public static List<LoanCalcResult> LoanCalculate(LoanCalcViewModel datInput)
        {
            List<LoanCalcResult> listResults = new List<LoanCalcResult>();

            // Input checks
            if (datInput == null)
                throw new Exception("Input the data!");
            if (datInput.InterestFreeLoan && datInput.InterestRate != 0)
                throw new Exception("Cannot input interest rate for Interest-Free loan");
            if (datInput.InterestRate < 0)
                throw new Exception("Interest rate can not be negative");
            if (datInput.TotalAmount <= 0)
                throw new Exception("Total amount must large than zero!");
            if (datInput.TotalMonths <= 0)
                throw new Exception("Total amount must large than zero");
            if (datInput.StartDate == null)
                throw new Exception("Start date is must");

            if (datInput.InterestFreeLoan)
            {
                listResults.Add(new LoanCalcResult
                {
                    TranDate = datInput.StartDate.AddMonths(datInput.TotalMonths),
                    TranAmount = datInput.TotalAmount,
                    InterestAmount = 0
                });
            }
            else
            {
                switch(datInput.RepaymentMethod)
                {
                    case LoanRepaymentMethod.EqualPrincipalAndInterset:
                        {
                            //每月月供额 =〔贷款本金×月利率×(1＋月利率)＾还款月数〕÷〔(1＋月利率)＾还款月数 - 1〕
                            //每月应还利息 = 贷款本金×月利率×〔(1 + 月利率) ^ 还款月数 - (1 + 月利率) ^ (还款月序号 - 1)〕÷〔(1 + 月利率) ^ 还款月数 - 1〕
                            //每月应还本金 = 贷款本金×月利率×(1 + 月利率) ^ (还款月序号 - 1)÷〔(1 + 月利率) ^ 还款月数 - 1〕
                            Decimal monthRate = datInput.InterestRate / 12;
                            Decimal d3 = (Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) - 1;
                            Decimal monthRepay = datInput.TotalAmount * monthRate * (Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) / d3;

                            Decimal totalInterestAmt = 0;
                            for(int i = 0; i < datInput.TotalMonths; i++)
                            {
                                var rst = new LoanCalcResult
                                {
                                    TranDate = datInput.StartDate.AddMonths(i + 1),
                                    TranAmount = Math.Round(datInput.TotalAmount * monthRate * (Decimal)Math.Pow((double)(1 + monthRate), i) / d3, 2),
                                    InterestAmount = Math.Round(datInput.TotalAmount * monthRate * ((Decimal)Math.Pow((double)(1 + monthRate), datInput.TotalMonths) - (Decimal)Math.Pow((double)(1 + monthRate), i)) / d3, 2)
                                };

                                //var diff = rst.TranAmount + rst.InterestAmount - monthRepay;
                                //if (diff != 0)
                                //{
                                //    rst.TranAmount -= diff;
                                //    rst.TranAmount = Math.Round(rst.TranAmount, 2);
                                //}

                                totalInterestAmt += rst.InterestAmount;

                                listResults.Add(rst);
                            }
                        }
                        break;

                    case LoanRepaymentMethod.EqualPrincipal:
                        {
                            // 每月月供额 = (贷款本金÷还款月数) + (贷款本金 - 已归还本金累计额)×月利率
                            // 每月应还本金 = 贷款本金÷还款月数
                            // 每月应还利息 = 剩余本金×月利率 = (贷款本金 - 已归还本金累计额)×月利率
                            // 每月月供递减额 = 每月应还本金×月利率 = 贷款本金÷还款月数×月利率
                            // 总利息 = 还款月数×(总贷款额×月利率 - 月利率×(总贷款额÷还款月数)*(还款月数 - 1)÷2 + 总贷款额÷还款月数)
                            Decimal monthRate = datInput.InterestRate / 12;
                            Decimal totalAmt = datInput.TotalAmount;
                            var monthPrincipal = datInput.TotalAmount / datInput.TotalMonths;

                            for (int i = 0; i < datInput.TotalMonths; i++)
                            {
                                var rst = new LoanCalcResult
                                {
                                    TranDate = datInput.StartDate.AddMonths(i + 1),
                                    TranAmount = Math.Round(monthPrincipal, 2),
                                    InterestAmount = Math.Round(totalAmt * monthRate, 2)
                                };

                                totalAmt -= monthPrincipal;

                                listResults.Add(rst);
                            }
                        }
                        break;

                    case LoanRepaymentMethod.DueRepayment: {
                            // 
                        }
                        break;

                    default: break;
                }
            }

            return listResults;
        }
    }
}
