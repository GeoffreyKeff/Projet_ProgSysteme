using GuiApp.Model;
using GuiApp.View;
using GuiApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace GuiApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string str_title;
        private readonly MainViewModel main;
        private static MainWindow instance;

        public MainWindow()
        {
            Process[] localByName = Process.GetProcessesByName("GuiApp");
            int r = localByName.Length;
            if (r > 1)
            {
                MessageBox.Show("Application already launch");
                Environment.Exit(0);
            }
            main = MainViewModel.GetInstance();
            main.InsertInL_save();
            InitializeComponent();
            this.Str_title = "welcome";
            this.Initialized();
            instance = this;
            Main.Content = SaveView.GetInstance();
        }

        public new void Initialized()
        {
            Title.Content = GetText(this.str_title);
            SaveButtonTextBlock.Text = GetText("saveButton");
            CreateSaveButtonTextBlock.Text = GetText("createSaveButton");
            OptionsButtonTextBlock.Text = GetText("optionsButton");
            generalOptionsTitle.Text = GetText("generalOptionsTitle");
            QuitButtonTextBlock.Text = GetText("quitButton");
        }

        // action for the save button
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Str_title = "saveTitle";
            SaveView sa = SaveView.GetInstance();
            Main.Content = sa;
            sa.PrintAllSaves();
            sa.Initialized();
            Initialized();
        }

        private void CreateCompleteSaveButton_Click(object sender, RoutedEventArgs e)
        {            
            this.Str_title = "createSaveTitle";
            CreateSaveView ca = CreateSaveView.GetInstance();
            Main.Content = ca;
            ca.InitializedLabels();
            Initialized();
        }

        // action for the options button
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Str_title = "optionsTitle";
            Main.Content = Options.GetInstance();
            Initialized();
        }

        private void GeneralOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Str_title = "generalOptionsTitle";
            Main.Content = GeneralOptions.GetInstance();
            Initialized();
        }

        // action for quit the application
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        // get the different text in language json
        private string GetText(string str_i)
        {
            return main.GetValueInJson(str_i);
        }

        public string Str_title
        {
            get { return this.str_title; }
            set { this.str_title = value; }
        }

        public static MainWindow Instance
        {
            get { return instance; }
            set { instance = value; }
        }
    }
}
