using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace JupiterBrowser
{
    internal class Vault
    {
        private string fileName = "vault.json";
        private readonly byte[] key;
        private readonly byte[] iv;

        public Vault()
        {
            // Load or generate key and IV
            if (File.Exists(fileName))
            {
                string jsonData = File.ReadAllText(fileName);
                dynamic vaultData = JsonConvert.DeserializeObject(jsonData);
                key = Convert.FromBase64String((string)vaultData.Key);
                iv = Convert.FromBase64String((string)vaultData.IV);
            }
            else
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.GenerateKey();
                    aesAlg.GenerateIV();
                    key = aesAlg.Key;
                    iv = aesAlg.IV;
                }
            }
        }

        public bool HasVaultExist()
        {
            return File.Exists(fileName);
        }

        public void CreateVault(string password)
        {
            string encryptedPassword = Encrypt(password);
            var vaultData = new
            {
                Password = encryptedPassword,
                Key = Convert.ToBase64String(key),
                IV = Convert.ToBase64String(iv)
            };
            string jsonData = JsonConvert.SerializeObject(vaultData, Formatting.Indented);
            File.WriteAllText(fileName, jsonData);
        }

        public string GetStoredPassword()
        {
            if (HasVaultExist())
            {
                string jsonData = File.ReadAllText(fileName);
                dynamic vaultData = JsonConvert.DeserializeObject(jsonData);
                string encryptedPassword = vaultData.Password;
                return Decrypt(encryptedPassword);
            }
            return null;
        }

        public string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
