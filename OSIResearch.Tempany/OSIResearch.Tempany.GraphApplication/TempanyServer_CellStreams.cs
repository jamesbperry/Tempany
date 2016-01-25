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
    partial class TempanyServer
    {
        public override void CreateCellStreamHandler(CreateCellStreamArgsReader request, CellIdentifierWriter response)
        {
            throw new NotImplementedException();
        }

        public override void GetCellStreamValueHandler(GetCellStreamValueArgsReader request, CellStreamValueCollectionDTOWriter response)
        {
            throw new NotImplementedException();
        }

        public override void InsertCellStreamValueHandler(InsertCellStreamValueArgsReader request)
        {
            throw new NotImplementedException();
        }
    }
}
