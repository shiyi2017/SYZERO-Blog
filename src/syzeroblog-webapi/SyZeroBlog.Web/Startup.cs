using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SyZero;
using SyZero.AspNetCore;
using SyZero.AutoMapper;
using SyZero.Log4Net;
using SyZero.Redis;
using SyZero.Web.Common;
using SyZeroBlog.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using Panda.DynamicWebApi;

namespace SyZeroBlog.Web
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
          
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());

            services.AddControllers().AddMvcOptions(options =>
            {
                options.Filters.Add(new SyZeroBlog.Web.Core.Authentication.AppVerificationFilter());
                options.Filters.Add(new SyZeroBlog.Web.Core.Authentication.AppExceptionFilter());
                options.Filters.Add(new SyZeroBlog.Web.Core.Authentication.AppResultFilter());
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new LongToStrConverter());
            });

            //��̬WebApi
            services.AddDynamicWebApi(new DynamicWebApiOptions()
            {
                DefaultApiPrefix = "/api/Service"
            });
            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SyZeroBlog�ӿ��ĵ�",
                    Description = "RESTful API for SyZeroBlog",
                    Contact = new OpenApiContact() { Name = "SYZERO", Email = "522112669@qq.com", Url = new Uri("http://test6.syzero.com") }
                });
                options.DocInclusionPredicate((docName, description) => true);
                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var xmlPath = Path.Combine(basePath, "SyZeroBlog.Web.xml");
                options.IncludeXmlComments(xmlPath);
            });
            #endregion

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //ʹ��AutoMapper
            builder.RegisterModule(new AutoMapperModule());
            //ʹ��EF�ִ�
            builder.RegisterModule(new SyZeroBlogEntityFrameworkModule(Configuration));
            //ʹ��SyZero
            builder.RegisterModule(new SyZeroModule());
            //ע�������
            builder.RegisterModule(new SyZeroControllerModule());
            //ע��Log4Net
            builder.RegisterModule(new Log4NetModule());
            //ע��Redis
            builder.RegisterModule(new RedisModule(Configuration));
            //ע�빫����
            builder.RegisterModule(new CommonModule());
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SyBlogManagement API V1");
                c.RoutePrefix = string.Empty;

            });
           
        }
    }
}
