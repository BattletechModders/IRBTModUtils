using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.Logging
{
    public class AsyncExceptionTest : Exception
    {
        public AsyncExceptionTest() : base("Test of the asynchronous exception log.")
        {
        }
    }
}
