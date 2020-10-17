using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleLexiconGeneratorCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsPlaying = false;
        Backend BE = new Backend();
        const string SAVING_PATH = "Info.bin";
        Storage St = new Storage();
        public MainWindow()
        {
            if (!IsValidTimeSpan())
            {
                MessageBox.Show("Unable to load program, please contact author for further information.");
                Close();
            }
            InitializeComponent();
            if (CheckSaving())
            {
                Init();
                EnableBottons();
            }
        }
        private bool CheckSaving()
        {
            if (!File.Exists(SAVING_PATH))
                return false;
            try
            {
                St.Load(SAVING_PATH);
                BE.InputTextFilePath = St.CurrentTextFile;
                BE.InputAudioFolderPath = St.CurrentAudioFolder;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        private void BtnRecordStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsPlaying)
                {
                    Stop();
                }
                else
                {
                    Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Stop()
        {
            BE.Stop();
            IsPlaying = false;
            BtnRecordStop.Foreground = Brushes.Green;
            BtnRecordStop.Content = "Play";
        }
        private void Play()
        {
            BE.Play();
            IsPlaying = true;
            BtnRecordStop.Foreground = Brushes.Red;
            BtnRecordStop.Content = "Stop";
        }
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            try
            {
                SaveCurrentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            BE.IndexIncrease();
            int i = BE.CurrentIndex;

            FillBoxes();
            DataCollection.SelectedIndex = i;
        }
        private void SaveCurrentData()
        {
            BE.SaveCurrentData(BoxTransliteration.Text, BoxHighGerman.Text, BoxContext.Text, BoxLexicon.Text);
            BE.SaveAllData(BE.InputTextFilePath);
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            if (!IsValidTimeSpan())
            {
                MessageBox.Show("Unable to load program, please contact author for further information.");
                Close();
            }
            try
            {
                SaveCurrentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            MessageBox.Show(BE.IncompleteTasksInfo());
            St.Save(SAVING_PATH);
            SaveFileDialog d = new SaveFileDialog();
            if (d.ShowDialog() == true)
            {
                BE.SaveAllData(d.FileName);
            }
        }
        private void DataCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stop();
            try
            {
                SaveCurrentData();
            }
            catch (Exception)
            {

            }
            BE.SetIndex(DataCollection.SelectedIndex);
            FillBoxes();
        }
        private void FillBoxes()
        {
            var line = BE.CurrentLine;
            BoxTransliteration.Text = line.Accent;
            BoxContext.Text = line.Context;
            BoxHighGerman.Text = line.HighGerman;
            BoxLexicon.Text = line.Lexicon;
        }
        private bool TextFileSet = false;
        private void BtnTextFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                BE.InputTextFilePath = fileDialog.FileName;
                TextFileSet = true;
                if (AudioFolderSet)
                {
                    EnableBottons();
                    Init();
                }
            }
        }
        private bool AudioFolderSet = false;
        private void BtnAudioFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BE.InputAudioFolderPath = folderDialog.SelectedPath;
                    AudioFolderSet = true;
                    if (TextFileSet)
                    {
                        EnableBottons();
                        Init();
                    }
                }
            }
        }
        private void EnableBottons()
        {
            BtnRecordStop.IsEnabled = true;
            BtnNext.IsEnabled = true;
            BtnSave.IsEnabled = true;
        }
        private void Init()
        {
            try
            {
                BE.Init();
                St.OverrideValues(BE.InputTextFilePath, BE.InputAudioFolderPath);
                IsPlaying = false;
                BoxColumn1.Text = BE.Column1String;
                BoxColumn2.Text = BE.Column2String;
                BoxColumn3.Text = BE.Column3String;
                BoxColumn4.Text = BE.Column4String;
                SetListBox();
                FillBoxes();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void SetListBox()
        {
            DataCollection.ItemsSource = BE.AudioNames;
        }
        static readonly DateTime ExpireTime = new DateTime(2021, 1, 1);
        private bool IsValidTimeSpan()
        {
            try
            {
                var req = WebRequest.CreateHttp("http://www.microsoft.com");
                using (var resp = req.GetResponse())
                {
                    string timeString = resp.Headers["date"];
                    DateTime dt = DateTime.ParseExact(timeString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
                    return dt < ExpireTime;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
