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
using System.Reflection;
using System.Text;
using hihapi.Models;
using Microsoft.AspNet.OData.Routing;
using hihapi.Utilities;

namespace hihapi.Controllers
{
    public class DBVersionsController : ODataController
    {
        // Version 4 - 2018.07
        // Version 5 - 2018.08.02
        // Version 6 - 2018.08.05
        // Version 7 - 2018.09.11
        // Version 8 - 2018.09.15
        // Version 9 - 2018.09.16
        // Version 10 - 2018.10.13
        // Version 11 - 2018.12.18
        // Version 12 - 2019.4.20
        public static Int32 CurrentVersion = 12;

        private readonly hihDataContext _context;

        public DBVersionsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /DBVersions
        [EnableQuery]
        public IQueryable<DBVersion> Get()
        {
            return _context.DBVersions;
        }

        /// GET: /DBVersions(:vid)
        [EnableQuery]
        public SingleResult<DBVersion> Get([FromODataUri] int vid)
        {
            return SingleResult.Create(_context.DBVersions.Where(p => p.VersionID == vid));
        }

        // POST: /DBVersions
        /// <summary>
        /// Checking DB version
        /// </summary>
        public async Task<IActionResult> Post()
        {
            var lastestVersion = await _context.DBVersions.MaxAsync(p => p.VersionID);
            if (lastestVersion++ < CurrentVersion)
            {
                while (lastestVersion <= CurrentVersion)
                {
                    var sqlfile = $"hihapi.Sqls.Delta.v{lastestVersion}.sql";

                    try
                    {
                        var asmy = typeof(DBVersionsController).GetTypeInfo().Assembly;
                        //var resourceNames = asmy.GetManifestResourceNames();
                        using var stream = asmy.GetManifestResourceStream(sqlfile);
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        
                        var strcontent = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(strcontent))
                        {
                            throw new Exception("Empty file");
                        }

                        if (!_context.TestingMode)
                        {
                            using var dbContextTransaction = _context.Database.BeginTransaction();
                            _context.Database.ExecuteSqlRaw(strcontent);
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            _context.DBVersions.Add(new DBVersion
                            {
                                VersionID = lastestVersion,
                                AppliedDate = DateTime.Today
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);

                        // ApplicationProvider.WriteToLog<EmbeddedResource>().Error(exception.Message);
                        throw new Exception($"Failed to read Embedded Resource {sqlfile}, reason: {exception.Message}");
                    }

                    ++lastestVersion;
                }

                if (_context.TestingMode)
                {
                    await _context.SaveChangesAsync();
                }
            }
            var dbv = new DBVersion
            {
                VersionID = CurrentVersion,
                AppliedDate = DateTime.Today
            };

            return Created(dbv);
        }

        [HttpGet]
        [ODataRoute("GetRepeatedDates(input={input})")]
        public IActionResult GetRepeatedDates([FromODataUri] RepeatDatesCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception != null? err.Exception.Message : err.ErrorMessage);
                    }
                }
#endif
                return BadRequest();
            }

            return Ok(CommonUtility.WorkoutRepeatedDates(input));
        }

        [HttpGet]
        [ODataRoute("GetRepeatedDatesWithAmount(input={input})")]
        public IActionResult GetRepeatedDatesWithAmount([FromODataUri] RepeatDatesWithAmountCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
#if DEBUG
                foreach (var value in ModelState.Values)
                {
                    foreach (var err in value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Exception != null ? err.Exception.Message : err.ErrorMessage);
                    }
                }
#endif
                return BadRequest();
            }

            return Ok(CommonUtility.WorkoutRepeatedDatesWithAmount(input));
        }
    }
}
