using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KAutoHelper;

namespace Auto_VTP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Log
    {
        public string Status { get; set; }
    }

    public partial class MainWindow : Window
    {
        ObservableCollection<Log> logs = new ObservableCollection<Log>();
        ObservableCollection<Log> errors = new ObservableCollection<Log>();

        bool isRunning = false;
        bool fullDetail = false;
        int loop = 0;
        int count = 0;
        List<string> devices = null;
        string[] lines = null;

        Bitmap success = (Bitmap)Bitmap.FromFile("success.png");
        Bitmap detail = (Bitmap)Bitmap.FromFile("detail.png");


        public MainWindow()
        {
            devices = KAutoHelper.ADBHelper.GetDevices();

            InitializeComponent();

            lvLogStatus.ItemsSource = logs;
            lvLogError.ItemsSource = errors;

            lines = File.ReadAllLines("data.txt");
            TotalTurns.Badge = int.Parse(lines[0]);

            if (devices.Count == 0)
            {
                MessageBox.Show("Not found any instance!");
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                Status.Content = devices.Count + " thiết bị đã kết nối!";
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning == false)
            {
                if (Turn.Text == "0")
                {
                    Error.Visibility = Visibility.Visible;
                    Error.Content = "Số lần muốn chạy không hợp lệ";
                    return;
                }

                isRunning = true;

                loop = int.Parse(Loop.Text);

                Error.Visibility = Visibility.Hidden;
                Error.Content = "";

                Status.Content = "Đang chạy...";
                Reset.IsEnabled = true;

                Start.Content = "Dừng lại";
                if (logs.Count == 0)
                {
                    logs.Add(new Log() { Status = "Đã nhận 0/" + loop * 3 + " lượt lắc" });
                }

                Auto(devices[0]);
            }
            else
            {
                Start.Content = "Bắt đầu";
                Status.Content = "Sẵn sàng...";
                Reset.IsEnabled = true;

                isRunning = false;

                logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Ngừng chạy" });
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning == false)
            {
                count = 0;
                logs.Clear();
                errors.Clear();

                Start.Content = "Bắt đầu";
                Error.Visibility = Visibility.Hidden;
                Error.Content = "";
                Status.Content = "Sẵn sàng...";
            }
        }

        async void Auto(string deviceID)
        {
            await Task.Run(() =>
            {
                if (count >= loop)
                {
                    isRunning = false;
                    this.Dispatcher.Invoke(() =>
                    {
                        Start.Content = "Bắt đầu";
                        Error.Visibility = Visibility.Visible;
                        Error.Content = "Đã hoàn tất";
                        Status.Content = "Sẵn sàng...";
                    });
                    return;
                }

                if (isRunning)
                {
                    this.Dispatcher.Invoke(() =>
                       {
                           if (fullDetail)
                           {
                               logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#click mở camera" });
                           }
                       });
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 15.7, 16.3); // mo camera
                }

                if (isRunning)
                {
                    int step = 0;
                    while (step < 100 && isRunning == true)
                    {
                        var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);

                        if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, detail) != null)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                if (fullDetail)
                                {
                                    logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#click textbox nhập tiền" });
                                }
                            });
                            KAutoHelper.ADBHelper.TapByPercent(deviceID, 8.3, 33.9);// click textbox nhap so tien
                            Delay(1000);
                            break;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                if (fullDetail)
                                {
                                    logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#kiểm tra lần " + step + " thất bại" });
                                }
                            });
                        }
                        step++;
                    }
                }

                if (isRunning)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#nhập tiền" });
                        }
                    });
                    KAutoHelper.ADBHelper.InputText(deviceID, "1"); // nhập số tiền
                    Delay(1000);
                }

            Fail:
                if (isRunning)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#click thanh toán" });
                        }
                    });
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 49.6, 55.6); // click "thanh toán"
                    Delay(1000);
                }

                if (isRunning)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#click xác nhận" });
                        }
                    });
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.5, 94.0); // click xác nhận
                    Delay(1000);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#nhập password" });
                        }
                    });
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 72.1); // nhap pass "2"
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 72.1); // nhap pass "2"
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 72.1); // nhap pass "2"
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.7, 79.7); // nhap pass "5"
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.7, 79.7); // nhap pass "5"
                }

                if (isRunning)
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.7, 79.7); // nhap pass "5"
                }

                if (isRunning)
                {
                    int step = 0;
                    while (step < 100 && isRunning == true)
                    {
                        var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);

                        if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, success) != null)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                if (fullDetail)
                                {
                                    logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#click trở về trang chủ" });
                                }
                            });
                            KAutoHelper.ADBHelper.TapByPercent(deviceID, 49.3, 78.7); // click "ve man hinh trang chu"
                            break;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                if (fullDetail)
                                {
                                    logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#kiểm tra lần " + step + " thất bại" });
                                }
                            });
                        }
                        step++;
                    }

                    if (step == 100)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            if (fullDetail)
                            {
                                logs.Insert(1, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#kiểm tra lần " + step + " thất bại" });
                            }
                        });

                        KAutoHelper.ADBHelper.TapByPercent(deviceID, 77.5, 62.5); //click "ok"
                        Delay(500);

                        goto Fail;
                    }
                }

                if (isRunning)
                {
                    count++;
                    this.Dispatcher.Invoke(() =>
                    {
                        CurrentTurns.Badge = int.Parse(CurrentTurns.Badge.ToString()) + 3;
                        TotalTurns.Badge = int.Parse(TotalTurns.Badge.ToString()) + 3;
                        lines[1] = TotalTurns.Badge.ToString();
                        File.WriteAllLines("data.txt", lines);
                    });
                    logs[0].Status = "Đã nhận" + count * 3 + "/" + loop * 3 + " lượt lắc";
                    Auto(deviceID);
                }
            });
        }

        void Delay(int delay)
        {
            while (delay > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                delay--;
                if (!isRunning)
                {
                    break;
                }
            }
        }

        private void Loop_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n = 0;
            bool check = int.TryParse(Loop.Text, out n);
            if (check)
            {
                Turn.Text = (n * 3).ToString();
            }
            else
            {
                Turn.Text = "0";
            }
        }

        private void ToggleButton_Click1(object sender, RoutedEventArgs e)
        {
            fullDetail = !fullDetail;
        }

        private void ToggleButton_Click2(object sender, RoutedEventArgs e)
        {
            if (HistoryDetail.IsChecked == true)
            {
                HistoryArea.Visibility = Visibility.Visible;
            }
            else
            {
                HistoryArea.Visibility = Visibility.Hidden;
            }
        }

        private void ToggleButton_Click3(object sender, RoutedEventArgs e)
        {
            if (NoteDetail.IsChecked == true)
            {
                NoteArea.Visibility = Visibility.Visible;
            }
            else
            {
                NoteArea.Visibility = Visibility.Hidden;
            }
        }

        private void ToggleButton_Click4(object sender, RoutedEventArgs e)
        {
            if (SignatureDetail.IsChecked == true)
            {
                SignatureArea.Visibility = Visibility.Visible;
            }
            else
            {
                SignatureArea.Visibility = Visibility.Hidden;
            }
        }
    }
}
