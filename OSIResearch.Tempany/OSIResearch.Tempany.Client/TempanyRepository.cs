using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIResearch.Tempany.Client
{
    class TempanyRepository
    {
        public TempanyRepository(ITempanyProxy proxy)
        {
            _proxy = proxy;
        }

        ITempanyProxy _proxy;
        
    }
}
