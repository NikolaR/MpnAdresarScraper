using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinistarstvoProsveteAdresarScraper
{
    public class UnknownHtmlException : ScraperException
    {
        public UnknownHtmlException(string code)
            : base("Skinuti dokument ima neocekivan sadrzaj. Moguće da je adresar promenjen. (" + code + ")")
        {

        }
    }
}
