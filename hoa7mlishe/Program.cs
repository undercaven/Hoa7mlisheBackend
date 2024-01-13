using hoa7mlishe.API.Services;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Trading;
using hoa7mlishe.API.Trading.Services;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.Interfaces;
using hoa7mlishe.Interfaces.Server;
using hoa7mlishe.Services;
using hoa7mlishe.Trading.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Logging.AddConsole();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "hoa7mlisheAPI", Version = "v1" });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.WithOrigins("https://www.hoa7mlishe.com", "http://localhost:5173", "https://localhost:5173", "https://dev.hoa7mlishe.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        ServerServices.SetServiceContainer(new AppServiceContainer());

        builder.Services.AddTransient<IMailService, MailService>();
        builder.Services.AddTransient<ICardsService, CardsService>();
        builder.Services.AddTransient<IUserRequestService, UserRequestService>();
        builder.Services.AddTransient<ITradesService, TradesService>();
        builder.Services.AddTransient<IFileService, FileService>();

        builder.Services.AddSignalR();

        builder.Services.AddDbContext<Hoa7mlisheContext>();
        builder.WebHost.UseUrls("http://localhost:76/");
        var app = builder.Build();
        // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

        app.UseCors("CorsPolicy");
        app.MapHub<TradesHub>("/trades");

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
           ForwardedHeaders.XForwardedProto
        });

        app.UseAuthorization();

        app.MapControllers();

        LoadPlugins(app.Services);
        app.Run();
    }

    private static void LoadPlugins(IServiceProvider serviceProvider)
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Plugins";
        if (!Path.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        foreach (string dll in Directory.GetFiles(path, "*.dll"))
        {
            IPlugin[] entryPoints = GetInterfaceImplementor<IPlugin>(dll);
            if (entryPoints.Length == 1)
            {
                entryPoints[0].Load(serviceProvider);
            }
        }
    }

    private static T[] GetInterfaceImplementor<T>(string directory)
    {
        if (String.IsNullOrEmpty(directory)) { return null; } //sanity check

        var info = new DirectoryInfo(directory);
        if (!info.Exists) { return null; } //make sure directory exists

        var implementors = new List<T>();

        foreach (FileInfo file in info.GetFiles("*.dll")) //loop through all dll files in directory
        {
            Assembly currentAssembly = null;
            try
            {
                var name = AssemblyName.GetAssemblyName(file.FullName);
                currentAssembly = Assembly.Load(name);
            }
            catch (Exception ex)
            {
                continue;
            }

            currentAssembly.GetTypes()
                .Where(t => t != typeof(T) && typeof(T).IsAssignableFrom(t))
                .ToList()
                .ForEach(x => implementors.Add((T)Activator.CreateInstance(x)));
        }
        return implementors.ToArray();
    }
}