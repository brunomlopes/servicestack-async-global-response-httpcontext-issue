using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Funq;
using ServiceStack.Web;

namespace ServiceStack.Hello
{
    /// <summary>
    /// Define your ServiceStack web service request (i.e. the Request DTO).
    /// </summary>
    [Route("/hello")]
    [Route("/hello/{name}")]
    public class Hello :IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }
    
    [Route("/hello-sync")]
    [Route("/hello-sync/{name}")]
    public class HelloSync :IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// 
    /// Define your ServiceStack web service response (i.e. Response DTO).
    /// </summary>
    public class HelloResponse
    {
        public string Result { get; set; }
    }

    /// <summary>
    /// Create your ServiceStack web service implementation.
    /// </summary>
    public class HelloService : IService
    {
        public async Task<HelloResponse> Any(Hello request)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            // This blows up because HttpContext.Current is null
            return new HelloResponse { Result = $"Hello, {request.Name ?? "John Doe"}. ContextContains: {HttpContext.Current.User?.Identity}"};
        }
        
        public HelloResponse Any(HelloSync request)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(250));
            return new HelloResponse { Result = $"Hello, {request.Name ?? "John Doe"}. ContextContains: {HttpContext.Current.User?.Identity}"};
        }
    }

    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Create your ServiceStack web service application with a singleton AppHost.
        /// </summary>        
        public class HelloAppHost : AppHostBase
        {
            /// <summary>
            /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
            /// </summary>
            public HelloAppHost() : base("Hello Web Services", typeof(HelloService).Assembly)
            {
                Plugins.Add(new APlugin());
            }

            /// <summary>
            /// Configure the container with the necessary routes for your ServiceStack application.
            /// </summary>
            /// <param name="container">The built-in IoC used with ServiceStack.</param>
            public override void Configure(Container container)
            {
                container.Register((IAppHost)this);
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            //Initialize your application
            (new HelloAppHost()).Init();
        }
    }

    public class APlugin : IPlugin
    {
        public void Register(IAppHost appHost)
        { 
            // Both fail
            appHost.GlobalResponseFilters.Add(SetupRequestForgeryCookies.ResponseFilter);
            //appHost.GlobalResponseFiltersAsync.Add(SetupRequestForgeryCookies.ResponseFilterAsync);
        }
    }

    public class SetupRequestForgeryCookies
    {
        public static void ResponseFilter(IRequest req, IResponse res, object dto)
        {
            if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext.Current is null");

        }

        public static Task ResponseFilterAsync(IRequest req, IResponse res, object dto)
        {
            ResponseFilter(req, res, dto);
            return Task.CompletedTask;
        }
    }
}
