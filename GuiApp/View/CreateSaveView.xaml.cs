using GuiApp.Model;
using GuiApp.ViewModel;
using Newtonsoft.Json.Linq;
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
using System.Windows.Forms;

namespace GuiApp
{
    public partial class CreateSaveView : Page
    {
        // get main instance
        private readonly MainViewModel main = MainViewModel.GetInstance();
        // get saveViewModel instance
        private readonly SaveViewModel sVM = SaveViewModel.GetInstance();
        // the only instance of this class
        private static CreateSaveView instance;

        private CreateSaveView()
        {
            InitializeComponent();
            InitializedLabels();
        }

        public static CreateSaveView GetInstance()
        {
            if (instance == null)
            {
                instance = new CreateSaveView();
            }
            return instance;
        }

        // initialized all the label for the created label
        public void InitializedLabels()
        {
            NameLabel.Content = GetText("createSaveName");
            SourceLabel.Content = GetText("createSaveSource");
            DestLabel.Content = GetText("createSaveDest");
            ValidSaveButton.Text = GetText("validButton");
            isDif.Content = GetText("createSaveDif");
            isCrypt.Content = GetText("checkIfCrypt");
        }

        // create a save
        public int CreateSave(int str_i)
        {
            // get info in the interface
            string str_name = NameTextBox.Text;
            string str_source = SourceTextBox.Text;
            string str_destination = DestTextBox.Text;
           
            bool b_crypt = false;
            // check if the crypt button is select
            if ((bool)isCrypt.IsChecked)
            {
                b_crypt = true;
            }
            // check if informations are correct or not
            int nb_j = sVM.CreateSave(str_name, str_source, str_destination, str_i, b_crypt);
            if (nb_j > 0)
            {
                return nb_j;
            }
            else
            {
                // if the save is create put value to null
                NameTextBox.Text = "";
                SourceTextBox.Text = "";
                DestTextBox.Text = "";
                isDif.IsChecked = false;
            }
            return nb_j;
        }

        // valid form action
        private void ValidFormSaveComplete_Click(object sender, RoutedEventArgs e)
        {
            // check different values
            int nb_type = 0;
            if ((bool)isDif.IsChecked)
            {
                nb_type = 1;
            }
            int nb_j = CreateSave(nb_type);
            // created save and print error

            switch (nb_j)
            {
                case (0):
                    System.Windows.MessageBox.Show(GetText("successCreate"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case (1):
                    System.Windows.MessageBox.Show(GetText("errorName"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case (2):
                    System.Windows.MessageBox.Show(GetText("errorSourceFile"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case (3):
                    System.Windows.MessageBox.Show(GetText("errorDestFile"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case (4):
                    System.Windows.MessageBox.Show(GetText("NameNoSpecialCharacter"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;

            }
        }

        // get the different text in language json
        private string GetText(string str_i)
        {
            return main.GetValueInJson(str_i);
        }

      
        private void PathSource(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceTextBox.Text = fdb.SelectedPath;
            }
            
        }
        private void PathDest(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fdb = new FolderBrowserDialog();
            if (fdb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestTextBox.Text= fdb.SelectedPath;
            }

        }

        
    }
}
