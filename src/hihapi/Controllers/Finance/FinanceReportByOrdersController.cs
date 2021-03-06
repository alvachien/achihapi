﻿using hihapi.Exceptions;
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

            List<FinanceReportOrderGroupAndExpenseView> arsts
                = (from hmem in _context.HomeMembers
                   where hmem.User == usrName
                   select new { HomeID = hmem.HomeID } into hids
                   join bal in _context.FinanceReportOrderGroupAndExpenseView on hids.HomeID equals bal.HomeID
                   select bal).ToList();
            List<FinanceOrder> orders = (from hmem in _context.HomeMembers
                                         where hmem.User == usrName
                                         select new { HomeID = hmem.HomeID } into hids
                                         join ord in _context.FinanceOrder on hids.HomeID equals ord.HomeID
                                         select ord).ToList();

            List<FinanceReportByOrder> listRsts = new List<FinanceReportByOrder>();
            foreach (var ord in orders)
            {
                var rst2 = new FinanceReportByOrder();
                rst2.OrderID = ord.ID;

                arsts.ForEach(action =>
                {
                    if (action.OrderID == ord.ID)
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

                rst2.HomeID = ord.HomeID;

                listRsts.Add(rst2);
            }

            return listRsts.AsQueryable<FinanceReportByOrder>();
        }
    }
}
