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
                            // 每月还本付息金额 = 本金 x 月利率 x (1+月利率)^贷款月数 ] / [(1+月利率)^(还款月数 - 1)]
                            // 每月利息 = 剩余本金 x 贷款月利率
                            // 还款总利息 = 贷款额 * 贷款月数 * 月利率* (1+月利率)^贷款月数 / (1+月利率) ^ (还款月数 - 1) - 贷款额
                            // 还款总额 = 还款月数 * 贷款额 * 月利率 * (1+月利率)^ 贷款月数 / [(1+月利率) ^ (还款月数 - 1)]
                        }
                        break;

                    case LoanRepaymentMethod.EqualPrincipal:
                        {
                            // 每月还本付息金额 = (本金/还款月数) + (本金-累计已还本金)×月利率
                            // 每月本金 = 总本金 / 还款月数
                            // 每月利息 = (本金 - 累计已还本金)×月利率 
                            // 还款总利息 =（还款月数 + 1）*贷款额 * 月利率 / 2
                            // 还款总额 = (还款月数 + 1) * 贷款额 * 月利率 / 2 + 贷款额
                        }
                        break;

                    case LoanRepaymentMethod.DueRepayment: {

                        }
                        break;

                    default: break;
                }
            }

            return listResults;
        }
    }
}
