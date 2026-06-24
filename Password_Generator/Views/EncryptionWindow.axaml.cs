using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Password_Generator.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Password_Generator
{
    public partial class EncryptionWindow : Window
    {
        // Путь к JSON-файлу
        private readonly string _jsonFilePath = "encrypted_passwords.json";
        // Список сохранённых записей
        private List<PasswordEntry> _passwordEntries = new();

        public EncryptionWindow()
        {
            InitializeComponent();
            LoadPasswordsFromJson();
            UpdateSavedPasswordsList();
        }

        // ---------- Загрузка / сохранение JSON ----------
        private void LoadPasswordsFromJson()
        {
            try
            {
                if (File.Exists(_jsonFilePath))
                {
                    string json = File.ReadAllText(_jsonFilePath, Encoding.UTF8);
                    _passwordEntries = JsonSerializer.Deserialize<List<PasswordEntry>>(json)
                                       ?? new List<PasswordEntry>();
                }
                else
                {
                    _passwordEntries = new List<PasswordEntry>();
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки файл считается пустым
                _passwordEntries = new List<PasswordEntry>();
                Console.WriteLine($"Ошибка загрузки JSON: {ex.Message}");
            }
        }

        private void SavePasswordsToJson()
        {
            try
            {
                string json = JsonSerializer.Serialize(_passwordEntries, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_jsonFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения JSON: {ex.Message}");
            }
        }

        // ---------- Обновление списка в левой панели ----------
        private void UpdateSavedPasswordsList()
        {
            // Отображаем только зашифрованный текст (обрезанный для краткости)
            var displayItems = _passwordEntries
                .Select((entry, index) =>
                    $"{index + 1}. {Truncate(entry.EncryptedText, 30)}")
                .ToList();
            SavedPasswordsList.ItemsSource = displayItems;
        }

        private string Truncate(string text, int maxLength)
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        // ---------- Шифрование (AES) ----------
        private string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                // Генерируем соль и IV
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                // Получаем ключ из пароля
                using (var deriveBytes = new Rfc2898DeriveBytes(key ?? "DefaultKey", salt, 10000))
                {
                    aes.Key = deriveBytes.GetBytes(32); // 256 бит
                    aes.IV = deriveBytes.GetBytes(16);  // 128 бит
                }

                using (var ms = new MemoryStream())
                {
                    // Записываем соль в начало
                    ms.Write(salt, 0, salt.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // ---------- Расшифровка (для справки) ----------
        private string Decrypt(string cipherText, string key)
        {
            try
            {
                byte[] fullCipher = Convert.FromBase64String(cipherText);
                byte[] salt = new byte[16];
                byte[] cipherData = new byte[fullCipher.Length - 16];
                Array.Copy(fullCipher, 0, salt, 0, 16);
                Array.Copy(fullCipher, 16, cipherData, 0, cipherData.Length);

                using (Aes aes = Aes.Create())
                {
                    using (var deriveBytes = new Rfc2898DeriveBytes(key ?? "DefaultKey", salt, 10000))
                    {
                        aes.Key = deriveBytes.GetBytes(32);
                        aes.IV = deriveBytes.GetBytes(16);
                    }
                    using (var ms = new MemoryStream(cipherData))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "[Ошибка расшифровки]";
            }
        }

        // ---------- Обработчики событий ----------

        // Кнопка «Зашифровать»
        public async void onGenerate_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string plainText = PasswordTextBox.Text ?? "";
            string key = KeyTextBox.Text ?? "";

            if (string.IsNullOrWhiteSpace(plainText))
            {
                OutputTextBox.Text = "Введите текст для шифрования.";
                return;
            }

            string encrypted = Encrypt(plainText, key);
            OutputTextBox.Text = encrypted;

            // Добавляем запись в список и сохраняем JSON
            _passwordEntries.Add(new PasswordEntry
            {
                EncryptedText = encrypted,
                Key = key, // можно сохранять ключ (но это небезопасно, для демонстрации)
                Date = DateTime.Now
            });
            SavePasswordsToJson();
            UpdateSavedPasswordsList();
        }

        // Кнопка «Генерация» (заглушка)
        public void onPasswordClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainWindow main_window = new MainWindow();
            main_window.Show();
            this.Close();
        }

        // Кнопка «Расшифровка» (заглушка)
        public void onDecodingClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DecodingWindow decoding_window =new DecodingWindow();
            decoding_window.Show();
            this.Close();
        }

        // Кнопка «Копировать»
        public async void onCopyClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string text = OutputTextBox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            // Получаем буфер обмена Avalonia
            if (TopLevel.GetTopLevel(this) is TopLevel topLevel)
            {
                var clipboard = topLevel.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    OutputTextBox.Text = "Скопировано!";
                    // Возвращаем исходный текст через секунду
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await Task.Delay(1500);
                        OutputTextBox.Text = text;
                    });
                }
            }
        }

        // ---------- Вспомогательный класс для хранения данных ----------
        public class PasswordEntry
        {
            public string EncryptedText { get; set; } = "";
            public string Key { get; set; } = "";
            public DateTime Date { get; set; } = DateTime.Now;
        }
    }
}