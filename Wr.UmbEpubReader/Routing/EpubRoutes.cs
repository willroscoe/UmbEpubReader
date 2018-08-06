using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Configuration;
using Umbraco.Web;
using Wr.UmbEpubReader.Extensions;
using Wr.UmbEpubReader.Helpers;

namespace Wr.UmbEpubReader.Routing
{
    public static class EpubRoutes
    {
        public static void Configure()
        {
            RouteTable.Routes.MapUmbracoRoute("BookCustomRoute",
                    UmbracoConfig.For.UmbEpubReader().BooksPathSegment + "/{booknameid}/" + UmbracoConfig.For.UmbEpubReader().ReadPathSegment + "/{*readparameters}", // get paths sections for the app settings in web.config
                    new
                    {
                        controller = "UmbEpubReader",
                        action = "UmbEpubReader_Read",
                        booknameid = "",
                        readparameters = UrlParameter.Optional
                    },
                    new BookContentFinderByNiceUrl());
        }
    }
}