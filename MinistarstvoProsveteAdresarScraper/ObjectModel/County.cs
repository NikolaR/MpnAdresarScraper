using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinistarstvoProsveteAdresarScraper.ObjectModel
{
    [DebuggerDisplay("{Name}; #{Id}")]
    public class County
    {
        public County(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
