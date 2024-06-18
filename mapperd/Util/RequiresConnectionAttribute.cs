using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace mapperd.Util;

public class RequiresConnectionAttribute : Attribute, IAuthorizationFilter
{
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Items.ContainsKey("Connection"))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}