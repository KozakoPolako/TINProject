using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using czatSerwerTIN.Hubs;


namespace czatSerwerTIN.Startup
{
    class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.MaximumReceiveMessageSize = 20971520;
            });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:3000/")
                        .AllowCredentials();
                });
            });

        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            //app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CzatHub>("/czatHub", options => 
                {
                    options.ApplicationMaxBufferSize = 20971520;
                    options.TransportMaxBufferSize = 20971520;
                });
            });
        }
    }
}


