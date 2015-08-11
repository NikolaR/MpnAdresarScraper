using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinistarstvoProsveteAdresarScraper
{
    public class ScraperException : Exception
    {
        public ScraperException()
        {
        }

        public ScraperException(string message)
            : base(message)
        {

        }

        public ScraperException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
