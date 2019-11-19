using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.ODataControllers
{
    public class HomeDefinitionQueryableAttribute : EnableQueryAttribute
    {
        //
        // Summary:
        //     Validates the OData query in the incoming request. By default, the implementation
        //     throws an exception if the query contains unsupported query parameters. Override
        //     this method to perform additional validation of the query.
        //
        // Parameters:
        //   request:
        //     The incoming request.
        //
        //   queryOptions:
        //     The Microsoft.AspNet.OData.Query.ODataQueryOptions instance constructed based
        //     on the incoming request.
        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            //if(queryOptions == null || queryOptions.Filter == null)
            //{
            //    throw new ODataErrorException("The filter is must.");
            //}
            //else
            //{
            //    // Check the filter is equal to member
            //}
        }
    }
}
