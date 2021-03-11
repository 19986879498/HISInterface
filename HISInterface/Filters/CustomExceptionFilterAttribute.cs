using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HISInterface.Filters
{
    public class CustomExceptionFilterAttribute:ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                Console.WriteLine($"程序出现异常：{context.Exception.Message}");
                context.Result = new ObjectResult(new {code=500,Msg="出现异常"+context.Exception.Message });
                context.ExceptionHandled = true;
            }
            base.OnException(context);
        }
    }
}
