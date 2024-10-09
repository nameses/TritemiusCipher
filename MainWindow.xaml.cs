using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TritemiusCipher;
using System.Text.RegularExpressions;

namespace TritemiusCipher
{
    public sealed partial class MainWindow : Window
    {
        private string OriginalTextValue { get; set; } = string.Empty;
        private string ProcessedTextValue { get; set; } = string.Empty;
        private string A { get; set; } = string.Empty;
        private string B { get; set; } = string.Empty;
        private string C { get; set; } = string.Empty;
        private string Keyword { get; set; } = string.Empty;

        public MainWindow()
        {
            this.InitializeComponent();
            //bind elements to properties
            OriginalTextBox.DataContext = OriginalTextValue;
            OriginalTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = OriginalTextValue });

            ProcessedTextBox.DataContext = ProcessedTextValue;
            ProcessedTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = ProcessedTextValue });

            A_TextBox.DataContext = A;
            A_TextBox.SetBinding(TextBox.TextProperty, new Binding { Source = A });

            B_TextBox.DataContext = B;
            B_TextBox.SetBinding(TextBox.TextProperty, new Binding { Source = B });

            C_TextBox.DataContext = C;
            C_TextBox.SetBinding(TextBox.TextProperty, new Binding { Source = C });

            Keyword_TextBox.DataContext = Keyword;
            Keyword_TextBox.SetBinding(TextBox.TextProperty, new Binding { Source = Keyword });
        }

        private void ClearOriginalText_Click(object sender, RoutedEventArgs e)
        {
            OriginalTextBox.ClearValue(TextBox.TextProperty);
        }

        private void ClearProcessedText_Click(object sender, RoutedEventArgs e)
        {
            ProcessedTextBox.ClearValue(TextBox.TextProperty);
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var stringFileContent = await readFileAsync();

            if (string.IsNullOrEmpty(stringFileContent))
            {
                return;
            }

            OriginalTextBox.SetValue(TextBox.TextProperty, stringFileContent);
        }

        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            ProcessText(text => new TrithemiusCipher(GetCipherModel()).Encrypt(text));
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            ProcessText(text => new TrithemiusCipher(GetCipherModel()).Decrypt(text));
        }

        private void ProcessText(Func<string, string> processFunction)
        {
            ProcessedTextBox.ClearValue(TextBox.TextProperty);
            var originalText = OriginalTextBox.GetValue(TextBox.TextProperty).ToString();

            var processedText = processFunction(originalText);

            ProcessedTextBox.SetValue(TextBox.TextProperty, string.IsNullOrEmpty(processedText) ? "Не вірно задані параметри шифрування" : processedText);
        }

        private TrithemiusCipher.CipherModel GetCipherModel()
        {
            return new TrithemiusCipher.CipherModel
            {
                A = convToInt(A_TextBox.GetValue(TextBox.TextProperty).ToString()),
                B = convToInt(B_TextBox.GetValue(TextBox.TextProperty).ToString()),
                C = convToInt(C_TextBox.GetValue(TextBox.TextProperty).ToString()),
                Keyword = Keyword_TextBox.GetValue(TextBox.TextProperty).ToString()
            };
        }
        
        private int convToInt(string value) => string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value);

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await saveFileAsync(ProcessedTextBox.GetValue(TextBox.TextProperty).ToString());
        }

        private async Task saveFileAsync(string fileContent)
        {
            // Створення діалогу для вибору файлу
            var savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeChoices = { { "Текстовий файл", new List<string>() { ".txt" } } },
                SuggestedFileName = "Нове_ім'я"
            };

            var handledWindow = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(savePicker, handledWindow);

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Збереження вмісту файлу
                await FileIO.WriteTextAsync(file, fileContent);
            }
        }

        /// <summary>
        /// Read file from FilePicker and return its content in string
        /// </summary>
        private async Task<string> readFileAsync()
        {
            var openPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                FileTypeFilter = { ".txt" }
            };

            var handledWindow = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(openPicker, handledWindow);

            var file = await openPicker.PickSingleFileAsync();

            if (file == null)
            {
                return string.Empty;
            }

            return await FileIO.ReadTextAsync(file);
        }

        private void NumericTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            Regex regex = new Regex("[^0-9]+");

            if (regex.IsMatch(sender.Text))
            {
                sender.Text = regex.Replace(sender.Text, string.Empty);
                sender.SelectionStart = sender.Text.Length;
            }
        }
    }
}