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

        bool[] isRunning = new bool[3];
        int count = 0;
        int sum = 0;
        List<string> devices = null;

        Bitmap newRequest1 = (Bitmap)Bitmap.FromFile("new.png");
        Bitmap newRequest2 = (Bitmap)Bitmap.FromFile("new.png");
        Bitmap newRequest3 = (Bitmap)Bitmap.FromFile("new.png");

        Bitmap close1 = (Bitmap)Bitmap.FromFile("close.png");
        Bitmap close2 = (Bitmap)Bitmap.FromFile("close.png");
        Bitmap close3 = (Bitmap)Bitmap.FromFile("close.png");

        public MainWindow()
        {
            devices = KAutoHelper.ADBHelper.GetDevices();

            InitializeComponent();

            isRunning[0] = false;
            isRunning[1] = false;
            isRunning[2] = false;

            lvLogStatus.ItemsSource = logs;
            lvLogError.ItemsSource = errors;

            if (devices.Count == 0)
            {
                MessageBox.Show("Not found any instance!");
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                Status.Content = devices.Count + " device Connected!";
            }
        }

        private async void Start_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (Start.Content.ToString() == "Start")
            {
                sum = int.Parse(Count.Text);

                if (remainTurns.Text == "")
                {
                    Error.Visibility = Visibility.Visible;
                    Error.Content = "Chưa nhập số lượt hiện có";
                    return;
                }

                if (sum < 1)
                {
                    Error.Visibility = Visibility.Visible;
                    Error.Content = "Số lượt lắc phải lớn hơn 0";
                    return;
                }

                Error.Visibility = Visibility.Hidden;
                Error.Content = "";
                Note.Visibility = Visibility.Visible;

                if (devices.Count >= 1)
                {
                    StartDevice1.Visibility = Visibility.Visible;
                }

                if (devices.Count >= 2)
                {
                    StartDevice2.Visibility = Visibility.Visible;
                }

                if (devices.Count >= 3)
                {
                    StartDevice3.Visibility = Visibility.Visible;
                }

                Status.Content = "Running!";

                Start.Content = "Stop All";
                if (logs.Count == 0)
                {
                    logs.Add(new Log() { Status = "Đã nhận 0/" + sum + " lượt lắc" });
                }
            }
            else
            {
                Start.Content = "Start";
                Status.Content = "Ready!";

                StartDevice1.Visibility = Visibility.Hidden;
                StartDevice2.Visibility = Visibility.Hidden;
                StartDevice3.Visibility = Visibility.Hidden;
                Note.Visibility = Visibility.Hidden;

                isRunning[0] = false;
                isRunning[1] = false;
                isRunning[2] = false;

                StartDevice1.Content = "Start device 1";
                StartDevice2.Content = "Start device 2";
                StartDevice3.Content = "Start device 3";


                logs.Add(new Log() { Status = "Đã ngừng tất cả" });
            }
        }

        private void StartDevice1_Click(object sender, RoutedEventArgs e)
        {
            StartDevice(0, sender);
        }

        private void StartDevice2_Click(object sender, RoutedEventArgs e)
        {
            StartDevice(1, sender);
        }

        private void StartDevice3_Click(object sender, RoutedEventArgs e)
        {
            StartDevice(2, sender);
        }

        private void StartDevice(int index, object sender)
        {
            if (((Button)sender).Content.ToString() == ("Start device " + (index + 1)))
            {
                if (count < sum)
                {
                    isRunning[index] = true;
                    ((Button)sender).Content = "Stop device " + (index + 1);
                    logs.Add(new Log() { Status = DateTime.Now.ToString("HH:mm:ss tt ") + "Đang chạy máy " + (index + 1) });
                    //Auto(devices[index]);
                }
                else
                {
                    Error.Content = "Số lượt lắc đã vượt quá giới hạn";
                }
            }
            else
            {
                isRunning[index] = false;
                logs.Add(new Log() { Status = "Ngừng chạy máy " + (index + 1) });
                ((Button)sender).Content = "Start device " + (index + 1);
            }
        }

        private string GetRandomAlphaNumeric(int n)
        {
            string s = "";
            Random random = new Random();
            // Any random integer   
            for (int i = 0; i < n; i++)
            {
                int num = random.Next(10);
                s += num;
            }
            return s;
        }

        async void Auto(string deviceID)
        {
            await Task.Run(() =>
            {
                int indexDevice = 0;
                string nameDevice = "";

                if (deviceID == devices[0])
                {
                    indexDevice = 0;
                    nameDevice = "Máy 1";
                }
                else if (deviceID == devices[1])
                {
                    indexDevice = 1;
                    nameDevice = "Máy 2";
                }
                else if (deviceID == devices[2])
                {
                    indexDevice = 2;
                    nameDevice = "Máy 3";
                }

                if (count > sum)
                {
                    return;
                }

                if (isRunning[indexDevice])
                {
                    var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                    Bitmap nq = null;

                    if (deviceID == devices[0])
                    {
                        nq = newRequest1;
                    }
                    else if (deviceID == devices[1])
                    {
                        nq = newRequest2;
                    }
                    else if (deviceID == devices[2])
                    {
                        nq = newRequest3;
                    }

                    if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, nq) != null)
                    {
                        KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 87.9);
                    }
                    else
                    {
                        KAutoHelper.ADBHelper.TapByPercent(deviceID, 6.2, 13.9);
                    }

                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 5.8, 32.3);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.InputText(deviceID, "20000");
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 15.0, 58.7);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    if (deviceID == devices[0])
                    {
                        KAutoHelper.ADBHelper.InputText(deviceID, "0338" + GetRandomAlphaNumeric(6));
                    }
                    else if (deviceID == devices[1])
                    {
                        KAutoHelper.ADBHelper.InputText(deviceID, "0397" + GetRandomAlphaNumeric(6));
                    }
                    else if (deviceID == devices[2])
                    {
                        KAutoHelper.ADBHelper.InputText(deviceID, "0352" + GetRandomAlphaNumeric(6));
                    }
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 5.1, 58.1);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 49.3, 92.7);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 5.1, 13.7);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.InputText(deviceID, count.ToString());
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 49.6, 93.3);
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 47.8, 79.2); // nhap pass "2"
                    Delay(1, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 47.8, 79.2); // nhap pass "2"
                    Delay(1, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 47.8, 79.2); // nhap pass "2"
                    Delay(1, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 85.1); // nhap pass "5"
                    Delay(1, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 85.1); // nhap pass "5"
                    Delay(1, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.9, 85.1); // nhap pass "5"
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    int step = 0;
                    while (step < 30 && isRunning[indexDevice])
                    {
                        step++;

                        var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                        if (deviceID == devices[0])
                        {
                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close1) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                                break;
                            }
                        }
                        else if (deviceID == devices[1])
                        {

                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close2) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                                break;
                            }
                        }
                        else if (deviceID == devices[2])
                        {
                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close3) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                                break;
                            }
                        }

                        Delay(100, isRunning[indexDevice]);
                    }

                    if (step == 30) // đụng độ
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            errors.Insert(0, new Log() { Status = DateTime.Now.ToString("HH:mm:ss tt ") + nameDevice + " đụng độ. Tự động fix" });
                        });

                        if (isRunning[indexDevice])
                        {
                            KAutoHelper.ADBHelper.Key(deviceID, ADBKeyEvent.KEYCODE_APP_SWITCH); // thoát app
                            Delay(2000, isRunning[indexDevice]);
                        }

                        if (isRunning[indexDevice])
                        {
                            KAutoHelper.ADBHelper.TapByPercent(deviceID, 51.7, 41.1); // mở lại app
                            Delay(3000, isRunning[indexDevice]);
                        }

                        var screen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
                        if (deviceID == devices[0])
                        {
                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close1) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                            }
                            else
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 13.6, 46.8);
                                }
                            }
                        }
                        else if (deviceID == devices[1])
                        {
                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close2) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                            }
                            else
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 13.6, 46.8);
                                }
                            }
                        }
                        else if (deviceID == devices[2])
                        {
                            if (KAutoHelper.ImageScanOpenCV.FindOutPoint(screen, close3) != null)
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 48.2, 56.7); // click đóng
                                }
                            }
                            else
                            {
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                                    Delay(1000, isRunning[indexDevice]);
                                }
                                if (isRunning[indexDevice])
                                {
                                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 13.6, 46.8);
                                }
                            }
                        }
                    }

                    Delay(1000, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 46.4, 28.0); // click mở 1 item
                    Delay(1000, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 67.6, 34.1); // click 3 chấm
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 46.8, 94.9); // cập nhật trạng thái
                    Delay(500, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 24.2, 79.0); // da thanh toan
                    Delay(1000, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 49.3, 93.3); // xac nhan
                    Delay(3000, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    KAutoHelper.ADBHelper.TapByPercent(deviceID, 4.1, 7.1); // tro ve
                    Delay(2000, isRunning[indexDevice]);
                }

                if (isRunning[indexDevice])
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        logs.Add(new Log() { Status = "+3 luot lac" });
                        count += 3;
                        int countLog = 0;
                        for (int i = 0; i < logs.Count; i++)
                        {
                            if (logs[i].Status == "+3 luot lac")
                            {
                                countLog += 3;
                            }
                        }
                        logs[0] = new Log() { Status = DateTime.Now.ToString("HH:mm:ss tt ") + "Đã nhận " + countLog + "/" + sum + " lượt lắc" };
                    });
                }

                if (isRunning[indexDevice])
                {
                    Auto(deviceID);
                }
            });
        }

        void Delay(int delay, bool isRunning)
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

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbCopy.Text);
            btnCopy.Content = "Copied";
        }
    }
}
