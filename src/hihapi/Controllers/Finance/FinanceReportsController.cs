using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Formatter;
using System.Threading.Tasks;

namespace hihapi.Controllers.Finance
{
    [Authorize]
    public class FinanceReportsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportsController(hihDataContext context)
        {
            _context = context;
        }

        // Actions
        [HttpPost]
        public IActionResult GetMonthlyReportByTranType([FromBody] ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }

                return BadRequest();
            }

            // 0. Get inputted parameter
            Int32 hid = (Int32)parameters["HomeID"];
            Int32 year = (Int32)parameters["Year"];
            Int32 month = (Int32)parameters["Month"];

            // 1. Check User
            String usrName = String.Empty;
            try
            {
                usrName = HIHAPIUtility.GetUserID(this);
                if (String.IsNullOrEmpty(usrName))
                {
                    throw new UnauthorizedAccessException();
                }
            }
            catch
            {
                throw new UnauthorizedAccessException();
            }

            // 2. Check the Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            // 3. Calculate the amount
            DateTime dtlow = new DateTime(year, month, 1);
            DateTime dthigh = dtlow.AddMonths(1);
            var results = (from item in _context.FinanceDocumentItemView
                          where item.HomeID == hid
                            && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                          group item by new { item.TransactionType, item.TransactionTypeName, item.IsExpense } into newresult                          
                          select new
                          {
                              HomeID = hid,
                              TransactionType = newresult.Key.TransactionType,
                              TransactionTypeName = newresult.Key.TransactionTypeName,
                              IsExpense = newresult.Key.IsExpense,
                              Amount = newresult.Sum(c => c.Amount)
                          }).ToList();
            List<FinanceReportByTransactionType> listResult = new List<FinanceReportByTransactionType>();
            foreach(var result in results)
            {
                FinanceReportByTransactionType financeReportByTransactionType = new FinanceReportByTransactionType(); 
                financeReportByTransactionType.HomeID = result.HomeID;
                financeReportByTransactionType.TransactionTypeName = result.TransactionTypeName;
                financeReportByTransactionType.TransactionType = result.TransactionType;
                if (result.IsExpense)
                    financeReportByTransactionType.OutAmount = result.Amount;
                else
                    financeReportByTransactionType.InAmount = result.Amount;
                listResult.Add(financeReportByTransactionType);
            }

            return Ok(listResult);
        }
    }
}
