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

namespace hihapi.Controllers
{
    public sealed class FinanceTmpDPDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceTmpDPDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [Authorize]
        public IQueryable<FinanceTmpDPDocument> Get()
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

            return from homemem in _context.HomeMembers
                    join fintdpd in _context.FinanceTmpDPDocument
                    on new { homemem.HomeID, homemem.User } equals new { fintdpd.HomeID, User = usrName }
                    select fintdpd;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostDocument(int DocID, int AccontID, int HomeID)
        {
            // User
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

            var tpdoc = await _context.FinanceTmpDPDocument.FindAsync();
            if (tpdoc == null)
            {
                return NotFound();
            }

            // Check 1: Home ID
            var hms = from hmem in _context.HomeMembers
                                where hmem.User == usrName && hmem.HomeID == tpdoc.HomeID
                           select new { HomeID = hmem.HomeID } into hids
                           join homedefs in _context.HomeDefines 
                           on hids.HomeID equals homedefs.ID
                           select new { HomeID = homedefs.ID, BaseCurrency = homedefs.BaseCurrency };
            if (hms.Count() != 1)
            {
                throw new UnauthorizedAccessException();
            }

            // Check 2: Document posted already?
            if (tpdoc.ReferenceDocumentID.HasValue && tpdoc.ReferenceDocumentID.Value > 0)
            {
                return BadRequest("Tmp Doc not existed yet or has been posted");
            }
            if (!tpdoc.ControlCenterID.HasValue && !tpdoc.OrderID.HasValue)
            {
                return BadRequest("Tmp doc lack of control center or order");
            }
            if (tpdoc.TranAmount == 0)
            {
                return BadRequest("Tmp doc lack of amount");
            }

            // Save it to normal doc.
            var findoc = new FinanceDocument();
            findoc.Desp = tpdoc.Description;
            findoc.DocType = FinanceDocumentType.DocType_Normal;
            findoc.HomeID = tpdoc.HomeID;
            //findoc.TranAmount = vmTmpDoc.TranAmount;
            findoc.TranCurr = hms.ElementAt(0).BaseCurrency;
            findoc.TranDate = tpdoc.TransactionDate;
            findoc.CreatedAt = DateTime.Now;
            var findocitem = new FinanceDocumentItem
            {
                AccountID = tpdoc.AccountID
            };
            if (tpdoc.ControlCenterID.HasValue)
                findocitem.ControlCenterID = tpdoc.ControlCenterID.Value;
            if (tpdoc.OrderID.HasValue)
                findocitem.OrderID = tpdoc.OrderID.Value;
            findocitem.Desp = tpdoc.Description;
            findocitem.ItemID = 1;
            findocitem.TranAmount = tpdoc.TranAmount;
            findocitem.TranType = tpdoc.TransactionType;
            findoc.Items.Add(findocitem);

            // Database update
            var errorString = "";
            var errorOccur = false;
            var origdocid = 0;

            //_context.Database. = Console.WriteLine;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    var docEntity = _context.FinanceDocument.Add(findoc);
                    _context.SaveChanges();
                    origdocid = docEntity.Entity.ID;

                    // 2. Update the tmp doc
                    tpdoc.ReferenceDocumentID = origdocid;
                    _context.FinanceTmpDPDocument.Update(tpdoc);
                    _context.SaveChanges();

                    findoc = docEntity.Entity;

                    transaction.Commit();
                }
                catch (Exception exp)
                {
                    errorOccur = true;
                    errorString = exp.Message;
                    transaction.Rollback();
                }
            }

            if (errorOccur)
            {
                return BadRequest(errorString);
            }

            return Created(findoc);
        }
    }
}