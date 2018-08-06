using System;

namespace Wr.UmbEpubReader.Models
{
    public class FileToServe
    {
        public string Filename { get; set; }

        public byte[] Data { get; set; }

        public string MimeType { get; set; }

        public string LastModified { get; set; }

        private DateTime? _setlastmodified;
        public DateTime? SetLastModified
        {
            get { return _setlastmodified;  }
            set
            {
                _setlastmodified = value;
                LastModified = value.Value.ToUniversalTime().ToString("R"); // convert date to string in the correct format for http response i.e. Wed, 21 Oct 2015 07:28:00 GMT
            }
        }

        public FileToServe(byte[] data = null, string mimeType = null, string filename = null, DateTime? lastmodified = null)
        {
            Data = data;
            MimeType = mimeType;
            Filename = filename;

            if (lastmodified.HasValue)
                SetLastModified = lastmodified;
        }

        public bool IsValid()
        {
            return (Data != null && Data.Length > 0) ? true : false;
        }
    }
}