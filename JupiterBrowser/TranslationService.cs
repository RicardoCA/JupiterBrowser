using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JupiterBrowser
{
    public class TranslationService
    {
        private Dictionary<string, string> _currentLanguageDict;
        private string _currentLanguage;

        public void LoadLanguage(string language)
        {
            if (_currentLanguage == language) return;

            string json = File.ReadAllText($"{language}.json");
            _currentLanguageDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            _currentLanguage = language;
        }

        public string GetString(string key)
        {
            return _currentLanguageDict.TryGetValue(key, out string value) ? value : $"#{key}#";
        }
    }
}
