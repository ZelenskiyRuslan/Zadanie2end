using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using Tmds.DBus.Protocol;
using Npgsql;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using static Zadanie2end.MainWindow;
using System.Threading.Tasks;

namespace Zadanie2end
{
    public class Weather
    {
        public Weather(string s, string n, string p)
        {
            city = s;
            temp1 = n;
            temp2 = p;
        }
        public string city { get; set; }
        public string temp1 { get; set; }
        public string temp2 { get; set; }

        public string View()
        {
            return "City: " + city + ", temp lo: " + temp1 + ", temp hi: " + temp2;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string connstr = "Host=localhost;Username=postgres;Password=postgres;Database=zad2";

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var username = this.FindControl<TextBox>("Username").Text;
            var password = this.FindControl<TextBox>("password").Text;

            List<Weather> weather = new List<Weather>();
            await using var dataSource = NpgsqlDataSource.Create(connstr);
            await using (var cmd = dataSource.CreateCommand("SELECT * FROM weather"))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    weather.Add(new Weather(reader.GetString(0), reader.GetInt32(1).ToString(), reader.GetInt32(2).ToString()));
                }
            }

            if (IsValidUser(username, password))
            {
                string mes = "";
                foreach (Weather weather1 in weather)
                {
                    mes += weather1.View() + "\n";
                }
                await ShowMessage(mes, "Weather Information");
            }
            else
            {
                await ShowMessage("Неверное имя пользователя или пароль.", "Ошибка");
            }
        }

        private bool IsValidUser(string username, string password)
        {
            using var dataSource = NpgsqlDataSource.Create(connstr);
            using (var cmd = dataSource.CreateCommand("SELECT COUNT(*) FROM users WHERE username = @username AND password = @password"))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password); // В реальном приложении пароли должны храниться в зашифрованном виде

                var result = cmd.ExecuteScalar();
                return (long)result > 0;
            }
        }

        private async Task ShowMessage(string message, string title)
        {
            var messageBox = new Window
            {
                Title = title,
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var textbb = new TextBlock { Text = message, Margin = new Thickness(10) };
            var closeButton = new Button { Content = "Закрыть", Margin = new Thickness(10) };
            closeButton.Click += (s, e) => messageBox.Close();

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(textbb);
            stackPanel.Children.Add(closeButton);
            messageBox.Content = stackPanel;

            await messageBox.ShowDialog(this);
        }
    }
}