using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIResearch.Tempany.Generated;
using Trinity;
using Trinity.Core.Lib;
using Trinity.TSL.Lib;
using OSIResearch.Tempany.Core;
using System.Collections.Concurrent;
using System.Threading;

namespace OSIResearch.Tempany.GraphApplication
{
    partial class TempanyServer : TempanyServerBase
    {

#if MEMCLOUD
        private Trinity.Storage.MemoryCloud Storage { get { return Global.CloudStorage; } }
#else
        private Trinity.Storage.LocalMemoryStorage Storage { get { return Global.LocalStorage; } }
#endif

    }
}
