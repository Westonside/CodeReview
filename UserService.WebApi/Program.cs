using System.Reflection;
using System.Text.Encodings.Web;
using FreeSql;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Serilog;
using Serilog.Events;
using UserService.Dao;
using UserService.Misc.FreeSql;
using UserService.Misc.Serializer;
using UserService.Service;
using UserService.WebApi.Extensions;

#region Log

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
             // .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .CreateLogger();

#endregion


try
{
    Log.Information("=== Starting web host ===");
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Log.Information("=== Begin printing env ===");
    // foreach (var c in builder.Configuration.AsEnumerable())
    // {
    //     Console.WriteLine(c.Key + " = " + c.Value);
    // }
    //
    // Log.Information("=== End printing env ===");


    builder.Host.UseSerilog();
    // remove Kestrel server header
    builder.WebHost.ConfigureKestrel(options => { options.AddServerHeader = false; });


    // Add services to the container.

    #region Services

    builder.Services.AddControllers()
           .AddJsonOptions(options =>
           {
               // no JSON escaping
               // options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
               options.JsonSerializerOptions.Encoder =
                   JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
           }).AddMvcOptions(options => // allow empty string in dto
               options.ModelMetadataDetailsProviders.Add(new EmptyStringAllowedModelMetadataProvider()));


    // check validation error output
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            List<string> errors = context.ModelState
                                         .Values
                                         .SelectMany(x => x.Errors.Select(p => p.ErrorMessage))
                                         .ToList();
            // only show error un dev env
            return builder.Environment.IsDevelopment()
                ? MyResult.Invalid(data: new { errors })
                : MyResult.Invalid();
        };
    });

    builder.Services.AddHttpContextAccessor();

    // Swagger API
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service", Version = "v1", Description = "Web API" });
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        options.OrderActionsBy(o => o.RelativePath);
    });

    // auth
    // services.AddAuthentication(options =>
    // {
    //     options.AddScheme<headerAuthHandler>(headerAuthHandler.SCHEME_NAME, "default scheme(headerAuth)");
    //     options.DefaultAuthenticateScheme = headerAuthHandler.SCHEME_NAME;
    //     options.DefaultChallengeScheme = headerAuthHandler.SCHEME_NAME;
    // });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
    });

    // DB
    IFreeSql db = builder.Environment.IsDevelopment()
        ? // no parameterized command under dev env, print sql
        new FreeSqlBuilder()
            // .UseConnectionString(FreeSql.DataType.MySql, builder.Configuration["DBConnStr"])
            .UseConnectionFactory(DataType.MySql, () => new MySqlConnection(builder.Configuration["DBConnStr"]))
            .UseMonitorCommand(cmd => Log.Debug("\n\nSQL: {CommandText}\n", cmd.CommandText))
            .UseNoneCommandParameter(true)
            // .UseGenerateCommandParameterWithLambda(false)
            .Build()
        : // prod env
        new FreeSqlBuilder()
            .UseConnectionFactory(DataType.MySql, () => new MySqlConnection(builder.Configuration["DBConnStr"]))
            // .UseConnectionString(DataType.MySql, builder.Configuration["DBConnStr"])
            .UseNoneCommandParameter(false)
            .UseGenerateCommandParameterWithLambda(true)
            .Build();


    builder.Services.AddSingleton(db);
    builder.Services.AddScoped<ITransactionFreeSql>(sp => new TransactionFreeSql(db));

    // AutoMapper
    // builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

    // === Dao ===
    builder.Services.AddScoped<IUserDao, UserDao>();

    // === Service ===
    builder.Services.AddScoped<IUserService, UserService.Service.UserService>();

    #endregion


    WebApplication app = builder.Build();

    // Configure the HTTP request pipeline.

    #region Pipeline & Middleware

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                // options.RoutePrefix = "";
            });
    }

    app.UseSerilogRequestLogging();

    app.UseCors();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    #endregion

    app.Urls.Add("http://localhost:5000");
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
