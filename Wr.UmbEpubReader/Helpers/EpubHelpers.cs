using System.Text;
using System.Text.RegularExpressions;

namespace Wr.UmbEpubReader.Helpers
{
    public class EpubHelpers
    {
        /// <summary>
        /// Converts the chapter title to a url friendly version
        /// </summary>
        /// <param name="chapterTitle"></param>
        /// <returns></returns>
        public static string EncodeChapterTitleForUrl(string chapterTitle = "")
        {
            if (string.IsNullOrEmpty(chapterTitle))
                return string.Empty;

            string RemovedBullets = chapterTitle.ToLower().Replace("·", "o");

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] byteArray = Encoding.UTF8.GetBytes(RemovedBullets);
            byte[] asciiArray = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);
            string convertedString = ascii.GetString(asciiArray);

            var result = new Regex("[^a-zA-Z0-9 -]").Replace(convertedString.ToLower(), "");
            result = result.Trim();
            result = result.Replace(" ", "-");
            result = result.Replace("--", "-");

            return result;
        }
    }
}