using System;
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
        // Version 18 - 2022.5.1
        // Version 19 - 2022.10.31
        // Version 20 - 2022.8.31
        // Version 21 - 2022.9.30
        public const Int32 CurrentVersion = 21;

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
            // SQLite migration is handled by DatabaseSeeder.SeedAsync() on startup.
            // The delta SQL files (v1.sql–v21.sql) are legacy SQL Server scripts
            // and should not be executed against SQLite.
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
