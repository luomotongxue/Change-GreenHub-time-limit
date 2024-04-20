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
using System.Net.Http;
using System.Security.Policy;
using System.Reflection;
using Path = System.IO.Path;

namespace Change_GreenHub_time_limit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        bool isDownloading = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public class DownloadProgress
        {
            public int BytesReceived { get; set; }
            public int TotalBytesToReceive { get; set; }
            public double ProgressPercentage => (BytesReceived / (double)TotalBytesToReceive) * 100;
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
                        "Please input a valid number!",
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

        public async Task DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgress> progress)
        {
            if (destinationPath == null)
            {
                return ;
            }
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    if (!response.Content.Headers.ContentLength.HasValue)
                    {
                        return ;
                    }
                    var totalBytes = response.Content.Headers.ContentLength.Value;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        try
                        {
                            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                            {
                                var buffer = new byte[81920];
                                var totalBytesRead = 0L;
                                var bytesRead = 0;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                                    totalBytesRead += bytesRead;
                                    var progressReport = new DownloadProgress { BytesReceived = (int)totalBytesRead, TotalBytesToReceive = (int)totalBytes };
                                    progress.Report(progressReport);
                                }
                            }
                        }
                        catch 
                        {
                            snackBar.MessageQueue?.Enqueue(
                                $"Download failed! tried to download file - {destinationPath}",
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
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (isDownloading)
            {
                return ;
            }
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            string downloadPath = Path.Combine(Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location), "GreenHub_installer.exe");
            if (File.Exists(downloadPath))
            {
                snackBar.MessageQueue?.Enqueue(
                    $"File already exist - {downloadPath}",
                    null,
                    null,
                    null,
                    false,
                    true,
                    TimeSpan.FromSeconds(2.0));
                return;
            }

            isDownloading = true;
            changeButton.IsEnabled = false;
            snackBarMessage.Content = "Starting Download..";
            var progress = new Progress<DownloadProgress>(report =>
            {
                if (materialSwitch1.IsChecked.HasValue && materialSwitch1.IsChecked.Value)
                {
                    snackBarMessage.Content = $"Downloading... {report.ProgressPercentage.ToString("F2")}%";
                    snackBar.IsActive = true;
                    if (report.ProgressPercentage == 100)
                    {
                        snackBar.IsActive = false;
                        isDownloading = false;
                        changeButton.IsEnabled = true;
                        snackBar.MessageQueue?.Enqueue(
                            $"Download completed! file - {downloadPath}",
                            null,
                            null,
                            null,
                            false,
                            true,
                            TimeSpan.FromSeconds(2.0));
                    }
                }
            });

            await DownloadFileAsync("https://kr042001.westmgreen.com/download/GreenHub%2520Setup%25202.2.0.exe", downloadPath, progress);
#pragma warning restore CS8604 // 引用类型参数可能为 null。     
        }

        public BitmapImage ByteArrayToImage(byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Icon = ByteArrayToImage(Properties.Resources.OIP_C);
        }
    }
}