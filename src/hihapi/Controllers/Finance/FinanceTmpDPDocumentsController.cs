using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;

namespace hihapi.Controllers
{
    [Authorize]
    public sealed class FinanceTmpDPDocumentsController : ODataController
    {
        private readonly hihDataContext _context;

        public FinanceTmpDPDocumentsController(hihDataContext context)
        {
            _context = context;
        }

        [EnableQuery]
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
        public async Task<IActionResult> PostDocument([FromBody]FinanceTmpDPDocumentPostContext context)
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

            var tpdoc = _context.FinanceTmpDPDocument
                .Where(p => p.DocumentID == context.DocumentID && p.AccountID == context.AccountID && p.HomeID == context.HomeID)
                .SingleOrDefault();
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
            var homes = hms.ToList();

            // Check 2: Document posted already?
            if (tpdoc.ReferenceDocumentID.HasValue && tpdoc.ReferenceDocumentID.Value > 0)
            {
                throw new BadRequestException("Tmp Doc not existed yet or has been posted");
            }
            if (!tpdoc.ControlCenterID.HasValue && !tpdoc.OrderID.HasValue)
            {
                throw new BadRequestException("Tmp doc lack of control center or order");
            }
            if (tpdoc.TranAmount == 0)
            {
                throw new BadRequestException("Tmp doc lack of amount");
            }

            // Left items
            var dpAccount = _context.FinanceAccount.Where(p => p.ID == context.AccountID && p.HomeID == context.HomeID).SingleOrDefault();
            if (dpAccount == null)
            {
                throw new BadRequestException("Cannot find Account");
            }
            else if (!(dpAccount.Status == null || dpAccount.Status == (Byte)FinanceAccountStatus.Normal))
            {
                throw new BadRequestException("Account status is not Normal");
            }

            var leftItemsCnt = _context.FinanceTmpDPDocument
                .Where(p => p.AccountID == context.AccountID && p.HomeID == context.HomeID && p.DocumentID != context.DocumentID && p.ReferenceDocumentID == null)
                .Count();

            // Save it to normal doc.
            var findoc = new FinanceDocument();
            findoc.Desp = tpdoc.Description;
            findoc.DocType = FinanceDocumentType.DocType_Normal;
            findoc.HomeID = tpdoc.HomeID;
            //findoc.TranAmount = vmTmpDoc.TranAmount;
            findoc.TranCurr = homes[0].BaseCurrency;
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

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Create the document
                    findoc.CreatedAt = DateTime.Now;
                    findoc.Createdby = usrName;
                    var docEntity = _context.FinanceDocument.Add(findoc);
                    _context.SaveChanges();
                    origdocid = docEntity.Entity.ID;

                    // 2. Update the tmp doc
                    tpdoc.ReferenceDocumentID = origdocid;
                    tpdoc.UpdatedAt = DateTime.Now;
                    tpdoc.Updatedby = usrName;
                    _context.FinanceTmpDPDocument.Update(tpdoc);
                    _context.SaveChanges();

                    // 3. Close DP account
                    if (leftItemsCnt == 0)
                    {
                        dpAccount.Status = (Byte)FinanceAccountStatus.Closed;
                        dpAccount.Updatedby = usrName;
                        dpAccount.UpdatedAt = DateTime.Now;
                        _context.FinanceAccount.Update(dpAccount);
                        _context.SaveChanges();
                    }

                    findoc = docEntity.Entity;

                    await transaction.CommitAsync();
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
                throw new DBOperationException(errorString);
            }

            return Created(findoc);
        }
    }
}
