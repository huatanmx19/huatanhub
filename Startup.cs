using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using System;
using HuatanHub.Data;
using HuatanHub.Hubs;
using Microsoft.EntityFrameworkCore;

namespace HuatanHub
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
            var connectionString = Environment.GetEnvironmentVariable("CONN_STR")
                                   ?? Configuration["ConnectionString"];

            services.AddDbContext<ApiContext>(options => options
                .UseSqlServer(connectionString,
                    x => x.UseNetTopologySuite()));

            services.AddCors(options =>
            {
                options.AddPolicy(name: "CorsPolicy",
                    builder =>
                    {
                        builder
                            .WithOrigins(
                                "https://huatan.firebaseapp.com",
                                "https://huatan.web.app",
                                "https://huatan.app",
                                "https://huatan-hub.firebaseapp.com",
                                "https://huatan-hub.web.app",
                                "https://huatan.app:70",
                                "http://70.37.53.118:70",
                                "http://localhost:4200",
                                "http://34.71.46.142"
                            )
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HuatanHub", Version = "v1.1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HuatanHub v1.1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<LocationHub>("/location");
            });
        }
    }
}
