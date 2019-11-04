using System.Web.Routing;
using Umbraco.Core.Composing;
using Wr.UmbEpubReader.Routing;

namespace Wr.UmbEpubReader.Components
{
    /// <summary>
    /// Register Routing - IComponent
    /// </summary>
    public class RoutingComponent : IComponent
    {
        public void Initialize()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public void Terminate()
        {
        }
    }
    
}