using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    public class FinanceReportByTransactionTypesController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByTransactionTypesController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<FinanceReportByControlCenter> Get()
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

            List<FinanceReportControlCenterGroupAndExpenseView> arsts
                = (from hmem in _context.HomeMembers
                   where hmem.User == usrName
                   select new { HomeID = hmem.HomeID } into hids
                   join bal in _context.FinanceReportControlCenterGroupAndExpenseView on hids.HomeID equals bal.HomeID
                   select bal).ToList();
            List<FinanceControlCenter> listCC = (from hmem in _context.HomeMembers
                                                 where hmem.User == usrName
                                                 select new { HomeID = hmem.HomeID } into hids
                                                 join cc in _context.FinanceControlCenter on hids.HomeID equals cc.HomeID
                                                 select cc).ToList();

            List<FinanceReportByControlCenter> listRsts = new List<FinanceReportByControlCenter>();
            foreach (var cc in listCC)
            {
                var rst2 = new FinanceReportByControlCenter();
                rst2.ControlCenterID = cc.ID;
                arsts.ForEach(action =>
                {
                    if (action.ControlCenterID == cc.ID)
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
                rst2.HomeID = cc.HomeID;

                listRsts.Add(rst2);
            }

            return listRsts.AsQueryable<FinanceReportByControlCenter>();
        }
    }
}