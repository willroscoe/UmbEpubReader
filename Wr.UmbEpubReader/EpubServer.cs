using dotless.Core;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VersOne.Epub;
using Wr.UmbEpubReader.Helpers;
using Wr.UmbEpubReader.Models;

namespace Wr.UmbEpubReader
{
    public class EpubServer
    {
        /// <summary>
        /// File system path to the epub file 
        /// </summary>
        private readonly string _ePubFilePath;

        /// <summary>
        /// This string from the request contains either the url safe chapter title or the path to an embeded file in the e-book
        /// </summary>
        private readonly string _processAction;

        /// <summary>
        /// If _processAction is empty then dispay the chapter at index. 0 is the first chapter 
        /// </summary>
        private int _startAtIndex;

        /// <summary>
        /// Holder for front-end book content
        /// </summary>
        public EpubDisplayModel ePubDisplayModel;

        /// <summary>
        /// Does the chapter content need to be cached in the Application cache
        /// </summary>
        private bool _isToCache;

        /// <summary>
        /// The hash of the chapter title _ date to use as the cache key
        /// </summary>
        private string _cacheHash = "";

        /// <summary>
        /// Holder for any requested file that is embeded in the e-book 
        /// </summary>
        public FileToServe FileToServe;

        /// <summary>
        /// List of safe file type that can be served from the e-book file
        /// </summary>
        private List<string> SafeFileTypes = new List<string> { ".html", ".htm", ".xhtml", ".jpg", ".gif", ".png", ".css", ".ttf", ".otf", ".woff", ".woff2", ".svg" }; // applies to files embeded in the ebook file


        public EpubServer(string ePubFilePath, string processAction = "", int startAtIndex = 0, bool isToCache = false)
        { 
            _ePubFilePath = ePubFilePath;

            _isToCache = isToCache;

            _processAction = processAction;
            if (!string.IsNullOrEmpty(_processAction))
                _processAction = _processAction.ToLower();

            _startAtIndex = startAtIndex;
        }


        public bool ProcessEpub()
        {
            ePubDisplayModel = new EpubDisplayModel();

            // Opens a book and reads all of its content into memory
            var epubBookRef = EpubReader.OpenBook(_ePubFilePath); // not using ReadBook as that loads the whole epub book in memory
            if (epubBookRef != null)
            {
                // get file date
                var fileDateStamp = new FileInfo(_ePubFilePath).LastWriteTime;

                var bookContent = epubBookRef.Content;

                var requestIsForXhtml = false; // a whole epub xhtml file has been requested - this happens when a link in the html is selected as it could be a link anywhere in the whole book

                // *************************************************************************
                // check if this request is for a chapter or a file asset i.e. image or font
                // *************************************************************************

                var extension = Path.GetExtension(_processAction);

                if (!string.IsNullOrEmpty(extension)) // this might be a request for a file ie. font or image
                {
                    if (SafeFileTypes.Contains(extension)) // this filetype is in the SafeFileTypes list
                    {
                        Dictionary<string, EpubContentFileRef> allFiles = bookContent.AllFiles; // would use bookContent.css/fonts/images but fonts have missing mime-types in the EpubReader package, and so are noth being selected as fonts
                        foreach (var item in allFiles) // loop all the files in the epub archive
                        {
                            if (item.Key.EndsWith(_processAction, StringComparison.InvariantCultureIgnoreCase)) // this filename matches the requested action (_processAction)
                            {
                                switch (item.Value) // switch based on the class of the EpubContentFileRef type
                                {
                                    case EpubByteContentFileRef itemValue:

                                        FileToServe = new FileToServe(itemValue.ReadContentAsBytes(), itemValue.ContentMimeType, itemValue.FileName, fileDateStamp);

                                        return false; // return ProcessEPub()

                                    case EpubContentFileRef textItem:

                                        if (textItem.ContentType == EpubContentType.XHTML_1_1)
                                        {
                                            requestIsForXhtml = true; // we want to process this file as it is not a 'File' to serve, it is an HTML document to process
                                        }
                                        else // stop any other text file from being processed or served
                                        {
                                            FileToServe = new FileToServe(); // this will stop this file being served
                                            return false;  // return ProcessEPub()
                                        }
                                        break;

                                    default:
                                        FileToServe = new FileToServe(); // this will stop this file being served
                                        return false;  // return ProcessEPub()
                                }
                            }
                        }
                    }
                    else // disallow filetype
                    {
                        FileToServe = new FileToServe(); // this will stop this file being served
                        return false;  // return ProcessEPub()
                    }
                }

                // check if this chapter has been cached in Application memory. Is so then return that without continuing.
                if (_isToCache)
                {
                    _cacheHash = GetMd5Hash(_ePubFilePath + _processAction);// create unique hash of this chapter in this book
                    var cachedItem = HttpContext.Current.Cache.Get(_cacheHash);
                    if (cachedItem != null) // cached chapter found
                    {
                        ePubDisplayModel = (EpubDisplayModel)cachedItem;
                        return true;
                    }
                }

                // Book's title
                ePubDisplayModel.Title = epubBookRef.Title;

                // Book's authors (comma separated list)
                ePubDisplayModel.Authors = epubBookRef.Author;

                // try and find the requested chapter or xhtml file
                if (requestIsForXhtml)
                {
                    FindAndProcessXHTML(ref ePubDisplayModel, epubBookRef); // updates ePubDisplayModel by reference with content
                }
                else
                {
                    FindAndProcessChapter(ref ePubDisplayModel, epubBookRef); // updates ePubDisplayModel by reference with content
                }

                if (!string.IsNullOrEmpty(ePubDisplayModel.RedirectToChapter)) // we need to redirect to a chapter so exit this method
                    return false;

                // All fonts in the book (file name is the key)
                //Dictionary<string, EpubByteContentFileRef> fonts = bookContent.Fonts;

                // All CSS content in the book
                // All CSS files in the book (file name is the key)
                Dictionary<string, EpubTextContentFileRef> cssFiles = bookContent.Css;
                string cssContent = string.Empty;

                foreach (var cssFile in cssFiles.Values)
                {
                    cssContent += cssFile.ReadContentAsText();
                }

                // using dotless (css preprocessor) we will add '.epub' to all the book styles in order to keep the book styles restricted to the book and not affect the rest of the browser page.
                string finalCss = string.Format(".epub {{{0}}}", cssContent); // add '.epub' class to all css elements, so we can contain all the book styles to just to book div (which should have a .epub class added to it)

                var dotlessConfig = new dotless.Core.configuration.DotlessConfiguration
                {
                    MinifyOutput = true
                };

                var resultCss = Less.Parse(finalCss, dotlessConfig);

                // FOR REFERENCE IF NEEDED - <base href="'. $this->base_link. '/" target="_self"> // need to include <base> as some cms's adds a trailing '/' to all requests which brakes the relative links in the epub html
                ePubDisplayModel.ChapterHtml = string.Format("<html><head><style>{0}</style></head><body><div class='epub'>{1}</div></body></html>", resultCss, ePubDisplayModel.ChapterHtml);

                if (_isToCache)
                {
                    try
                    {
                        HttpContext.Current.Cache.Insert(_cacheHash, ePubDisplayModel, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Display the whole XHTML file (actually just the body innerhtml). This is generally needed to display the whole chapter/book if hyperlinks link to a targets in another chapter - in this case we display the whole book (if the whole book is in one xhtml file)
        /// </summary>
        /// <param name="ePubDisplayModel"></param>
        /// <param name="epubBookRef"></param>
        public void FindAndProcessXHTML(ref EpubDisplayModel ePubDisplayModel, EpubBookRef epubBookRef)
        {
            // build TOC
            foreach (var item in epubBookRef.GetChapters())
            {
                ePubDisplayModel.TOC_Items.Add(new EpubLink() { LinkTitle = item.Title }); // add TOC to display model
            }

            var foundContent = epubBookRef.Content.Html.Where(x => x.Key.EndsWith(_processAction, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (!string.IsNullOrEmpty(foundContent.Key))
            {
                var foundHtml = foundContent.Value.ReadContentAsText();

                var doc = new HtmlDocument();
                doc.LoadHtml(foundHtml ?? "");

                var bodyNodes = doc.DocumentNode.SelectSingleNode("//body"); // select everything inside the <body> tag
                ePubDisplayModel.ChapterHtml = bodyNodes.OuterHtml;
            }
        }


        /// <summary>
        /// Display the chapter. This might be a whole xhtml file or a section of an xhtml file between 2 'chapter' id's (or epub navnodes)
        /// </summary>
        /// <param name="ePubDisplayModel"></param>
        /// <param name="epubBookRef"></param>
        public void FindAndProcessChapter(ref EpubDisplayModel ePubDisplayModel, EpubBookRef epubBookRef)
        {
            EpubChapterHolder cHolder = new EpubChapterHolder();

            // Enumerating chapters
            var allChapters = epubBookRef.GetChapters();
            for (int i = 0; i < allChapters.Count; i++)
            {
                var _chapter = allChapters[i];
                string encTitle = EpubHelpers.EncodeChapterTitleForUrl(_chapter.Title);

                if (encTitle == _processAction && !cHolder.CurrentChapter.IsValid()) // chapter found! select this chapter to display
                {
                    cHolder.CurrentChapter.ChapterIndex = i;
                    cHolder.CurrentChapter.EpubChapter = _chapter;
                    cHolder.CurrentChapter.TitleAndLinkUrl.LinkTitle = _chapter.Title;
                }

                ePubDisplayModel.TOC_Items.Add(new EpubLink() { LinkTitle = _chapter.Title }); // add TOC to display model
            }

            if (!cHolder.CurrentChapter.IsValid()) // chapter not found - so get the default chapter and redirect to that
            {
                if (_startAtIndex < 0 || _startAtIndex >= allChapters.Count) // check if the _startAtIndex property is within valid limits - if not then set the index to 0
                {
                    _startAtIndex = 0;
                }

                var _chapter = allChapters[_startAtIndex];
                //cHolder.CurrentChapter.ChapterIndex = _startAtIndex;
                //cHolder.CurrentChapter.EpubChapter = _chapter;
                cHolder.CurrentChapter.TitleAndLinkUrl.LinkTitle = _chapter.Title;

                ePubDisplayModel.RedirectToChapter = cHolder.CurrentChapter.TitleAndLinkUrl.LinkUrl;
                return; // exit this method as we need to perform a redirect in the calling controller rather than 
            }

            ePubDisplayModel.TOC_Items[cHolder.CurrentChapter.ChapterIndex].IsCurrent = true;


            // try and find the previous chapter details
            int previousChapterIndex = cHolder.CurrentChapter.ChapterIndex - 1;

            if (previousChapterIndex >= 0) // check index is valid
            {
                var _chapter = allChapters[previousChapterIndex];
                cHolder.PreviousChapter.ChapterIndex = previousChapterIndex;
                cHolder.PreviousChapter.EpubChapter = _chapter;
                cHolder.PreviousChapter.TitleAndLinkUrl.LinkTitle = _chapter.Title;

                ePubDisplayModel.Nav_PreviousChapterLink.LinkTitle = _chapter.Title;
            }

            // try and find the next chapter details
            int nextChapterIndex = cHolder.CurrentChapter.ChapterIndex + 1;
            if (nextChapterIndex < allChapters.Count) // check index is valid
            {
                var _chapter = allChapters[nextChapterIndex];
                cHolder.NextChapter.ChapterIndex = nextChapterIndex;
                cHolder.NextChapter.EpubChapter = _chapter;
                cHolder.NextChapter.TitleAndLinkUrl.LinkTitle = _chapter.Title;

                ePubDisplayModel.Nav_NextChapterLink.LinkTitle = _chapter.Title;
            }

            ePubDisplayModel.ChapterHtml = BuildChapterHtml(cHolder);

        }


        private string BuildChapterHtml(EpubChapterHolder cHolder)
        {
            string result = string.Empty;
            int startTagPosition = 0;
            int lengthOfRequiredText = 0;

            // load current chapter html into HtmlAgilityPack to parse
            var doc = new HtmlDocument();
            doc.LoadHtml(cHolder.CurrentChapter.EpubChapter.ReadHtmlContent() ?? "");

            var bodyNodes = doc.DocumentNode.SelectSingleNode("//body"); // select everything inside the <body> tag

            var startNode = bodyNodes.SelectSingleNode(string.Format("//node()[@id='{0}']", cHolder.CurrentChapter.EpubChapter.Anchor)); // return the start node i.e. the start of the current chapter
            var startTag = startNode.OuterHtml;
            startTagPosition = bodyNodes.InnerHtml.IndexOf(startTag); // index of the start tag

            if (!string.IsNullOrEmpty(cHolder.NextChapter.EpubChapter?.Anchor ?? "")) // there is a next Chapter
            {
                var endNode = bodyNodes.SelectSingleNode(string.Format("//node()[@id='{0}']", cHolder.NextChapter.EpubChapter.Anchor)); // return the end node i.e. the start of the next chapter
                if (endNode != null) // next chapter found
                {
                    var endTag = endNode.OuterHtml;
                    int endTagPosition = bodyNodes.InnerHtml.IndexOf(endTag); // index of the end tag
                    lengthOfRequiredText = endTagPosition - startTagPosition;
                }
                else // next chapter is not in this chapter file
                {
                    // don't need to do anything here, as lengthOfRequiredText will remain 0
                }
            }

            var theHtml = string.Empty;
            if (lengthOfRequiredText > 0) // there is a next tag
                theHtml = bodyNodes.InnerHtml.Substring(startTagPosition, lengthOfRequiredText); // return the content beteen the start of the start tag and the start of the end tag
            else // no end tag so just get all the content to the end of the body
                theHtml = bodyNodes.InnerHtml.Substring(startTagPosition);


            // update all chapter links to use the friendly url
            var charSetOccurences = new Regex("href=\"(.+)#([^\"]+)\"", RegexOptions.IgnoreCase); // find all href links with anchors
            var charSetMatches = charSetOccurences.Matches(theHtml);
            foreach (Match match in charSetMatches)
            {
                if (theHtml.IndexOf(string.Format(@"id=""{0}""", match.Groups[2].Value)) > 0) // the matching id tag has been found, so update the href to remove the url before the #
                {
                    theHtml = theHtml.Replace(match.Groups[1].Value + "#" + match.Groups[2].Value, "#" + match.Groups[2].Value);
                }
            }

            // validate/fix html
            doc.LoadHtml(theHtml ?? ""); // by now loading the resultant html into the HtmlDocument object this should fix and issues i.e. close any unclosed tags etc
            result = doc.DocumentNode.OuterHtml;

            // now add any sub chapters
            result = result + BuildSubChapters(cHolder.CurrentChapter.EpubChapter, "", true);

            return result;
        }


        /// <summary>
        /// Use recursion to loop down any sub chapters
        /// </summary>
        /// <param name="chapter"></param>
        /// <param name="subchapterContent"></param>
        /// <param name="IsFirst"></param>
        /// <returns></returns>
        private string BuildSubChapters(EpubChapterRef chapter, string subchapterContent, bool IsFirst = false)
        {
            if (!IsFirst) // don't need to do the first on as it is the main chapter that has already been processed
            {
                subchapterContent += chapter.ReadHtmlContent();
            }

            foreach (var subChapter in chapter.SubChapters)
            {
                subchapterContent += BuildSubChapters(subChapter, subchapterContent);
            }

            return subchapterContent;
        }


        /// <summary>
        /// Hash used for Caching key
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}