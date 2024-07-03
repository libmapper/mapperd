using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace mapperd.Util;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
        if (actionContext.ModelState.IsValid == false)
        {
            actionContext.Result = new BadRequestResult();
        }
    }
}

public class RequirePropertiesResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var def = base.GetTypeInfo(type, options);
        foreach (var prop in def.Properties)
        {
            if (!prop.PropertyType.IsAssignableFrom(typeof(Nullable<>)) && !prop.IsExtensionData)
            {
                prop.IsRequired = true;
            }
        }

        return def;
    }
}