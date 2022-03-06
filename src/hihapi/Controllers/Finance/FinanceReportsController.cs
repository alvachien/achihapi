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
        const String MoMPeriod_Last12Month = "1";   // Last 12 months
        const String MoMPeriod_Last6Month = "2";    // Last 6 months
        const String MoMPeriod_Last3Month = "3";    // Last 3 months

        public FinanceReportsController(hihDataContext context)
        {
            _context = context;
        }

        // Actions
        [HttpPost]
        public IActionResult GetReportByTranType([FromBody] ODataActionParameters parameters)
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
            Int32? month = null;
            if (parameters.ContainsKey("Month"))
                month = (Int32?)parameters["Month"];

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
            DateTime dtlow = new DateTime(year, month == null ? 1 : month.Value, 1);
            DateTime dthigh = month == null ? dtlow.AddYears(1) : dtlow.AddMonths(1);
            var results = (from item in _context.FinanceDocumentItemView
                          where item.HomeID == hid
                            && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                            //&& item.TransactionType != FinanceTransactionType.TranType_TransferIn
                            //&& item.TransactionType != FinanceTransactionType.TranType_TransferOut
                            //&& item.TransactionType != FinanceTransactionType.TranType_OpeningAsset
                            //&& item.TransactionType != FinanceTransactionType.TranType_OpeningLiability
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
    
        [HttpPost]
        public IActionResult GetReportByAccount([FromBody]ODataActionParameters parameters)
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
            var results = (
                from docitem in _context.FinanceDocumentItem
                    join docheader in _context.FinanceDocument
                        on docitem.DocID equals docheader.ID
                    join trantype in _context.FinTransactionType
                        on docitem.TranType equals trantype.ID
                    join account in _context.FinanceAccount
                            on new { docitem.AccountID, IsNormal = true } equals new { AccountID = account.ID, IsNormal = account.Status == null || account.Status == (byte)FinanceAccountStatus.Normal }
                    where docheader.HomeID == hid
                select new
                {
                    AccountID = docitem.AccountID,
                    IsExpense = trantype.Expense,
                    TranCurr = docheader.TranCurr,
                    TranCurr2 = docheader.TranCurr2,
                    UseCurr2 = docitem.UseCurr2,
                    TranAmount = docitem.TranAmount,
                    docheader.ExgRate,
                    docheader.ExgRate2,
                }
                into docitem2
                group docitem2 by new { docitem2.AccountID, docitem2.IsExpense, docitem2.TranCurr, docitem2.TranCurr2, docitem2.UseCurr2, docitem2.ExgRate, docitem2.ExgRate2 } into docitem3
                select new
                {
                    AccountID = docitem3.Key.AccountID,
                    IsExpense = docitem3.Key.IsExpense,
                    TranCurr = docitem3.Key.TranCurr,
                    TranCurr2 = docitem3.Key.TranCurr2,
                    UseCurr2 = docitem3.Key.UseCurr2,
                    ExgRate = docitem3.Key.ExgRate,
                    ExgRate2 = docitem3.Key.ExgRate2,
                    TranAmount = docitem3.Sum(p => p.TranAmount)
                }).ToList();

            List<FinanceReportByAccount> listResults = new List<FinanceReportByAccount>();
            foreach(var rst in results)
            {
                var amountLC = rst.TranAmount;
                // Calculte the amount
                if (rst.IsExpense)
                    amountLC = -1 * rst.TranAmount;
                if (rst.UseCurr2 != null) 
                {
                    if (rst.ExgRate2 != null && rst.ExgRate2.GetValueOrDefault() > 0)
                    {
                        amountLC *= rst.ExgRate2.GetValueOrDefault();
                    }
                }
                else
                {
                    if (rst.ExgRate != null && rst.ExgRate.GetValueOrDefault() > 0)
                    {
                        amountLC *= rst.ExgRate.GetValueOrDefault();
                    }
                }

                // Does Account exist?
                var acntidx = listResults.FindIndex(p => p.AccountID == rst.AccountID);
                if (acntidx == -1)
                {
                    var nrst = new FinanceReportByAccount();
                    nrst.HomeID = hid;
                    nrst.AccountID = rst.AccountID;
                    if (rst.IsExpense)
                        nrst.CreditBalance += amountLC;
                    else
                        nrst.DebitBalance += amountLC;
                    nrst.Balance = nrst.DebitBalance + nrst.CreditBalance;
                    listResults.Add(nrst);
                }
                else
                {
                    if (rst.IsExpense)
                        listResults[acntidx].CreditBalance += amountLC;
                    else
                        listResults[acntidx].DebitBalance += amountLC;
                    listResults[acntidx].Balance = listResults[acntidx].DebitBalance + listResults[acntidx].CreditBalance;
                }
            }

            return Ok(listResults);
        }

        [HttpPost]
        public IActionResult GetReportByControlCenter([FromBody] ODataActionParameters parameters)
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
            var results = (
                from docitem in _context.FinanceDocumentItem
                join docheader in _context.FinanceDocument
                    on docitem.DocID equals docheader.ID
                join trantype in _context.FinTransactionType
                    on docitem.TranType equals trantype.ID
                where docheader.HomeID == hid && docitem.ControlCenterID != null
                select new
                {
                    ControlCenterID = docitem.ControlCenterID,
                    IsExpense = trantype.Expense,
                    TranCurr = docheader.TranCurr,
                    TranCurr2 = docheader.TranCurr2,
                    UseCurr2 = docitem.UseCurr2,
                    TranAmount = docitem.TranAmount,
                    docheader.ExgRate,
                    docheader.ExgRate2,
                }
                into docitem2
                group docitem2 by new { docitem2.ControlCenterID, docitem2.IsExpense, docitem2.TranCurr, docitem2.TranCurr2, docitem2.UseCurr2, docitem2.ExgRate, docitem2.ExgRate2 } into docitem3
                select new
                {
                    ControlCenterID = docitem3.Key.ControlCenterID,
                    IsExpense = docitem3.Key.IsExpense,
                    TranCurr = docitem3.Key.TranCurr,
                    TranCurr2 = docitem3.Key.TranCurr2,
                    UseCurr2 = docitem3.Key.UseCurr2,
                    ExgRate = docitem3.Key.ExgRate,
                    ExgRate2 = docitem3.Key.ExgRate2,
                    TranAmount = docitem3.Sum(p => p.TranAmount)
                }).ToList();

            List<FinanceReportByControlCenter> listResults = new List<FinanceReportByControlCenter>();
            foreach (var rst in results)
            {
                var amountLC = rst.TranAmount;
                // Calculte the amount
                if (rst.IsExpense)
                    amountLC = -1 * rst.TranAmount;
                if (rst.UseCurr2 != null)
                {
                    if (rst.ExgRate2 != null && rst.ExgRate2.GetValueOrDefault() > 0)
                    {
                        amountLC *= rst.ExgRate2.GetValueOrDefault();
                    }
                }
                else
                {
                    if (rst.ExgRate != null && rst.ExgRate.GetValueOrDefault() > 0)
                    {
                        amountLC *= rst.ExgRate.GetValueOrDefault();
                    }
                }

                // Does Account exist?
                var ccidx = listResults.FindIndex(p => p.ControlCenterID == rst.ControlCenterID);
                if (ccidx == -1)
                {
                    var nrst = new FinanceReportByControlCenter();
                    nrst.HomeID = hid;
                    nrst.ControlCenterID = rst.ControlCenterID.GetValueOrDefault();
                    if (rst.IsExpense)
                        nrst.CreditBalance += amountLC;
                    else
                        nrst.DebitBalance += amountLC;
                    nrst.Balance = nrst.DebitBalance + nrst.CreditBalance;
                    listResults.Add(nrst);
                }
                else
                {
                    if (rst.IsExpense)
                        listResults[ccidx].CreditBalance += amountLC;
                    else
                        listResults[ccidx].DebitBalance += amountLC;
                    listResults[ccidx].Balance = listResults[ccidx].DebitBalance + listResults[ccidx].CreditBalance;
                }
            }

            return Ok(listResults);
        }

        [HttpPost]
        public IActionResult GetReportByOrder([FromBody] ODataActionParameters parameters)
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
            Int32? orderid = null;
            if (parameters.ContainsKey("OrderID"))
                orderid = (Int32?)parameters["OrderID"];

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
            List<FinanceReportByOrder> listResults = new List<FinanceReportByOrder>();

            if (orderid != null)
            {
                var results = (
                    from docitem in _context.FinanceDocumentItem
                    join docheader in _context.FinanceDocument
                        on docitem.DocID equals docheader.ID
                    join trantype in _context.FinTransactionType
                        on docitem.TranType equals trantype.ID
                    where docheader.HomeID == hid && docitem.OrderID == orderid.Value
                    select new
                    {
                        IsExpense = trantype.Expense,
                        TranCurr = docheader.TranCurr,
                        TranCurr2 = docheader.TranCurr2,
                        UseCurr2 = docitem.UseCurr2,
                        TranAmount = docitem.TranAmount,
                        docheader.ExgRate,
                        docheader.ExgRate2,
                    }
                    into docitem2
                    group docitem2 by new { docitem2.IsExpense, docitem2.TranCurr, docitem2.TranCurr2, docitem2.UseCurr2, docitem2.ExgRate, docitem2.ExgRate2 } into docitem3
                    select new
                    {
                        IsExpense = docitem3.Key.IsExpense,
                        TranCurr = docitem3.Key.TranCurr,
                        TranCurr2 = docitem3.Key.TranCurr2,
                        UseCurr2 = docitem3.Key.UseCurr2,
                        ExgRate = docitem3.Key.ExgRate,
                        ExgRate2 = docitem3.Key.ExgRate2,
                        TranAmount = docitem3.Sum(p => p.TranAmount)
                    }).ToList();

                foreach (var rst in results)
                {
                    var amountLC = rst.TranAmount;
                    // Calculte the amount
                    if (rst.IsExpense)
                        amountLC = -1 * rst.TranAmount;
                    if (rst.UseCurr2 != null)
                    {
                        if (rst.ExgRate2 != null && rst.ExgRate2.GetValueOrDefault() > 0)
                        {
                            amountLC *= rst.ExgRate2.GetValueOrDefault();
                        }
                    }
                    else
                    {
                        if (rst.ExgRate != null && rst.ExgRate.GetValueOrDefault() > 0)
                        {
                            amountLC *= rst.ExgRate.GetValueOrDefault();
                        }
                    }

                    // Does Account exist?
                    if (listResults.Count == 0)
                    {
                        var nrst = new FinanceReportByOrder();
                        nrst.HomeID = hid;
                        nrst.OrderID = orderid.Value;
                        if (rst.IsExpense)
                            nrst.CreditBalance += amountLC;
                        else
                            nrst.DebitBalance += amountLC;
                        nrst.Balance = nrst.DebitBalance + nrst.CreditBalance;
                        listResults.Add(nrst);
                    }
                    else
                    {
                        if (rst.IsExpense)
                            listResults[0].CreditBalance += amountLC;
                        else
                            listResults[0].DebitBalance += amountLC;
                        listResults[0].Balance = listResults[0].DebitBalance + listResults[0].CreditBalance;
                    }
                }
            }
            else
            {
                var results = (
                    from docitem in _context.FinanceDocumentItem
                    join docheader in _context.FinanceDocument
                        on docitem.DocID equals docheader.ID
                    join trantype in _context.FinTransactionType
                        on docitem.TranType equals trantype.ID
                    where docheader.HomeID == hid && docitem.OrderID != null
                    select new
                    {
                        OrderID = docitem.OrderID,
                        IsExpense = trantype.Expense,
                        TranCurr = docheader.TranCurr,
                        TranCurr2 = docheader.TranCurr2,
                        UseCurr2 = docitem.UseCurr2,
                        TranAmount = docitem.TranAmount,
                        docheader.ExgRate,
                        docheader.ExgRate2,
                    }
                    into docitem2
                    group docitem2 by new { docitem2.OrderID, docitem2.IsExpense, docitem2.TranCurr, docitem2.TranCurr2, docitem2.UseCurr2, docitem2.ExgRate, docitem2.ExgRate2 } into docitem3
                    select new
                    {
                        OrderID = docitem3.Key.OrderID,
                        IsExpense = docitem3.Key.IsExpense,
                        TranCurr = docitem3.Key.TranCurr,
                        TranCurr2 = docitem3.Key.TranCurr2,
                        UseCurr2 = docitem3.Key.UseCurr2,
                        ExgRate = docitem3.Key.ExgRate,
                        ExgRate2 = docitem3.Key.ExgRate2,
                        TranAmount = docitem3.Sum(p => p.TranAmount)
                    }).ToList();

                foreach (var rst in results)
                {
                    var amountLC = rst.TranAmount;
                    // Calculte the amount
                    if (rst.IsExpense)
                        amountLC = -1 * rst.TranAmount;
                    if (rst.UseCurr2 != null)
                    {
                        if (rst.ExgRate2 != null && rst.ExgRate2.GetValueOrDefault() > 0)
                        {
                            amountLC *= rst.ExgRate2.GetValueOrDefault();
                        }
                    }
                    else
                    {
                        if (rst.ExgRate != null && rst.ExgRate.GetValueOrDefault() > 0)
                        {
                            amountLC *= rst.ExgRate.GetValueOrDefault();
                        }
                    }

                    // Does Account exist?
                    var ordidx = listResults.FindIndex(p => p.OrderID == rst.OrderID);
                    if (ordidx == -1)
                    {
                        var nrst = new FinanceReportByOrder();
                        nrst.HomeID = hid;
                        nrst.OrderID = rst.OrderID.GetValueOrDefault();
                        if (rst.IsExpense)
                            nrst.CreditBalance += amountLC;
                        else
                            nrst.DebitBalance += amountLC;
                        nrst.Balance = nrst.DebitBalance + nrst.CreditBalance;
                        listResults.Add(nrst);
                    }
                    else
                    {
                        if (rst.IsExpense)
                            listResults[ordidx].CreditBalance += amountLC;
                        else
                            listResults[ordidx].DebitBalance += amountLC;
                        listResults[ordidx].Balance = listResults[ordidx].DebitBalance + listResults[ordidx].CreditBalance;
                    }
                }
            }

            return Ok(listResults);
        }

        [HttpPost]
        public IActionResult GetFinanceOverviewKeyFigure([FromBody] ODataActionParameters parameters)
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
            bool excludeTransfer = (bool)parameters["ExcludeTransfer"];

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

            // 3. Calculate the key figure of current month.
            FinanceOverviewKeyFigure keyfigure = new FinanceOverviewKeyFigure();
            keyfigure.HomeID = hid;

            DateTime dtlow = new DateTime(year, month, 1);
            DateTime dthigh = dtlow.AddMonths(1);
            if (excludeTransfer)
            {
                var results = (from item in _context.FinanceDocumentItemView
                               where item.HomeID == hid
                                 && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                                 && item.TransactionType != FinanceTransactionType.TranType_TransferIn
                                 && item.TransactionType != FinanceTransactionType.TranType_TransferOut
                               group item by new { item.IsExpense } into newresult
                               select new
                               {
                                   HomeID = hid,
                                   IsExpense = newresult.Key.IsExpense,
                                   Amount = newresult.Sum(c => c.Amount)
                               }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.CurrentMonthOutgo = rst.Amount;
                    else
                        keyfigure.CurrentMonthIncome = rst.Amount;
                });
            }
            else
            {
                var results = (from item in _context.FinanceDocumentItemView
                           where item.HomeID == hid
                             && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                           group item by new { item.IsExpense } into newresult
                           select new
                           {
                               HomeID = hid,
                               IsExpense = newresult.Key.IsExpense,
                               Amount = newresult.Sum(c => c.Amount)
                           }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.CurrentMonthOutgo = rst.Amount;
                    else
                        keyfigure.CurrentMonthIncome = rst.Amount;
                });
            }

            // 4. Calculate the key figure of current year
            dtlow = new DateTime(year, 1, 1);
            dthigh = dtlow.AddYears(1);
            if (excludeTransfer)
            {
                var results = (from item in _context.FinanceDocumentItemView
                               where item.HomeID == hid
                                 && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                                 && item.TransactionType != FinanceTransactionType.TranType_TransferIn
                                 && item.TransactionType != FinanceTransactionType.TranType_TransferOut
                               group item by new { item.IsExpense } into newresult
                               select new
                               {
                                   HomeID = hid,
                                   IsExpense = newresult.Key.IsExpense,
                                   Amount = newresult.Sum(c => c.Amount)
                               }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.OutgoYTD = rst.Amount;
                    else
                        keyfigure.IncomeYTD = rst.Amount;
                });
            }
            else
            {
                var results = (from item in _context.FinanceDocumentItemView
                           where item.HomeID == hid
                             && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                           group item by new { item.IsExpense } into newresult
                           select new
                           {
                               HomeID = hid,
                               IsExpense = newresult.Key.IsExpense,
                               Amount = newresult.Sum(c => c.Amount)
                           }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.OutgoYTD = rst.Amount;
                    else
                        keyfigure.IncomeYTD = rst.Amount;
                });
            }

            // 5. Calculate the key figure of last month
            dtlow = new DateTime(year, month, 1).AddMonths(-1);
            dthigh = new DateTime(year, month, 1);
            if (excludeTransfer)
            {
                var results = (from item in _context.FinanceDocumentItemView
                           where item.HomeID == hid
                             && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                             && item.TransactionType != FinanceTransactionType.TranType_TransferIn
                             && item.TransactionType != FinanceTransactionType.TranType_TransferOut
                           group item by new { item.IsExpense } into newresult
                           select new
                           {
                               HomeID = hid,
                               IsExpense = newresult.Key.IsExpense,
                               Amount = newresult.Sum(c => c.Amount)
                           }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.LastMonthOutgo = rst.Amount;
                    else
                        keyfigure.LastMonthIncome = rst.Amount;
                });
            }
            else
            {
                var results = (from item in _context.FinanceDocumentItemView
                           where item.HomeID == hid
                             && item.TransactionDate >= dtlow && item.TransactionDate < dthigh
                           group item by new { item.IsExpense } into newresult
                           select new
                           {
                               HomeID = hid,
                               IsExpense = newresult.Key.IsExpense,
                               Amount = newresult.Sum(c => c.Amount)
                           }).ToList();
                results.ForEach(rst =>
                {
                    if (rst.IsExpense)
                        keyfigure.LastMonthOutgo = rst.Amount;
                    else
                        keyfigure.LastMonthIncome = rst.Amount;
                });
            }

            // 6. Currency
            keyfigure.Currency = (from hd in _context.HomeDefines
                                  where hd.ID == hid
                                  select hd.BaseCurrency).SingleOrDefault();

            // 7. Calculate the precentage
            if (keyfigure.LastMonthIncome != 0)
                keyfigure.CurrentMonthIncomePrecentage = 100 * (keyfigure.CurrentMonthIncome - keyfigure.LastMonthIncome) / keyfigure.LastMonthIncome;
            if (keyfigure.LastMonthOutgo != 0)
                keyfigure.CurrentMonthOutgoPrecentage = 100 * (keyfigure.CurrentMonthOutgo - keyfigure.LastMonthOutgo) / keyfigure.LastMonthOutgo;

            List < FinanceOverviewKeyFigure > listResult = new List<FinanceOverviewKeyFigure>();
            listResult.Add(keyfigure);

            return Ok(listResult);
        }

        [HttpPost]
        public IActionResult GetReportByTranTypeMOM([FromBody] ODataActionParameters parameters)
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
            Int32 ttid = (int)parameters["TransactionType"];
            Boolean includeChildren = false;
            if (parameters.ContainsKey("IncludeChildren"))
                includeChildren = (bool)parameters["IncludeChildren"];
            String period = (String)parameters["Period"];

            // 1. Check User
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

            // 2. Check the Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == hid && p.User == usrName).Count();
            if (hms <= 0)
                throw new UnauthorizedAccessException();

            // 3. Calculate the dates
            DateTime dtToday = DateTime.Today;
            DateTime dtbgn = DateTime.Today;
            DateTime dtNextMonth = DateTime.Today.AddMonths(1);
            DateTime dtNextMonthFirstDay = new DateTime(dtNextMonth.Year, dtNextMonth.Month, 1);
            DateTime dtend = new DateTime(dtNextMonth.Year, dtNextMonth.Month, 1).AddDays(-1);
            List<int> ttids = new List<int>();
            ttids.Add(ttid);
            if (includeChildren)
            {
                var lvl = (from fintt in _context.FinTransactionType
                          where fintt.ParID != null
                          && ttids.Contains(fintt.ParID.GetValueOrDefault())
                          select fintt.ID).ToList();
                ttids.AddRange(lvl);
            }
            
            if (String.CompareOrdinal(period, MoMPeriod_Last12Month) == 0)
            {
                dtbgn = dtNextMonthFirstDay.AddYears(-1);
            }
            else if (String.CompareOrdinal(period, MoMPeriod_Last6Month) == 0)
            {
                dtbgn = dtNextMonthFirstDay.AddMonths(-6);
            }
            else if (String.CompareOrdinal(period, MoMPeriod_Last3Month) == 0)
            {
                dtbgn = dtNextMonthFirstDay.AddMonths(-3);
            }
            else
                return BadRequest("Invalid Period");

            var results = (from item in _context.FinanceDocumentItemView
                           where item.HomeID == hid
                             && item.TransactionDate >= dtbgn && item.TransactionDate < dtend
                             && ttids.Contains(item.TransactionType)
                           //&& item.TransactionType != FinanceTransactionType.TranType_TransferIn
                           //&& item.TransactionType != FinanceTransactionType.TranType_TransferOut
                           //&& item.TransactionType != FinanceTransactionType.TranType_OpeningAsset
                           //&& item.TransactionType != FinanceTransactionType.TranType_OpeningLiability
                           group item by new { item.TransactionType, item.TransactionTypeName, item.IsExpense, item.TransactionDate.Month } into newresult
                           select new
                           {
                               HomeID = hid,
                               TransactionType = newresult.Key.TransactionType,
                               TransactionTypeName = newresult.Key.TransactionTypeName,
                               IsExpense = newresult.Key.IsExpense,
                               Month = newresult.Key.Month,
                               Amount = newresult.Sum(c => c.Amount)
                           }).ToList();

            List<FinanceReportByTransactionTypeMOM> listResult = new List<FinanceReportByTransactionTypeMOM>();
            foreach (var result in results)
            {
                FinanceReportByTransactionTypeMOM financeReportByTransactionType = new FinanceReportByTransactionTypeMOM();
                financeReportByTransactionType.HomeID = result.HomeID;
                financeReportByTransactionType.TransactionTypeName = result.TransactionTypeName;
                financeReportByTransactionType.TransactionType = result.TransactionType;
                if (result.IsExpense)
                    financeReportByTransactionType.OutAmount = result.Amount;
                else
                    financeReportByTransactionType.InAmount = result.Amount;
                financeReportByTransactionType.Month = result.Month;
                listResult.Add(financeReportByTransactionType);
            }

            return Ok(listResult);

        }
    }
}
