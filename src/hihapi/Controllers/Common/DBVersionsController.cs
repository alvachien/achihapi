using System;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;

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
        // Version 13 - 2020.02.28
        // Version 14 - 2020.03.12 
        // Version 15 - 2020.04.01
        // Version 16 - 2020.04.15
        // Version 17 - 2020.09.12
        public static Int32 CurrentVersion = 17;

        private readonly hihDataContext _context;

        public DBVersionsController(hihDataContext context)
        {
            _context = context;
        }

        /// GET: /DBVersions
        [EnableQuery]
        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public IActionResult Get()
        {
            return Ok(_context.DBVersions);
        }

        /// GET: /DBVersions(:vid)
        [EnableQuery]
        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public IActionResult Get(int key)
        {
            return Ok(_context.DBVersions.FirstOrDefault(p => p.VersionID == key));
        }

        // POST: /DBVersions
        /// <summary>
        /// Checking DB version
        /// </summary>
        [HttpPost]
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
                            var subcontents = strcontent.Split("GO");
                            foreach(var content in subcontents)
                            {
                                _context.Database.ExecuteSqlRaw(content);
                            }
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
            var dbv = new CheckVersionResult
            {
                StorageVersion = CurrentVersion.ToString(),
                APIVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            return Created(dbv);
        }

        [HttpGet("GetRepeatedDates2(StartDate={StartDate}, EndDate={EndDate}, RepeatType={RepeatType})")]
        public IActionResult GetRepeatedDates2([FromODataUri] string StartDate, string EndDate, int RepeatType)
        {
            var input = new RepeatDatesCalculationInput
            {
                StartDate = DateTime.Parse(StartDate),
                EndDate = DateTime.Parse(EndDate),
                RepeatType = (RepeatFrequency)RepeatType,
            };
            return Ok(CommonUtility.WorkoutRepeatedDates(input));
        }

        [HttpPost("GetRepeatedDates")]
        public IActionResult GetRepeatedDates([FromBody] RepeatDatesCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDates(input));
        }

        [HttpPost("GetRepeatedDatesWithAmount")]
        public IActionResult GetRepeatedDatesWithAmount([FromBody] RepeatDatesWithAmountCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDatesWithAmount(input));
        }

        [HttpPost("GetRepeatedDatesWithAmountAndInterest")]
        public IActionResult GetRepeatedDatesWithAmountAndInterest([FromBody] RepeatDatesWithAmountAndInterestCalInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModalStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(input));
        }
    }
}
