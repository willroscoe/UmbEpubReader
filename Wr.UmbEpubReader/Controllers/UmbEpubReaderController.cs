using System;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Wr.UmbEpubReader.Helpers;

namespace Wr.UmbEpubReader.Controllers
{
    public class UmbEpubReaderController : RenderMvcController
    {

        private AppSettingsConfig appSettingsConfig = new AppSettingsConfig();

        public ActionResult Index()
        {
            return null;
        }

        public ActionResult UmbEpubReader_Read(ContentModel model, string booknameid = "", string readparameters = "")
        {
            var epubFileContent = model.Content.GetProperty("epubFile").GetValue();
            
            //if (!string.IsNullOrEmpty(epubFile))
            if (epubFileContent != null)
            {
                var epubFileContentMedia = Umbraco.Media(((IPublishedContent)epubFileContent).Id);
                var epubUrl = epubFileContentMedia.Url();
                var epubPath = Server.MapPath(epubUrl);

                var startAtChapter = Convert.ToInt32(model.Content.GetProperty("startAtChapter").GetValue() ?? 0); // a no chapter is requested then start at this chapter index. 0  = the first chapter

                EpubServer epub = new EpubServer(epubPath, readparameters, startAtChapter);

                if (epub.ProcessEpub())
                {
                    // try and get a cover image url from the Umbraco book page content
                    var cover = model.Content.GetProperty("bookCoverImage").GetValue();
                    
                    if (cover != null)
                    {
                        var coverMedia = Umbraco.Media(((IPublishedContent)cover).Id);
                        var coverUrl = coverMedia.Url();
                        if (!string.IsNullOrEmpty(coverUrl)) // cover image found in umbraco content for this book
                        {
                            epub.ePubDisplayModel.CoverImageUrl = coverUrl;
                        }
                    }
                    
                    ViewBag.Epub = epub.ePubDisplayModel; // store the book content in a ViewBag so the View can access it.
                }
                else // either a redirect or a embeded file to be served
                {
                    if (!string.IsNullOrEmpty(epub.ePubDisplayModel.RedirectToChapter)) // we need to redirect to this chapter probably because no chapter was requested
                    {
                        // build route url to redirect chapter i.e. /books/book_name/read/chapter_name
                        string redirectUrl = string.Format("/{0}/{1}/{2}/{3}", appSettingsConfig.BooksPathSegment, booknameid, appSettingsConfig.ReadPathSegment, epub.ePubDisplayModel.RedirectToChapter);

                        return Redirect(redirectUrl);
                    }
                    else if (epub.FileToServe != null) // this request is for a file embeded in the e-book
                    {
                        if (epub.FileToServe.IsValid())
                        {
                            Response.AppendHeader("Last-Modified", epub.FileToServe.LastModified); // allows the file to be cached in the users client (browser)
                            return File(epub.FileToServe.Data, epub.FileToServe.MimeType, epub.FileToServe.Filename); // serve the file and halt all
                        }
                        return null;
                    }
                }
            }

            return CurrentTemplate(model);
        }
    }
}