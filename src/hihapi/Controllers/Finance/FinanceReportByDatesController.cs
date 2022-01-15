using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using hihapi.Models;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    [Authorize]
    public class FinanceReportByDatesController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceReportByDatesController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [HomeIDEnableQuery]
        public IActionResult Get()
        {
            //String usrName = String.Empty;
            //try
            //{
            //    usrName = HIHAPIUtility.GetUserID(this);
            //    if (String.IsNullOrEmpty(usrName))
            //        throw new UnauthorizedAccessException();
            //}
            //catch
            //{
            //    throw new UnauthorizedAccessException();
            //}

            //List<FinanceReportControlCenterGroupAndExpenseView> arsts
            //    = (from hmem in _context.HomeMembers
            //       where hmem.User == usrName
            //       select new { HomeID = hmem.HomeID } into hids
            //       join bal in _context.FinanceReportControlCenterGroupAndExpenseView on hids.HomeID equals bal.HomeID
            //       select bal).ToList();
            //List<FinanceControlCenter> listCC = (from hmem in _context.HomeMembers
            //                                     where hmem.User == usrName
            //                                     select new { hmem.HomeID, hmem.IsChild, hmem.User } into hmems
            //                                     join cc in _context.FinanceControlCenter 
            //                                        on hmems.HomeID equals cc.HomeID
            //                                     where (hmems.IsChild == true && hmems.User == cc.Owner)
            //                                         || !hmems.IsChild.HasValue
            //                                         || hmems.IsChild == false
            //                                     select cc).ToList();

            //List<FinanceReportByControlCenter> listRsts = new List<FinanceReportByControlCenter>();
            //foreach (var cc in listCC)
            //{
            //    var rst2 = new FinanceReportByControlCenter();
            //    rst2.ControlCenterID = cc.ID;
            //    arsts.ForEach(action =>
            //    {
            //        if (action.ControlCenterID == cc.ID)
            //        {
            //            if (action.IsExpense)
            //            {
            //                rst2.CreditBalance = -1 * action.Balance;
            //            }
            //            else
            //            {
            //                rst2.DebitBalance = action.Balance;
            //            }
            //        }
            //    });
            //    rst2.Balance = rst2.DebitBalance - rst2.CreditBalance;
            //    rst2.HomeID = cc.HomeID;

            //    listRsts.Add(rst2);
            //}

            //return Ok(listRsts.ToList<FinanceReportByControlCenter>());
            return BadRequest();
        }
    }
}
