using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Wr.UmbEpubReader.Models
{
    /// <summary>
    /// See https://our.umbraco.com/documentation/Reference/Routing/custom-controllers
    /// </summary>
    public class BookContentModel : ContentModel
    {
        // Standard Model Pass Through
        public BookContentModel(IPublishedContent content) : base(content) { }

        public EpubDisplayModel epub { get; set; }
        
    }
}