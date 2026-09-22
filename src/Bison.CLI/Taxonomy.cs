using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison.CLI
{
    public static class Taxonomy
    {
        public static Dictionary<string, Taxon> Lookup { get; private set; } = new();

        public static void Initialize(string path)
        {
            Lookup = TaxonLoader.Load(path);
        }

        public static bool Exists(string taxonId) =>
            Lookup.ContainsKey(taxonId);


        // Reset method to clear the taxonomy lookup, useful for testing purposes.
        public static void Reset()
        {
            Lookup.Clear();
        }
    }
}


    
