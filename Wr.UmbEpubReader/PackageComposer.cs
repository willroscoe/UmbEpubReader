using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Wr.UmbEpubReader.Components;

namespace Wr.UmbEpubReader
{
    public class PackageComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<RoutingComponent>();
        }
    }
}