using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using hihapi.Models;
using hihapi.Utilities;
using hihapi.Exceptions;
using Microsoft.AspNet.OData.Query;

namespace hihapi.Controllers.Finance
{
    public sealed class FinanceTmpLoanDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceTmpLoanDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceTmpLoanDocument> Get()
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

            return _context.FinanceTmpLoanDocument;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostRepayDocument([FromBody]FinanceLoanRepayDocumentCreateContext createContext)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception?.Message);
                    }
                }
#endif

                return BadRequest("Model State is Invalid");
            }


            // Checks
            // Check 0: Input data check

            // Check 1: User
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


            // The post here is:
            // 1. Post a repayment document with the content from this template doc
            // 2. Update the template doc with REFDOCID
            // 3. If the account balance is zero, close the account;


            // Check 1: Home ID
            var hms = _context.HomeMembers.Where(p => p.HomeID == createContext.HomeID && p.User == usrName).Count();
            if (hms <= 0)
            {
                throw new UnauthorizedAccessException();
            }

            //// Check 2: Document posted already?
            //if (loandoc.ReferenceDocumentID.HasValue && loandoc.ReferenceDocumentID.Value > 0)
            //{
            //    return BadRequest("Tmp Doc not existed yet or has been posted");
            //}
            //if (!loandoc.ControlCenterID.HasValue && !loandoc.OrderID.HasValue)
            //{
            //    return BadRequest("Tmp doc lack of control center or order");
            //}
            //if (loandoc.TranAmount == 0)
            //{
            //    return BadRequest("Tmp doc lack of amount");
            //}

            //// Save it to normal doc.
            var findoc = new FinanceDocument();
            //findoc.Desp = loandoc.Description;
            //findoc.DocType = FinanceDocumentType.DocType_Normal;
            //findoc.HID = hid;
            ////findoc.TranAmount = vmTmpDoc.TranAmount;
            //findoc.TranCurr = vmHome.BaseCurrency;
            //findoc.TranDate = loandoc.TransactionDate;
            //findoc.CreatedAt = DateTime.Now;
            //var findocitem = new FinanceDocumentItem
            //{
            //    AccountID = loandoc.AccountID
            //};
            //if (loandoc.ControlCenterID.HasValue)
            //    findocitem.ControlCenterID = loandoc.ControlCenterID.Value;
            //if (loandoc.OrderID.HasValue)
            //    findocitem.OrderID = loandoc.OrderID.Value;
            //findocitem.Desp = loandoc.Description;
            //findocitem.ItemID = 1;
            //findocitem.TranAmount = loandoc.TranAmount;
            //findocitem.TranType = loandoc.TransactionType;
            //findoc.Items.Add(findocitem);

            return Created(findoc);
        }
    }
}
