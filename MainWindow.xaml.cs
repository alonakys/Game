using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;

namespace Game
{
    public partial class MainWindow : Window
    {
        private Horse[] _horses;
        private Horse[] _horseOnScreen;
        private CancellationTokenSource _cancellationTokenSource;
        private int BankAccount { get; set; }
        private int Reserve = 20;
        private string HorseBetName { get; set; }
        private int horseIndex = 1;
        public MainWindow()
        {
            _horses = new Horse[5]
            {
                new Horse("Lucky", Brushes.Red),
                new Horse("Barn", Brushes.Green),
                new Horse("Arni", Brushes.Yellow),
                new Horse("Justi", Brushes.Blue),
                new Horse("Fasti", Brushes.Firebrick)
            };
            _horseOnScreen = (Horse[])_horses.Clone();

            BankAccount = 250;

            InitializeComponent();
        }

        public void StopProcess()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void SetHorses(Horse[] horses)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateHorseInformation(horses[0], firstColor, firstName, firstCoefficient, firstTime, firstPosition);
                UpdateHorseInformation(horses[1], secondColor, secondName, secondCoefficient, secondTime, secondPosition);
                UpdateHorseInformation(horses[2], thirdColor, thirdName, thirdCoefficient, thirdTime, thirdPosition);
                UpdateHorseInformation(horses[3], fourthColor, fourthName, fourthCoefficient, fourthTime, fourthPosition);
                UpdateHorseInformation(horses[4], fifthColor, fifthName, fifthCoefficient, fifthTime, fifthPosition);
            });
        }
        public void UpdateHorseInformation(Horse horse, Rectangle color, Label name, Label acceration, Label time, Label position)
        {
            color.Fill = horse.Color;
            name.Content = horse.Name;
            acceration.Content = horse.Accelaration;
            time.Content = horse.Timer.Elapsed;
            position.Content = horse.Position;
        }
        public async Task LaunchHorses(Horse[] horses)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < horses.Length; i++)
            {
                tasks.Add(horses[i].RunAsync());
            }

            await Task.WhenAll(tasks);
        }

        private List<Task> RenderHorseAnimation()
        {
            List<List<ImageSource>> horsesAnimation = new List<List<ImageSource>>();

            Color[] colors = new Color[5] { Colors.Red, Colors.Green, Colors.Yellow, Colors.Blue, Colors.Firebrick };

            foreach (var color in colors)
            {
                horsesAnimation.Add(GetHorseAnimation(color));
            }

            List<Task> horsesTask = new List<Task>
            {
                PlayAnimation(horsesAnimation[0], horse_1),
                PlayAnimation(horsesAnimation[1], horse_2),
                PlayAnimation(horsesAnimation[2], horse_3),
                PlayAnimation(horsesAnimation[3], horse_4),
                PlayAnimation(horsesAnimation[4], horse_5)
            };

            return horsesTask;
        }
        private async void RunProgram(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            List<Task> horsesAnimation = RenderHorseAnimation();

            Task updateRatingPositionHorses = UpdateRatingPositionHorses();
            Task launchHorses = LaunchHorses(_horses);
            Task changePositionHorses = ChangePositionHorses();

            await Task.WhenAll(launchHorses, updateRatingPositionHorses, changePositionHorses);
            await Task.WhenAll(horsesAnimation);

            MessageBox.Show("Race finished");
        }

        private async Task PlayAnimation(List<ImageSource> animationFrames, Image targetImage)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                foreach (var frame in animationFrames)
                {
                    await Task.Run(() =>
                    {
                        targetImage.Dispatcher.Invoke(() =>
                        {
                            targetImage.Source = frame;
                        });
                    });

                    await Task.Delay(TimeSpan.FromSeconds(0.1));

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                }
            }
        }
        private void PositionChanges(Image horse, int i)
        {
            double horseChangePositionValue = _horseOnScreen[i].Position % 800;
            horse.Margin = new Thickness(horseChangePositionValue, 0, 0, 0);
        }
        private async Task ChangePositionHorses()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PositionChanges(horse_1, 0);
                        PositionChanges(horse_2, 1);
                        PositionChanges(horse_3, 2);
                        PositionChanges(horse_4, 3);
                        PositionChanges(horse_5, 4);
                    });

                    Task.Delay(100).Wait();

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                }
            });
        }
        public List<ImageSource> GetHorseAnimation(Color color)
        {
            const int count = 12;
            var bitmap_image_list = ReadImageList(@"Images\Horses", "WithOutBorder_", ".png", count);
            var mask_image_list = ReadImageList(@"Images\HorsesMask", "mask_", ".png", count);
            return bitmap_image_list.Select((item, index) => GetImageWithColor(item, mask_image_list[index], color)).ToList();
        }
        private List<BitmapImage> ReadImageList(string path, string name, string format, int count)
        {
            path = $@"D:\Кисіль\Telegram Desktop\Game\Game\{path}\{name}";
            List<BitmapImage> list = new List<BitmapImage>();
            for (int i = 0; i < count; i++)
            {
                var uri = path + string.Format("{0:0000}", i) + format;
                var img = new BitmapImage(new Uri(uri));
                list.Add(img);
            }
            return list;
        }
        private ImageSource GetImageWithColor(BitmapImage image, BitmapImage mask, Color color)
        {
            WriteableBitmap image_bmp = new WriteableBitmap(image);
            WriteableBitmap mask_bmp = new WriteableBitmap(mask);
            WriteableBitmap output_bmp = BitmapFactory.New(image.PixelWidth, image.PixelHeight);
            output_bmp.ForEach((x, y, z) =>
            {
                return MultiplyColors(image_bmp.GetPixel(x, y), color, mask_bmp.GetPixel(x, y).A);
            });
            return output_bmp;
        }
        private Color MultiplyColors(Color color_1, Color color_2, byte alpha)
        {
            var amount = alpha / 255.0;
            byte r = (byte)(color_2.R * amount + color_1.R * (1 - amount));
            byte g = (byte)(color_2.G * amount + color_1.G * (1 - amount));
            byte b = (byte)(color_2.B * amount + color_1.B * (1 - amount));
            return Color.FromArgb(color_1.A, r, g, b);
        }
        public async Task UpdateRatingPositionHorses()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!_horses[0].Timer.IsRunning && !_horses[1].Timer.IsRunning && !_horses[2].Timer.IsRunning && !_horses[3].Timer.IsRunning && !_horses[4].Timer.IsRunning)
                    {
                        _horses = Horse.ChangePositionRaiting(_horses);

                        if (!string.IsNullOrEmpty(HorseBetName) && HorseBetName.Contains(_horses[0].Name))
                        {
                            BankAccount += Reserve * 2;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            BalanceContent.Content = $"Balance: {BankAccount}$";
                        });

                        StopProcess();
                    }

                    Dispatcher.Invoke(() =>
                    {
                        SetHorses(_horses);
                    });

                    _horses = Horse.ChangePlace(_horses);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            horseIndex %= 5;
            HorsesNameContent.Content = $"{horseIndex + 1}. " + _horses[horseIndex].Name;
            horseIndex++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (horseIndex == 0)
            {
                horseIndex = _horses.Length - 1;
            }
            else
            {
                horseIndex--;
            }

            HorsesNameContent.Content = $"{horseIndex + 1}. " + _horses[horseIndex].Name;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Reserve += 5;
            MoneyThatPayed.Content = Reserve.ToString() + "$";
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Reserve > 0)
            {
                Reserve -= 5;
            }
            MoneyThatPayed.Content = Reserve.ToString() + "$";
        }

        private void Bet(object sender, RoutedEventArgs e)
        {
            if (BankAccount - Reserve >= 0)
            {
                BankAccount -= Reserve;
                BalanceContent.Content = $"Balance: {BankAccount}$";
                MessageBox.Show($"You are bet in {HorsesNameContent.Content} {Reserve}$");
                HorseBetName = HorsesNameContent.Content.ToString();
            }
            else
            {
                MessageBox.Show($"Not enough money. Needed {Reserve - BankAccount}");
            }
        }
    }
    public class Horse
    {
        public string Name { get; private set; }
        public Brush Color { get; private set; }
        public double Accelaration { get; private set; }
        public double Position { get; private set; }

        public Stopwatch Timer;

        public Horse(string name, Brush color)
        {
            Name = name;
            Color = color;
            Position = -700;

            Accelaration = new Random().Next(10, 16);

            Timer = new Stopwatch();
            Timer.Start();
        }
        public void ChangeAccelaration()
        {
            double value = new Random().Next(10, 16) / 10.0;

            Position += Accelaration * value;
            //Position += 3;
        }
        public async Task RunAsync()
        {
            while (true)
            {
                if (Position >= 720)
                {
                    Timer.Stop();
                    break;
                }

                ChangeAccelaration();

                await Task.Delay(100 + new Random().Next(0, 500));
            }
        }

        public static Horse[] ChangePlace(Horse[] horses)
        {
            horses = horses.OrderByDescending(x => x.Position).ToArray();

            return horses;
        }

        public static Horse[] ChangePositionRaiting(Horse[] horses)
        {
            horses = horses.OrderBy(x => x.Timer.ElapsedMilliseconds).ToArray();

            return horses;
        }
    }
}