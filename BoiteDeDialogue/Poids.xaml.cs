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
using System.Windows.Shapes;

namespace CashphotoWPF.BoiteDeDialogue
{
    /// <summary>
    /// Logique d'interaction pour Poids.xaml
    /// </summary>
    public partial class Poids : Window
    {
        private MainWindow _app; //un accès à la fenêtre principale
        private TextBox _focusedTextBox { get; set; }
        public Poids(MainWindow app)
        {
            InitializeComponent();
            InputTextBox.Focus();
            _app = app;
        }

        private void Valider(object sender, RoutedEventArgs e)
        {
            if (_app.isValidPoids(InputTextBox.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ErreurPoids.Content = "Veuillez entrer un poids valide.";
            }
        }

        private void Annuler(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "";
            this.DialogResult = false;
            this.Close();
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _focusedTextBox = InputTextBox;
        }

        //-------------Clavier Virtuel-------------

        private void Key0_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "0";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key1_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "1";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key2_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "2";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key3_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "3";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key4_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "4";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key5_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "5";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void Key6_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "6";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key7_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "7";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key8_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "8";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }
        private void Key9_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += "9";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void KeyDot_Click(object sender, RoutedEventArgs e)
        {
            _focusedTextBox.Text += ".";
            _focusedTextBox.CaretIndex = _focusedTextBox.Text.Length;
        }

        private void KeyDelete_Click(object sender, RoutedEventArgs e)
        {
            //_focusedTextBox
            var routedEvent = Keyboard.KeyDownEvent;
            _focusedTextBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(_focusedTextBox), 0, Key.Back) { RoutedEvent = routedEvent });

        }

        private void KeyEnter_Click(object sender, RoutedEventArgs e)
        {
            var routedEvent = Keyboard.KeyDownEvent;
            _focusedTextBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(_focusedTextBox), 0, Key.Enter) { RoutedEvent = routedEvent });
        }

    }
}
