using System.Web.Routing;
using Umbraco.Core.Composing;
using Wr.UmbEpubReader.Routing;

namespace Wr.UmbEpubReader.Components
{
    
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