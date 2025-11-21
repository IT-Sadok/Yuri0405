using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequiredHeaderAttribute:ActionFilterAttribute
{
    private readonly string _headerName;

    public RequiredHeaderAttribute(string headerName)
    {
        _headerName = headerName;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var headerValue) ||
            string.IsNullOrWhiteSpace(headerValue))
        {
            context.Result = new BadRequestObjectResult(new
            {
               error = $"{_headerName} header is required"
            });
            return;
        }

        base.OnActionExecuting(context);
    }
}