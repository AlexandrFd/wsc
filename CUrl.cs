using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSiteChecker
{
    class CUrl
    {
        public string Url;
        public double Timing;

        public CUrl(string url, double timing)
        {
            Url = url;
            Timing = timing;
        }
    }
}
