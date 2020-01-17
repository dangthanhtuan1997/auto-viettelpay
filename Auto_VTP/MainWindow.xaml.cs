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
using System.Windows.Threading;
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

        bool[] isRunning = { false, false };
        bool fullDetail = false;
        bool block = false;
        int loop = 0;
        int count = 0;
        List<string> devices = null;
        string[] lines = null;
        DateTime startTime = DateTime.Now;

        Bitmap success = (Bitmap)Bitmap.FromFile("success.png");
        Bitmap pay = (Bitmap)Bitmap.FromFile("pay.png");
        Bitmap ok = (Bitmap)Bitmap.FromFile("ok.png");


        public MainWindow()
        {
            devices = KAutoHelper.ADBHelper.GetDevices();

            InitializeComponent();

            lvLogStatus.ItemsSource = logs;

            lines = File.ReadAllLines("data.txt");
            TotalTurns.Badge = int.Parse(lines[0]);

            DispatcherTimer dtClockTime = new DispatcherTimer();

            dtClockTime.Interval = new TimeSpan(0, 0, 1); //in Hour, Minutes, Second.
            dtClockTime.Tick += dtClockTime_Tick;

            dtClockTime.Start();

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

        private void dtClockTime_Tick(object sender, EventArgs e)
        {
            TimeSpan t = DateTime.Now - startTime;
            Timer.Content = t.ToString(@"hh\:mm\:ss");
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Turn.Text == "0")
            {
                Error.Content = "Số lần muốn chạy không hợp lệ!";
                return;
            }

            if (SDTQR.IsChecked == false)
            {
                Error.Content = "Chưa cập nhật SĐT QR!";
                return;
            }

            if (QRCODE.IsChecked == false)
            {
                Error.Content = "Chưa cập nhật hình ảnh QR code!";
                return;
            }

            if (TurnRemaining.IsChecked == false)
            {
                Error.Content = "Chưa kiểm tra lượt hiện có!";
                return;
            }

            if (Balance.IsChecked == false)
            {
                Error.Content = "Chưa kiểm tra số dư hiện có!";
                return;
            }

            loop = int.Parse(Loop.Text);

            Error.Content = "";

            Start.Visibility = Visibility.Collapsed;
            StartDevice1.Visibility = Visibility.Visible;
            StartDevice2.Visibility = Visibility.Visible;

            //Auto(devices[0]);

            //logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Ngừng chạy" });
        }

        private void StartDevice1_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning[0] == false)
            {
                isRunning[0] = true;
                StartDevice1.Content = "Dừng máy 1";

                Auto(devices[0], 0);

                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Bắt đầu chạy máy 1" });
            }
            else
            {
                isRunning[0] = false;
                StartDevice1.Content = "Chạy máy 1";

                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Ngừng chạy máy 1" });
            }
        }

        private void StartDevice2_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning[1] == false)
            {
                isRunning[1] = true;
                StartDevice2.Content = "Dừng máy 2";

                Auto(devices[1], 1);

                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Bắt đầu chạy máy 2" });
            }
            else
            {
                isRunning[1] = false;
                StartDevice2.Content = "Chạy máy 2";

                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "Ngừng chạy máy 2" });
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning[0] == true || isRunning[1] == true)
            {
                Error.Content = "Phải ngừng tất cả các máy trước khi reset!";

                return;
            }
            count = 0;
            CurrentTurns.Badge = 0;
            block = false;
            logs.Clear();
            SDTQR.IsChecked = false;
            QRCODE.IsChecked = false;
            TurnRemaining.IsChecked = false;
            Balance.IsChecked = false;

            Error.Content = "";
            Status.Content = devices.Count + " thiết bị đã kết nối!";
            Error.Content = "Reset thành công!";

            Start.Visibility = Visibility.Visible;
            StartDevice1.Visibility = Visibility.Collapsed;
            StartDevice2.Visibility = Visibility.Collapsed;

            StartDevice1.Content = "Chạy máy 1";
            StartDevice2.Content = "Chạy máy 2";
        }

        async void Auto(string deviceID, int index)
        {
            await Task.Run(() =>
            {
                if (count >= loop)
                {
                    isRunning[0] = false;
                    isRunning[1] = false;
                    this.Dispatcher.Invoke(() =>
                    {
                        Error.Content = "Đã hoàn tất";
                    });
                    return;
                }

            Loop:
                if (count >= loop)
                {
                    isRunning[0] = false;
                    isRunning[1] = false;
                    this.Dispatcher.Invoke(() =>
                    {
                        Error.Content = "Đã hoàn tất";
                    });
                    return;
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 8.0, 31.9);// click textbox nhap so tien
                }

                if (isRunning[index])
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#Máy " + (index + 1) + " nhập tiền" });
                        }
                    });
                    KAutoHelper.ADBHelper.InputText(deviceID, "10000"); // nhập số tiền
                }

            Fail:
                if (isRunning[index])
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#Máy " + (index + 1) + " click thanh toán" });
                        }
                    });
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 51.7, 50.8); // click "thanh toán"
                    Delay(1000, index);
                }

                if (isRunning[index])
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#Máy " + (index + 1) + " click xác nhận" });
                        }
                    });
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.3, 94.1); // click xác nhận
                    Delay(500, index);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (fullDetail)
                        {
                            logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#Máy " + (index + 1) + " nhập password" });
                        }
                    });
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.0, 79.0); // nhap pass "2"
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.0, 79.0); // nhap pass "2"
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 50.0, 79.0); // nhap pass "2"
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.6, 84.9); // nhap pass "5"
                }

                if (isRunning[index])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.6, 84.9); // nhap pass "5"
                }

                if (isRunning[index])
                {
                    while (block == true)
                    {
                        Delay(200, index);
                    }
                    block = true;
                    Delay(500, index);
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.6, 84.9); // nhap pass "5"
                }

                if (isRunning[index])
                {
                    int step = 0;
                    while (isRunning[index] == true)
                    {
                        var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                        var s = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, success);
                        var o = KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, ok);

                        if (s != null)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "- Thành công - máy " + (index + 1) });
                            });
                            KAutoHelper.ADBHelper.TapByPercent(deviceID, 5.1, 6.5); // click trở về
                            block = false;

                            break;
                        }

                        if (o != null)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                TotalFailTurns.Badge = int.Parse(TotalFailTurns.Badge.ToString()) + 1;
                                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "- Thất bại - máy " + (index + 1) });
                            });

                            KAutoHelper.ADBHelper.Tap(deviceID, o.Value.X, o.Value.Y);
                            //KAutoHelper.ADBHelper.TapByPercent(deviceID, 76.8, 60.3); //click "ok"
                            Delay(500, index);
                            block = false;

                            goto Fail;
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            if (fullDetail)
                            {
                                logs.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss ") + "#Máy " + (index + 1) + " kiểm tra lần " + step + " thất bại" });
                            }
                        });

                        step++;
                    }
                }

                if (isRunning[index])
                {
                    count++;
                    this.Dispatcher.Invoke(() =>
                    {
                        CurrentTurns.Badge = int.Parse(CurrentTurns.Badge.ToString()) + 3;
                        TotalTurns.Badge = int.Parse(TotalTurns.Badge.ToString()) + 3;
                        TotalSuccessTurns.Badge = int.Parse(TotalSuccessTurns.Badge.ToString()) + 1;
                        lines[0] = TotalTurns.Badge.ToString();
                        File.WriteAllLines("data.txt", lines);
                    });
                    goto Loop;
                }
            });
        }

        void Delay(int delay, int index)
        {
            while (delay > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                delay--;
                if (!isRunning[index])
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

        private void CurrentTurn_Click(object sender, RoutedEventArgs e)
        {
            CurrentTurns.Badge = int.Parse(CurrentTurns.Badge.ToString()) + 3;
            TotalSuccessTurns.Badge = int.Parse(TotalSuccessTurns.Badge.ToString()) + 1;
            count += 3;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbCopy.Text);
            btnCopy.Content = "Copied";
        }
    }
}
