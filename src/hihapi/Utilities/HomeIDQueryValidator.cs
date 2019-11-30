using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData.UriParser;

namespace hihapi.Utilities
{
    public class HomeIDQueryValidator : FilterQueryValidator
    {
        public HomeIDQueryValidator(DefaultQuerySettings setting): base(setting)
        {
        }

        public override void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
        {
            // Todo: how??!!
            if (filterQueryOption.FilterClause.Expression != null)
            {

            }
            base.Validate(filterQueryOption, settings);
        }        

        public override void ValidateAllNode(AllNode allNode, ODataValidationSettings settings)
        {
            // NOT WORKING!!!
            foreach (var node in allNode.RangeVariables)
            {
                // if (node.Kind == )
                if (node != null)
                {

                }
            }

            base.ValidateAllNode(allNode, settings);
        }
    }
}
