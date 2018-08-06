using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Wr.UmbEpubReader.Routing
{
    /// <summary>
	/// Provides an implementation of <see cref="UmbracoVirtualNodeRouteHandler"/> that handles /books/{url friendly book name}/read/{url friendly chapter name} urls.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice url of a document.</para>
	/// </remarks>
    public class BookContentFinderByNiceUrl : UmbracoVirtualNodeRouteHandler
    {
        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>IPublishedContent</c>.
        /// </summary>		
        /// <returns>IPublishedContent</returns>
        protected override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext)
        {
            var bookFriendlyUrl = requestContext.RouteData.Values["booknameid"].ToString(); // get the url friendly name of the book

            var bookBaseRoute = requestContext.HttpContext.Request.Url.Segments[1]?.ToString(); // get the first path segment i.e. books/

            if (umbracoContext == null)
                return null;

            var node = umbracoContext.ContentCache.GetByRoute("/" + bookBaseRoute + bookFriendlyUrl); // build the book base url i.e. /books/my-book 

            return node;
        }

        /// <summary>
        /// This allows for urls with '.' in them to be routed to the book controller - so the EpubServer class can serve/send the requested files embeded in the epub file to the response buffer
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        protected override UmbracoContext GetUmbracoContext(RequestContext requestContext)
        {
            var ctx = base.GetUmbracoContext(requestContext);
            //check if context is null, we know it will be null if we are dealing with a request that
            //has an extension and by default no Umb ctx is created for the request
            if (ctx == null)
            {
                //Here you can EnsureContext , please note that the requestContext is passed in 
                //therefore your should refrain from using other singletons like HttpContext.Current since
                //you will already have a reference to it. Also if you need an ApplicationContext you should
                //pass this in via a ctor instead of using the ApplicationContext.Current singleton.
                return UmbracoContext.EnsureContext(
                requestContext.HttpContext,
                ApplicationContext.Current,
                new WebSecurity(requestContext.HttpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
            }

            return base.GetUmbracoContext(requestContext);
        }
    }
}