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

namespace GuiApp.View
{
    /// <summary>
    /// Logique d'interaction pour GeneralOptions.xaml
    /// </summary>
    public partial class GeneralOptions : Page
    {
        // the instance of main
        private readonly MainViewModel main = MainViewModel.GetInstance();
        // the only instance of this class
        private static GeneralOptions instance;

        private GeneralOptions()
        {
            InitializeComponent();
            // initialized all labels and button of the interface
            Initialized();
        }

        public static GeneralOptions GetInstance()
        {
            if (instance == null)
            {
                instance = new GeneralOptions();
            }
            return instance;
        }

        // method for initialized interface components
        public void Initialized()
        {
            LabelLanguages.Content = GetText("optionLanguage");
            LabelType.Content = GetText("optionTypeOutput");
            LabelLogPath.Content = GetText("optionLogPath");
            LabelStatePath.Content = GetText("optionStatePath");
            ValidButtonOption.Text = GetText("validButton");
            LabelPathLogState.Content = GetText("pathLogState");
            LabelGeneralSettings.Content = GetText("optionsGeneralTitle");
            LabelSizeFiles.Content = GetText("filesSize");
            // initialized the combobox of languages
            InitializedLanguageComboBox();
            // initialized the combobox of log type
            InitializedTypeComboBox();
            // initialized the textbox of paths
            InitializedPaths();
        }

        // add item in languages combobox
        public void InitializedLanguageComboBox()
        {
            // read the language files
            JObject j = main.ReadJsonFile(@"..\..\..\Languages\languages.json");
            LanguageComboBox.Items.Clear();
            // for each languages
            foreach (var str_j in j)
            {
                // find the current language select
                bool isCurrent = false;
                if (str_j.Key == MainViewModel.Str_chooseLanguage)
                {
                    isCurrent = true;
                }
                // create item
                ComboBoxItem c = new ComboBoxItem()
                {
                    Content = str_j.Key,
                    IsSelected = isCurrent
                };
                // add item to combobox
                LanguageComboBox.Items.Add(c);
            }
        }

        // add item in type combobox
        public void InitializedTypeComboBox()
        {
            TypeComboBox.Items.Clear();
            // create the two type of log fle
            ComboBoxItem[] comboTab = {
                new ComboBoxItem()
                {
                    Content = ".xml",
                    IsSelected = false
                },
                new ComboBoxItem()
                {
                    Content = ".json",
                    IsSelected = false
                }
            };
            // put selected the current type
            if (LogViewModel.getInstance().B_isJson)
            {
                comboTab[1].IsSelected = true;
            }
            else
            {
                comboTab[0].IsSelected = true;
            }
            // add item to the combobox
            for (int i = 0; i < comboTab.Length; i++)
            {
                TypeComboBox.Items.Add(comboTab[i]);
            }
        }

        // initialized the two textbox for the different path
        public void InitializedPaths()
        {
            TextBoxLogPath.Text = LogViewModel.getInstance().Str_path;
            TextBoxStatePath.Text = StateViewModel.GetInstance().Str_path;
            TextBoxNbPause.Text = SaveViewModel.GetInstance().Nb_CopyPause.ToString();
        }

        // write in json the different informations
        public void ValidOptionsForm()
        {
            // get instance of log
            LogViewModel log = LogViewModel.getInstance();
            //  if the combobox is on json
            if (TypeComboBox.Text == ".json")
            {
                // change value
                log.B_isJson = true;
            }
            else
            {
                // change value to false
                log.B_isJson = false;
            }
            // write in settings the differnts values
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "type", TypeComboBox.Text);
            MainViewModel.Str_chooseLanguage = LanguageComboBox.Text;
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "language", LanguageComboBox.Text);
            log.Str_path = TextBoxLogPath.Text;
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "dirlog", TextBoxLogPath.Text);
            StateViewModel.GetInstance().Str_path = TextBoxStatePath.Text;
            main.WriteSettings(@"..\..\..\Settings\Settings.json", "dirstate", TextBoxStatePath.Text);
            try
            {
                SaveViewModel.GetInstance().Nb_CopyPause = Int32.Parse(TextBoxNbPause.Text);
                main.WriteSettings(@"..\..\..\Settings\Settings.json", "nbCopyPause", TextBoxNbPause.Text);
            } catch
            {
                MessageBox.Show(GetText("errorStringInt"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            MessageBox.Show(GetText("successOption"), "", MessageBoxButton.OK, MessageBoxImage.Information);
            // initialized all componant of interface
            MainWindow.Instance.Initialized();
        }

        // action for click on add valid button
        private void ValidFormOption_Click(object sender, RoutedEventArgs e)
        {
            ValidOptionsForm();
            Initialized();
        }

        // get the different text in language json
        private string GetText(string str_i)
        {
            return main.GetValueInJson(str_i);
        }
    }
}
