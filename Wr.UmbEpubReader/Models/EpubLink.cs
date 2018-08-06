using Wr.UmbEpubReader.Helpers;

namespace Wr.UmbEpubReader.Models
{
    public class EpubLink
    {
        public string LinkUrl { get; set; }

        public bool IsCurrent { get; set; }

        private string _linktitle;
        public string LinkTitle
        {
            get { return _linktitle; }
            set
            {
                _linktitle = value;
                LinkUrl = EpubHelpers.EncodeChapterTitleForUrl(value); // automatically set the LinkUrl when the LinkTitle is set
            }
        }
    }
}