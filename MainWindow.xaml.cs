using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Change_GreenHub_time_limit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int time = 0;

            try
            {
                time = Int32.Parse(materialTextBox21.Text);
            }
            catch
            {
                if (materialSwitch1.IsChecked.HasValue && materialSwitch1.IsChecked.Value)
                {
                    snackBar.MessageQueue?.Enqueue(
                        "Please input number!",
                        null,
                        null,
                        null,
                        false,
                        true,
                        TimeSpan.FromSeconds(2.0));
                }
                return;
            }

            string jsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\GreenHub\\config.json";
            string newMinutesValue = time.ToString();
            string jsContent;
            // 读取文件内容  
            try
            {
                jsContent = File.ReadAllText(jsFilePath);
            }
            catch
            {
                if (materialSwitch1.IsChecked.HasValue && materialSwitch1.IsChecked.Value)
                {
                    snackBar.MessageQueue?.Enqueue(
                        "Failed to open file - ",
                        null,
                        null,
                        null,
                        false,
                        true,
                        TimeSpan.FromSeconds(2.0));
                }

                return;
            }

            // 使用正则表达式找到要替换的minutes值  
            // 假设minutes的值是一个数字，并且你想要替换整个数字（包括前面的逗号和空格）  
            string pattern = @"""minutes"":\s*\d+"; // 匹配 "minutes": 后面的空格和任意数字  
            string replacement = $"\"minutes\": {newMinutesValue}"; // 创建替换字符串，注意格式要保持一致  

            // 执行替换  
            string newJsContent = Regex.Replace(jsContent, pattern, replacement);

            // 写回文件  
            File.WriteAllText(jsFilePath, newJsContent);

            if (materialSwitch1.IsChecked.HasValue && materialSwitch1.IsChecked.Value)
            {
                snackBar.MessageQueue?.Enqueue(
                    "Successfully!",
                    null,
                    null,
                    null,
                    false,
                    true,
                    TimeSpan.FromSeconds(2.0));
            }
        }
    }
}