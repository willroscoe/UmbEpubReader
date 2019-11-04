using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Web;
using Wr.UmbEpubReader.Helpers;

namespace Wr.UmbEpubReader.Routing
{
    /// <summary>
    /// Custom Route for Book
    /// </summary>
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            AppSettingsConfig appSettingsConfig = new AppSettingsConfig();

            routes.MapUmbracoRoute("EpubBookCustomRoute",
                    appSettingsConfig.BooksPathSegment + "/{booknameid}/" + appSettingsConfig.ReadPathSegment + "/{*readparameters}", // get paths sections for the app settings in web.config
                    new
                    {
                        controller = "UmbEpubReader",
                        action = "UmbEpubReader_Read",
                        booknameid = "",
                        readparameters = UrlParameter.Optional
                    },
                    new BookContentFinderByNiceUrl()); // this UmbracoVirtualNodeRouteHandler allows '.' in the url so the plugin can route/serve files (embeded files in the epub)
        }
    }
}