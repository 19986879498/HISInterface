using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HISInterface.DBContext;
using HISInterface.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace HISInterface
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(item=> {
                item.ModelBinderProviders.Insert(0, new JObjectModelBinderProvider());
                item.Filters.Add(typeof(CustomExceptionFilterAttribute));
            });
            services.AddTransient<CustomExceptionFilterAttribute>();
            services.AddDbContextPool<DBContext.DB>(db => db.UseOracle(this.Configuration["OrclDBStrCSK"].ToString(), item => item.UseOracleSQLCompatibility("11")));
            services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Latest).AddNewtonsoftJson();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CustomCorsPolicy", policy =>
            //    {
            //        // 设定允许跨域的来源，有多个可以用','隔开
            //        policy.WithOrigins("http://localhost:6500")//只允许https://localhost:6500来源允许跨域
            //        .AllowAnyHeader()
            //        .AllowAnyMethod()
            //        .AllowCredentials();
            //    });
            //});
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HISInterface", Version = "v1" });
                // 为 Swagger JSON and UI设置xml文档注释路径
                
                var basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                //添加swagger注释
                var xmlPath = Path.Combine(basePath, "HISInterface.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HISInterface v1"));
          //  app.UseCors("CustomCorsPolicy");
           // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
