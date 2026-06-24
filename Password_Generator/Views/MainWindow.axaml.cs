using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Password_Generator.Views
{
    
    public partial class MainWindow : Window
    {
        static string password_result = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnGenerateClick(object? sender, RoutedEventArgs e)
        {
            password_result = "";
            int passwordLength = (int)PasswordLengthNumericUpDown.Value;

            // длина
            if (passwordLength <= 0)
            {
                PasswordOutput.Text = "Длина должна быть больше 0";
                return;
            }

            // алфавит 
            bool useLatin = LatinRadioButton.IsChecked == true;
            bool useCyrillic = CyrillicRadioButton.IsChecked == true;
            // Если ни один не выбран, по умолчанию используем латиницу
            if (!useLatin && !useCyrillic)
                useLatin = true;

            // Списки символов 
            List<char> upperChars = new List<char>();
            List<char> lowerChars = new List<char>();
            List<char> digitChars = new List<char>();
            List<char> specialChars = new List<char> { '.', '@', '!', '#', '$', '&', '%', '*', '/' };

            // Заполняем списки
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

            // количество критериев
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
            if (needDigit) allChars.AddRange(Enumerable.Range('0', 10).Select(c => (char)c)); // '0' = 48
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
            if (needDigit) mandatory.Add((char)rand.Next('0', ':')); // '0'..'9'
            if (needSpecial) mandatory.Add(specialChars[rand.Next(specialChars.Count)]);

            // Заполняемпозиции случайными символами
            int remaining = passwordLength - mandatory.Count;
            List<char> result = new List<char>(mandatory);
            for (int i = 0; i < remaining; i++)
            {
                result.Add(allChars[rand.Next(allChars.Count)]);
            }

            // 3. Перемешиваем список 
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                char temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }

            // Выводим пароль
            foreach (var item in result)
            {
                password_result += item.ToString();
            }
             
            PasswordOutput.Text = password_result;
        }

        private async void onCopyClick(object? sender, RoutedEventArgs e)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                string textToCopy = password_result;
                await clipboard.SetTextAsync(textToCopy);
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