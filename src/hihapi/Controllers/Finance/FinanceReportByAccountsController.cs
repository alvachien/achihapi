using hihapi.Models;
using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    public sealed class FinanceReportByAccountsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByAccountsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IQueryable<FinanceReportByAccount> Get()
        {
            List<FinanceReportAccountBalanceView> arsts 
                = (from acntbal in _context.FinanceReportAccountBalanceView
                   select acntbal).ToList();

            List<FinanceReportByAccount> listRsts = new List<FinanceReportByAccount>();
            foreach(var rst in arsts)
            {
                var rst2 = new FinanceReportByAccount();
                rst2.AccountID = rst.AccountID;
                rst2.Balance = rst.Balance;
                rst2.CreditBalance = rst.CreditBalance;
                rst2.DebitBalance = rst.DebitBalance;
                rst2.HomeID = rst.HomeID;
                
                listRsts.Add(rst2);
            }

            return listRsts.AsQueryable<FinanceReportByAccount>();
        }
    }
}
