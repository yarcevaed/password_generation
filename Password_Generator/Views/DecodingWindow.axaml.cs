using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Password_Generator.Views;
using System.Collections.Generic;

namespace Password_Generator;

public partial class DecodingWindow : Window
{
    string ready_password = "";
    public DecodingWindow()
    {
        InitializeComponent();
    }


    private int ReduceToDigit(string input)
    {
        if (int.TryParse(input, out int number))
        {
            while (number >= 10)
            {
                int sum = 0;
                while (number > 0)
                {
                    sum += number % 10;
                    number /= 10;
                }
                number = sum;
            }
            return number;
        }
        else
        {
            int sumCodes = 0;
            foreach (char ch in input)
                sumCodes += (int)ch;

            while (sumCodes >= 10)
            {
                int sum = 0;
                while (sumCodes > 0)
                {
                    sum += sumCodes % 10;
                    sumCodes /= 10;
                }
                sumCodes = sum;
            }
            return sumCodes;
        }
    }
    public void onDecoding_Click(object? sender, RoutedEventArgs e)
    {
        ready_password = "";
        if (PasswordTextBox != null && KeyTextBox.Text != "")//רטפנ ס ךכ‏קמל
        {
            string password = PasswordTextBox.Text.ToString();
            string key = KeyTextBox.Text.ToString();
            int indent = 0;
            List<char> new_password = new List<char>();
            indent = ReduceToDigit(key);

            for (int i = 0; i < password.Length; ++i)
            {
                new_password.Add(' ');
            }
            for (int i = 0; i < password.Length; ++i)
            {
                int ascii_code = (int)password[i];
                if (ascii_code - indent <= 255 && ascii_code - indent >= 32)
                {
                    new_password[i] = (char)(ascii_code - indent);
                }
                else if (ascii_code - indent > 255)
                {
                    new_password[i] = (char)(ascii_code - indent + (225 - 32));
                }

            }

            foreach (var item in new_password)
            {
                ready_password += item.ToString();
            }

            OutputTextBox.Text = ready_password;

        }
        else if (PasswordTextBox != null && KeyTextBox.Text == "")//רטפנ בוח ךכ‏קא
        {
            string password = PasswordTextBox.Text.ToString();
            List<char> new_password = new List<char>();
            for (int i = 0; i < password.Length; ++i)
            {
                new_password.Add(' ');
            }
            for (int i = 0; i < password.Length; ++i)
            {
                int ascii_code = (int)password[i];
                if (ascii_code - 8 <= 255 && ascii_code - 8 >= 32)
                {
                    new_password[i] = (char)(ascii_code - 8);
                }
                else if (ascii_code - 8 > 255)
                {
                    new_password[i] = (char)(ascii_code - 8 + (225 - 32));
                }

            }

            foreach (var item in new_password)
            {
                ready_password += item.ToString();
            }

            OutputTextBox.Text = ready_password;
        }
        else
        {
            OutputTextBox.Text = "Âגוהטעו ןאנמכü!";
        }
    }

    private void OnEncryptClick(object? sender, RoutedEventArgs e)
    {
        EncryptionWindow encryption_window = new EncryptionWindow();
        encryption_window.Show();
        this.Close();
    }
    private void onPasswordClick(object? sender, RoutedEventArgs e)
    {
        MainWindow main_window = new MainWindow();
        main_window.Show();
        this.Close();
    }
}