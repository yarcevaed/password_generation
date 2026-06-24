using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Password_Generator.Views
{
    public partial class MainWindow : Window
    {
        static string password_result = "";
        private List<string> savedPasswords = new List<string>();
        private readonly string saveFilePath = "saved_passwords.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadSavedPasswords();
            SavedPasswordsList.ItemsSource = savedPasswords;
        }

        // Загрузка сохранённых паролей из JSON
        private void LoadSavedPasswords()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    savedPasswords = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
                else
                {
                    savedPasswords = new List<string>();
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки просто создаём пустой список
                savedPasswords = new List<string>();
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки паролей: {ex.Message}");
            }
        }

        // Сохранение списка в JSON
        private void SaveSavedPasswords()
        {
            try
            {
                string json = JsonSerializer.Serialize(savedPasswords, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения паролей: {ex.Message}");
            }
        }

        public void OnGenerateClick(object? sender, RoutedEventArgs e)
        {
            password_result = "";
            int passwordLength = (int)PasswordLengthNumericUpDown.Value;

            if (passwordLength <= 0)
            {
                PasswordOutput.Text = "Длина должна быть больше 0";
                return;
            }

            bool useLatin = LatinRadioButton.IsChecked == true;
            bool useCyrillic = CyrillicRadioButton.IsChecked == true;
            if (!useLatin && !useCyrillic)
                useLatin = true;

            List<char> upperChars = new List<char>();
            List<char> lowerChars = new List<char>();
            List<char> digitChars = new List<char>();
            List<char> specialChars = new List<char> { '.', '@', '!', '#', '$', '&', '%', '*', '/' };

            if (useLatin)
            {
                upperChars.AddRange(Enumerable.Range('A', 26).Select(c => (char)c));
                lowerChars.AddRange(Enumerable.Range('a', 26).Select(c => (char)c));
            }
            else if (useCyrillic)
            {
                upperChars.AddRange(Enumerable.Range(1040, 32).Select(c => (char)c));
                lowerChars.AddRange(Enumerable.Range(1072, 32).Select(c => (char)c));
            }

            bool needUpper = UpperCaseCheckBox.IsChecked == true;
            bool needLower = LowerCaseCheckBox.IsChecked == true;
            bool needDigit = DigitCheckBox.IsChecked == true;
            bool needSpecial = SpecialCheckBox.IsChecked == true;

            int criteriaCount = 0;
            if (needUpper) criteriaCount++;
            if (needLower) criteriaCount++;
            if (needDigit) criteriaCount++;
            if (needSpecial) criteriaCount++;

            if (criteriaCount == 0)
            {
                PasswordOutput.Text = "Выберите хотя бы один критерий";
                return;
            }

            if (passwordLength < criteriaCount)
            {
                PasswordOutput.Text = $"Длина пароля ({passwordLength}) меньше числа выбранных критериев ({criteriaCount})";
                return;
            }

            List<char> allChars = new List<char>();
            if (needUpper) allChars.AddRange(upperChars);
            if (needLower) allChars.AddRange(lowerChars);
            if (needDigit) allChars.AddRange(Enumerable.Range('0', 10).Select(c => (char)c));
            if (needSpecial) allChars.AddRange(specialChars);

            if (allChars.Count == 0)
            {
                PasswordOutput.Text = "Нет доступных символов для генерации";
                return;
            }

            Random rand = new Random();

            List<char> mandatory = new List<char>();
            if (needUpper) mandatory.Add(upperChars[rand.Next(upperChars.Count)]);
            if (needLower) mandatory.Add(lowerChars[rand.Next(lowerChars.Count)]);
            if (needDigit) mandatory.Add((char)rand.Next('0', ':'));
            if (needSpecial) mandatory.Add(specialChars[rand.Next(specialChars.Count)]);

            int remaining = passwordLength - mandatory.Count;
            List<char> result = new List<char>(mandatory);
            for (int i = 0; i < remaining; i++)
            {
                result.Add(allChars[rand.Next(allChars.Count)]);
            }

            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                char temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }

            password_result = new string(result.ToArray());
            PasswordOutput.Text = password_result;

            // ---- СОХРАНЕНИЕ В СПИСОК И JSON ----
            if (!string.IsNullOrEmpty(password_result))
            {
                savedPasswords.Add(password_result);
                SaveSavedPasswords();
                // Обновляем отображение списка (ItemsSource обновляется автоматически, так как мы присвоили новый список)
                // Но если мы используем тот же объект List, нужно перезагрузить ItemsSource.
                // Проще заново присвоить ItemsSource:
                SavedPasswordsList.ItemsSource = null;
                SavedPasswordsList.ItemsSource = savedPasswords;
            }
        }

        private async void onCopyClick(object? sender, RoutedEventArgs e)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                await clipboard.SetTextAsync(password_result);
            }
        }

        private void OnEncryptClick(object? sender, RoutedEventArgs e)
        {
            EncryptionWindow encryption_window = new EncryptionWindow();
            encryption_window.Show();
            this.Close();
        }

        private void onDecodingClick(object? sender, RoutedEventArgs e)
        {
            DecodingWindow decoding_window = new DecodingWindow();
            decoding_window.Show();
            this.Close();
        }
    }
}