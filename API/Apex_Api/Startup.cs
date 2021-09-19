using Apex.DataAccess;
using Apex.DataAccess.Models;
using ElmahCore;
using ElmahCore.Mvc;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling.Storage;
using System;
using System.Text;
using Utility = Apex.DataAccess.Utility;

namespace Apex_Api
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
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddCors(options => options.AddPolicy("CorsPolicy", xyz =>
            {
                xyz
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .WithOrigins("*")
                   .AllowCredentials();
            }));

            services.AddSingleton<IValidator<Setting>, SettingsValidator>();

            //services.AddControllers();
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSetting>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSetting>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddFluentValidation();
            services.AddSingleton<IConfiguration>(Configuration);
            // Register the Swagger generator, defining 1 or more Swagger documents
            Utility utility = new Utility(Configuration);

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            var apiVersion = "3.7.7";

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "APEX API", Version = "v1", Description = "Version: " + apiVersion });

                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    //Scheme = "bearer",
                    //BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme.",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "bearer"
                                }
                            },
                            new string[] {}
                    }
                });

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            });

            //Logs
            string contentRoot = services.BuildServiceProvider()
                             .GetService<IHostingEnvironment>()
                             .ContentRootPath;
            services.AddElmah<XmlFileErrorLog>(options =>
            {
                options.LogPath = contentRoot + "\\log"; // OR options.LogPath = "E:\errors";
                options.ApplicationName = "APEX_API";
            });
            //
            services.AddMemoryCache();
            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                // (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                // (default is everyone can access profilers)
                //options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                // Or, there are async versions available:
                //options.ResultsAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
                //options.ResultsAuthorizeListAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

                // (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
                // (default is everything should be profiled)
                //options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                // (Optional) Profiles are stored under a user ID, function to get it:
                // (default is null, since above methods don't use it by default)
                //options.UserIdProvider = request => MyGetUserIdFunction(request);

                // (Optional) Swap out the entire profiler provider, if you want
                // (default handles async and works fine for almost all applications)
                //options.ProfilerProvider = new MyProfilerProvider();

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                // (Optional) Use something other than the "light" color scheme.
                // (defaults to "light")
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

                // The below are newer options, available in .NET Core 3.0 and above:

                // (Optional) You can disable MVC filter profiling
                // (defaults to true, and filters are profiled)
                //options.EnableMvcFilterProfiling = true;
                // ...or only save filters that take over a certain millisecond duration (including their children)
                // (defaults to null, and all filters are profiled)
                // options.MvcFilterMinimumSaveMs = 1.0m;

                // (Optional) You can disable MVC view profiling
                // (defaults to true, and views are profiled)
                //options.EnableMvcViewProfiling = true;
                // ...or only save views that take over a certain millisecond duration (including their children)
                // (defaults to null, and all views are profiled)
                // options.MvcViewMinimumSaveMs = 1.0m;

                // (Optional) listen to any errors that occur within MiniProfiler itself
                // options.OnInternalError = e => MyExceptionLogger(e);

                // (Optional - not recommended) You can enable a heavy debug mode with stacks and tooltips when using memory storage
                // It has a lot of overhead vs. normal profiling and should only be used with that in mind
                // (defaults to false, debug/heavy mode is off)
                //options.EnableDebugMode = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseMiniProfiler();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseAuthentication();
            app.UseElmah();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "APEX API V1");
            });

            app.UseCors(builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials());

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}