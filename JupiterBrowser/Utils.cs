using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JupiterBrowser
{

    internal class Utils
    {
        private const string Pinneds = "pinneds.json";

        public Utils()
        {
            EnsureSidebarFileExists();
        }

        private void EnsureSidebarFileExists()
        {
            if (!File.Exists(Pinneds))
            {
                File.WriteAllText(Pinneds, "{ }"); // Cria um arquivo JSON vazio
            }
        }

    }
}
