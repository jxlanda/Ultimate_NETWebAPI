using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;

namespace NET5.WebAPI.Extensions
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILoggerManager logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                        var ex = contextFeature.Error;
                        if (ex.InnerException != null)
                        {

                            try
                            {
                                if (((SqlException)ex.InnerException).Number == 2627)
                                {
                                    await context.Response.WriteAsync(new ErrorDetails()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = "Duplicate ID Error"
                                    }.ToString());

                                }
                                else if (((SqlException)ex.InnerException).Number == 53)
                                {
                                    await context.Response.WriteAsync(new ErrorDetails()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = "Can't Connect to DB"
                                    }.ToString());
                                }
                                else if (((SqlException)ex.InnerException).Number == 15)
                                {
                                    await context.Response.WriteAsync(new ErrorDetails()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = "Reference"
                                    }.ToString());

                                }
                                else
                                {
                                    await context.Response.WriteAsync(new ErrorDetails()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = $"Internal Server Error: {ex.Message}"
                                    }.ToString());
                                }
                            }
                            catch
                            {
                                await context.Response.WriteAsync(new ErrorDetails()
                                {
                                    StatusCode = context.Response.StatusCode,
                                    Message = "Can't Connect to DB"
                                }.ToString());
                            }
                        }
                        else
                        {
                            await context.Response.WriteAsync(new ErrorDetails()
                            {
                                StatusCode = context.Response.StatusCode,
                                Message = $"Internal Server Error: {ex.Message}"
                            }.ToString());
                        } 
                        
                    }
                });
            });
        }
    }
}
