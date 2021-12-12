using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.ModelBuilder.Config;

namespace hihapi.Utilities
{
    public class HomeIDQueryValidator : FilterQueryValidator
    {
        //public HomeIDQueryValidator(DefaultQuerySettings setting): base(setting)
        //{
        //}

        //public override void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
        //{
        //    // Todo: how??!!
        //    if (filterQueryOption.FilterClause.Expression != null)
        //    {

        //    }
        //    base.Validate(filterQueryOption, settings);
        //}        

        //public override void ValidateAllNode(AllNode allNode, ODataValidationSettings settings)
        //{
        //    // NOT WORKING!!!
        //    foreach (var node in allNode.RangeVariables)
        //    {
        //        // if (node.Kind == )
        //        if (node != null)
        //        {

        //        }
        //    }

        //    base.ValidateAllNode(allNode, settings);
        //}
    }
}
