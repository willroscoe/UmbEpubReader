using System.Collections.Generic;
using System.Text;
using VersOne.Epub;

namespace Wr.UmbEpubReader.Models
{
    /// <summary>
    /// Model of ePub details for use when displaying the ePub
    /// </summary>
    public class EpubDisplayModel
    {
        public bool IsValid { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        /// <summary>
        /// From the Umbraco document type field 'bookCoverImage'
        /// </summary>
        public string CoverImageUrl { get; set; }

        //public string CoverImage { get; set; }

        public List<EpubLink> TOC_Items { get; set; } // ordered list of chapters - to build a TOC from on the front-end

        public string ChapterHtml { get; set; } // Html chapter content

        public EpubLink Nav_PreviousChapterLink { get; set; }

        public EpubLink Nav_NextChapterLink { get; set; }

        public EpubByteContentFile ExternalFileHolder { get; set; }

        public string RedirectToChapter { get; set; }

        public string RenderTOC_AsUL(string CssClass = "")
        {
            if (TOC_Items.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<ul class=\"{0}\">", CssClass);
            foreach (var item in TOC_Items)
            {
                sb.AppendFormat("<li><a href=\"{0}\">{1}</a></li>", item.LinkUrl, item.LinkTitle);
            }
            sb.Append("</ul>");

            return sb.ToString();
        }

        public string RenderNav_PreviousChapterLinkAsHtml(string CssClass = "")
        {
            if (string.IsNullOrEmpty(Nav_PreviousChapterLink?.LinkUrl))
                return string.Empty;

            return string.Format("<a href=\"{0}\" class=\"{1}\">{2}</a>", Nav_PreviousChapterLink.LinkUrl, CssClass, Nav_PreviousChapterLink.LinkTitle);
        }

        public string RenderNav_NextChapterLinkAsHtml(string CssClass = "")
        {
            if (string.IsNullOrEmpty(Nav_NextChapterLink?.LinkUrl))
                return string.Empty;

            return string.Format("<a href=\"{0}\" class=\"{1}\">{2}</a>", Nav_NextChapterLink.LinkUrl, CssClass, Nav_NextChapterLink.LinkTitle);
        }

        public EpubDisplayModel()
        {
            TOC_Items = new List<EpubLink>();

            Nav_PreviousChapterLink = new EpubLink();

            Nav_NextChapterLink = new EpubLink();

        }
    }
}