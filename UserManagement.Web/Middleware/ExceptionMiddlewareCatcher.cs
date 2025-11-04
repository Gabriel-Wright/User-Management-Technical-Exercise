using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;


/// <summary>
/// This class is intended to catch errors that occur on Service Layer, that are either not caught at 
/// Controller Level e.g. Checking if an email is unique - or if there's some Man in the Middle attack, 
/// e.g. data is maliciously altered between layers. Overall - aim is to provide neater responses back to client-side.
/// 
/// I have exposed the error messages in the HTTP Responses for ValidationException, ArgumentException,
/// InvalidOperationException since I do not think the data in error messages should be secretive?
/// 
/// </summary>
public class ExceptionMiddlewareCatcher
{
    private readonly RequestDelegate _next;

    public ExceptionMiddlewareCatcher(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            //can show error message here, Validation issue
            Log.Warning(ex, "Validation failed.");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            Log.Warning(ex, "Invalid argument.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            Log.Warning(ex, "Operation not allowed.");
            context.Response.StatusCode = StatusCodes.Status409Conflict; // 409 Conflict
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            //
            Log.Warning(ex, "No item found.");
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "No item found." });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}
