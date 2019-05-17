using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using ProxyKit;

namespace ReverseProxyApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy(httpClientBuilder => {
                httpClientBuilder.ConfigureHttpClient(
                    client => client.Timeout = TimeSpan.FromSeconds(5));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });

            app.RunProxy(async context =>
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent("Your response text")
                };
            });

            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseWhen(
                context => context.Request.Host.Host.Equals("metrics.test"),
                appInner => appInner.RunProxy(context =>
                {
                    Task<System.Net.Http.HttpResponseMessage> forwardContext = context.ForwardTo("http://localhost:5005")
                        .CopyXForwardedHeaders()
                        .AddXForwardedHeaders()
                        .Send();

                    return forwardContext;
                })
            );

            app.UseWhen(
                context => context.Request.Host.Host.Equals("metrics2.test"),
                appInner => appInner.RunProxy(context =>
                {
                    Task<System.Net.Http.HttpResponseMessage> forwardContext = context.ForwardTo("http://localhost:5005")
                        .CopyXForwardedHeaders()
                        .AddXForwardedHeaders()
                        .Send();

                    return forwardContext;
                })
            );

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
