using ServerSide.Hubs;

namespace ServerSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();
            builder.Services.AddCors(option =>
            {
                option.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .SetIsOriginAllowed(isOrigin => true);
                });
            });
            var app = builder.Build();

            app.UseCors();
            app.MapGet("/", () => "Hello World!");
            app.MapHub<ChatHub>("/chathub");
            app.Run();
        }
    }
}
