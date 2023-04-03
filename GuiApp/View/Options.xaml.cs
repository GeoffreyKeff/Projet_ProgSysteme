using GuiApp.Model;
using GuiApp.View;
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

namespace GuiApp
{
    public partial class Options : Page
    {
        // the instance of main
        private readonly MainViewModel main = MainViewModel.GetInstance();
        // the only instance of this class
        private static Options instance;

        private Options()
        {
            InitializeComponent();
            // initialized all labels and button of the interface
            Initialized();
        }

        public static Options GetInstance()
        {
            if(instance == null)
            {
                instance = new Options();
            }
            return instance;
        }

        // method for initialized interface components
        public void Initialized()
        {
            LabelTitleExtensions.Content = GetText("optionAddExtensions");
            LabelTitlePriorityExtensions.Content = GetText("optionsAddExtensionsPriority");
            // initialized the label of extensions
            InitializedExtensions();
            InitializedPriorityExtensions();

        }

        public void InitializedPriorityExtensions()
        {
            ListBox listbox = LabelPriorityExtensions;
            foreach (string str in SaveViewModel.GetInstance().L_priorityExtension)
            {
                listbox.Items.Add(str);
            }
        }

        // print all extensions that already exist
        public void InitializedExtensions()
        {
            ListBox listbox = LabelExtensions;
            foreach (string str in SaveViewModel.GetInstance().L_cryptExtension)
            {
                listbox.Items.Add(str);
            }
        }

        // add extension in the list
        public void AddExtensions()
        {
            // get the list of extension
            List<string> l_str = SaveViewModel.GetInstance().L_cryptExtension;
            string str = TextBoxExtensions.Text;
            if (str != "")
            {
                // if the exstension already exist in the list
                if (l_str.Find(x => x == str) != null)
                {
                    // delete it
                    l_str.Remove(str);
                }
                else if (l_str.Find(x => x == str) == null)
                {
                    // add it
                    l_str.Add(str);
                }
            }
            // write in the settings json the new extension list
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "filetype", "");
            ListBox listbox = LabelExtensions;
            listbox.Items.Add(str);
        }


        public void AddPriorityExtensions()
        {
            // get the list of extension
            List<string> l_str = SaveViewModel.GetInstance().L_priorityExtension;
            string str = TextBoxPriorityExtensions.Text;
            if (str != "")
            {
                // if the exstension already exist in the list
                if (l_str.Find(x => x == str) != null)
                {
                    // delete it
                    l_str.Remove(str);
                }
                else if (l_str.Find(x => x == str) == null)
                {
                    // add it
                    l_str.Add(str);
                }
            }

            // write in the settings json the new extension list
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "filePrioritytype", "");
            ListBox listbox = LabelPriorityExtensions;
            listbox.Items.Add(str);
        }

        // write in json the different informations


        // action for click on add valid button
        private void ValidFormOption_Click(object sender, RoutedEventArgs e)
        {
            Initialized();
        }

        // action for click on add extension button
        private void AddExtenstionButton_Click(object sender, RoutedEventArgs e)
        {
            AddExtensions();
        }
        private void AddPriorityExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            AddPriorityExtensions();
        }

        public void ClickRemovePriorityExtension(object sender, RoutedEventArgs e)
        {
            var extension = LabelPriorityExtensions.SelectedItem;
            if (extension != null)
            {
                LabelPriorityExtensions.Items.Remove(extension.ToString());
                SaveViewModel.GetInstance().L_priorityExtension.Remove(extension.ToString());
                main.WriteSettings(@"..\..\..\Settings\Settings.json", "filePrioritytype", "");
            }
        }

        public void ClickRemoveExtension(object sender, RoutedEventArgs e)
        {
            var extension = LabelExtensions.SelectedItem;
            if (extension != null)
            {
                LabelExtensions.Items.Remove(extension.ToString());
                SaveViewModel.GetInstance().L_cryptExtension.Remove(extension.ToString());
                main.WriteSettings(@"..\..\..\Settings\Settings.json", "filetype", "");

            }
        }

        public void EasterEggHidden(object sender, RoutedEventArgs e)
        {
            EasterEgg.Visibility = Visibility.Hidden;
            EasterEgg2.Visibility = Visibility.Hidden;
        }

        public void EasterEggVisible(object sender, RoutedEventArgs e) 
        {
            EasterEgg.Visibility = Visibility.Visible;
            EasterEgg2.Visibility = Visibility.Visible;
        }


        // get the different text in language json
        private string GetText(string str_i)
        {
            return main.GetValueInJson(str_i);
        }
    }
}
