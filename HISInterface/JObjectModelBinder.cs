using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace HISInterface
{
    public class JObjectModelBinder : IModelBinder
    {
        public JObjectModelBinder(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException("bindingContext");
            ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            try
            {
                JObject obj = new JObject();
                if (bindingContext.ModelType == typeof(JObject))
                {
                    foreach (var item in bindingContext.ActionContext.HttpContext.Request.Form)
                    {
                        obj.Add(new JProperty(item.Key.ToString(), item.Value.ToString()));
                    }
                    if ((obj.Count == 0))
                    {
                        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(result.ToString()));
                        return Task.CompletedTask;
                    }
                    bindingContext.Result = (ModelBindingResult.Success(obj));
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                if (!(exception is FormatException) && (exception.InnerException != null))
                {
                    exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                }
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exception, bindingContext.ModelMetadata);
                return Task.CompletedTask;
            }
        }
    }
}