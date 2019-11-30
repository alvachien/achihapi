using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hihapi.Utilities
{
    public class HomeIDEnableQueryAttribute: EnableQueryAttribute
    {
        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            return base.ApplyQuery(queryable, queryOptions);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }

        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            if (queryOptions.Filter != null)
            {
                // Filter is must!
                var settings = new DefaultQuerySettings
                {
                    EnableFilter = true,
                    EnableSkipToken = true,
                    EnableCount = true,
                    EnableExpand = true,
                    EnableOrderBy = true,
                    EnableSelect = true,
                    MaxTop = 100,                    
                };

                queryOptions.Filter.Validator = new HomeIDQueryValidator(settings);
            }

            base.ValidateQuery(request, queryOptions);
        }
    }
}
