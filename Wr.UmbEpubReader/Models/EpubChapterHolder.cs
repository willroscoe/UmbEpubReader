using VersOne.Epub;

namespace Wr.UmbEpubReader.Models
{
    public class EpubChapterHolder
    {
        public EpubChaperItem PreviousChapter { get; set; }

        public EpubChaperItem CurrentChapter { get; set; }

        public EpubChaperItem NextChapter { get; set; }

        /// <summary>
        /// Holds the css content to be used
        /// </summary>
        public string CssContent { get; set; }

        public EpubChapterHolder()
        {

            PreviousChapter = new EpubChaperItem();

            CurrentChapter = new EpubChaperItem();

            NextChapter = new EpubChaperItem();
        }
    }

    /// <summary>
    /// The properties needed for a chapter
    /// </summary>
    public class EpubChaperItem
    {
        public int ChapterIndex { get; set; }

        public EpubLink TitleAndLinkUrl { get; set; }

        public EpubChapterRef EpubChapter { get; set; }

        public string FinalHtmlContent { get; set; }

        public EpubChaperItem()
        {
            ChapterIndex = -1;

            TitleAndLinkUrl = new EpubLink();
        }

        /// <summary>
        /// Return IsValid if the chapter has been found
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return (ChapterIndex >= 0) ? true : false;
        }

    }
}