using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace JupiterBrowser
{
    internal class BackgroundPersist
    {
        private const string SidebarFileName = "sidebar.json";

        public BackgroundPersist()
        {
            EnsureSidebarFileExists();
        }

        private void EnsureSidebarFileExists()
        {
            if (!File.Exists(SidebarFileName))
            {
                File.WriteAllText(SidebarFileName, "{\"color\": \"#FF2D2D30\"}"); // Cria um arquivo JSON vazio
            }
        }

        public void SaveColor(string color)
        {
            string jsonContent = File.ReadAllText(SidebarFileName);

            // Converte o conteúdo em um JObject
            JObject jsonObject = JObject.Parse(jsonContent);

            // Define ou atualiza a propriedade 'color'
            jsonObject["color"] = color;

            // Salva o JSON atualizado de volta ao arquivo
            File.WriteAllText(SidebarFileName, jsonObject.ToString());
        }

        public string GetColor()
        {
            string jsonContent = File.ReadAllText(SidebarFileName);

            // Converte o conteúdo em um JObject
            JObject jsonObject = JObject.Parse(jsonContent);

            // Retorna o valor da propriedade 'color', ou null se não existir
            return jsonObject["color"]?.ToString();
        }


    }
}
