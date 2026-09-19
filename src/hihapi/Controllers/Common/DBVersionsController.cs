using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using hihapi.Models;
using hihapi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace hihapi.Controllers
{
    [Authorize]
    public class DBVersionsController : ODataController
    {
        // The schema version is owned by DatabaseSeeder: it registers the upgrade
        // steps, runs the missing ones at startup and stamps T_DBVERSION. The
        // historical v1-v21 changelog lives with the delta scripts in Sqls/Delta/.
        public const Int32 CurrentVersion = DatabaseSeeder.CurrentVersion;

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
        [AllowAnonymous]
        public IActionResult Post()
        {
            // DatabaseSeeder.SeedAsync() applies missing upgrade steps and stamps
            // T_DBVERSION before the app starts listening, so the stored maximum is
            // the schema this database actually carries. The delta SQL files under
            // Sqls/Delta/ are legacy SQL Server scripts and are never executed.
            var storedVersion = _context.DBVersions.Any()
                ? _context.DBVersions.Max(v => v.VersionID)
                : 0;
            var dbv = new CheckVersionResult
            {
                StorageVersion = storedVersion.ToString(CultureInfo.InvariantCulture),
                APIVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            return Created(dbv);
        }

        [HttpGet("GetRepeatedDates2(StartDate={StartDate}, EndDate={EndDate}, RepeatType={RepeatType})")]
        public IActionResult GetRepeatedDates2([FromODataUri] string StartDate, string EndDate, int RepeatType)
        {
            var input = new RepeatDatesCalculationInput
            {
                StartDate = DateTime.Parse(StartDate, CultureInfo.InvariantCulture),
                EndDate = DateTime.Parse(EndDate, CultureInfo.InvariantCulture),
                RepeatType = (RepeatFrequency)RepeatType,
            };
            return Ok(CommonUtility.WorkoutRepeatedDates(input));
        }

        [HttpPost("GetRepeatedDates")]
        public IActionResult GetRepeatedDates([FromBody] RepeatDatesCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDates(input));
        }

        [HttpPost("GetRepeatedDatesWithAmount")]
        public IActionResult GetRepeatedDatesWithAmount([FromBody] RepeatDatesWithAmountCalculationInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDatesWithAmount(input));
        }

        [HttpPost("GetRepeatedDatesWithAmountAndInterest")]
        public IActionResult GetRepeatedDatesWithAmountAndInterest([FromBody] RepeatDatesWithAmountAndInterestCalInput input)
        {
            if (!ModelState.IsValid)
            {
                HIHAPIUtility.HandleModelStateError(ModelState);
            }

            return Ok(CommonUtility.WorkoutRepeatedDatesWithAmountAndInterest(input));
        }
    }
}
