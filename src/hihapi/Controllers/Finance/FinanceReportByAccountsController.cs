using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    [Authorize]
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
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                    throw new UnauthorizedAccessException();
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            List<FinanceReporAccountGroupAndExpenseView> arsts 
                = (from hmem in _context.HomeMembers
                   where hmem.User == usrName
                   select new { HomeID = hmem.HomeID } into hids
                   join bal in _context.FinanceReporAccountGroupAndExpenseView on hids.HomeID equals bal.HomeID
                   select bal).ToList();

            List<FinanceAccount> accounts = (from hmem in _context.HomeMembers
                                               where hmem.User == usrName
                                               select new { hmem.HomeID, hmem.IsChild, hmem.User } into hmems
                                             join acnt in _context.FinanceAccount 
                                                on hmems.HomeID equals acnt.HomeID
                                               where (hmems.IsChild == true && hmems.User == acnt.Owner)
                                                   || !hmems.IsChild.HasValue
                                                   || hmems.IsChild == false
                                             select acnt).ToList();

            List<FinanceReportByAccount> listRsts = new List<FinanceReportByAccount>();
            foreach(var acnt in accounts)
            {
                var rst2 = new FinanceReportByAccount();
                rst2.AccountID = acnt.ID;
                arsts.ForEach(action =>
                {
                    if (action.AccountID == acnt.ID)
                    {
                        if (action.IsExpense)
                        {
                            rst2.CreditBalance = -1 * action.Balance;
                        }
                        else
                        {
                            rst2.DebitBalance = action.Balance;
                        }
                    }
                });
                rst2.Balance = rst2.DebitBalance - rst2.CreditBalance;
                rst2.HomeID = acnt.HomeID;
                
                listRsts.Add(rst2);
            }

            return listRsts.AsQueryable<FinanceReportByAccount>();
        }
    }
}
