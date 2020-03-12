using hihapi.Exceptions;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Controllers
{
    public sealed class FinanceReportByOrdersController: ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByOrdersController(hihDataContext context)
        {
            _context = context;
        }

        [Authorize]
        [EnableQuery]
        public IQueryable<FinanceReportByOrder> Get()
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

            List<FinanceReportOrderBalanceView> arsts
                = (from hmem in _context.HomeMembers
                   where hmem.User == usrName
                   select new { HomeID = hmem.HomeID } into hids
                   join bal in _context.FinanceReportOrderBalanceView on hids.HomeID equals bal.HomeID
                   select bal).ToList();

            List<FinanceReportByOrder> listRsts = new List<FinanceReportByOrder>();
            foreach (var rst in arsts)
            {
                var rst2 = new FinanceReportByOrder();
                rst2.OrderID = rst.OrderID;
                rst2.Balance = rst.Balance;
                rst2.CreditBalance = rst.CreditBalance;
                rst2.DebitBalance = rst.DebitBalance;
                rst2.HomeID = rst.HomeID;

                listRsts.Add(rst2);
            }

            return listRsts.AsQueryable<FinanceReportByOrder>();
        }
    }
}
