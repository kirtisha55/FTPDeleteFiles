using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftpdeletefiles
{
    class Program
    {
        static void Main(string[] args)
        {
            long ts = Ftpconn.TestMeth();

            bool check = true;
            if (ts > 32000000000)
            {
                check = Ftpconn.DeleteFtpFiles(ts);
            }
        }
    }
}
