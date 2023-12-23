using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentPlaylistIndex = 0;
        
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        
        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.Items.Count > 0)
            {
                string selectedFile = playlistListBox.Items[currentPlaylistIndex].ToString();
                mediaPlayer.Source = new Uri(selectedFile);
            }
            mediaPlayer.Play();
            timer.Start();
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            progressSlider.Value = 0;
        }

        private void VolumeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeSlider.Value;
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3;*.mp4;*.wav;*.avi;*.mkv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName);
            }
        }

        private void AddToPlayListButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Media Files|*.mp3;*.mp4;*.wav;*.avi;*.mkv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach(string filename in openFileDialog.FileNames)
                {
                    playlistListBox.Items.Add(filename);
                }
            }
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            currentPlaylistIndex++;
            if (currentPlaylistIndex < playlistListBox.Items.Count)
            {
                string nextFile = playlistListBox.Items[currentPlaylistIndex].ToString();
                mediaPlayer.Source = new Uri(nextFile);
                mediaPlayer.Play();
            }
            else
            {
                currentPlaylistIndex = 0;
            }
            timer.Stop();
            progressSlider.Value = 0;
        }

        private void RemoveSelectedButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlistListBox.SelectedItems.Count > 0)
            {
                List<string> itemsToRemove = new List<string>();
                foreach(var selectedItem in playlistListBox.SelectedItems)
                {
                    itemsToRemove.Add(selectedItem.ToString());
                }
                foreach (var itemToRemove in itemsToRemove)
                {
                    playlistListBox.Items.Remove(itemToRemove);
                }
            }
        }

        private void SavePlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                SavePlaylist(saveFileDialog.FileName);
            }
        }

        private void SavePlaylist(string fileName)
        {
            try
            {
                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach(var item in playlistListBox.Items)
                    {
                        writer.WriteLine(item.ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error saving");
            }
        }

        private void LoadPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                playlistListBox.Items.Clear();
                LoadPlaylistButtonClick(openFileDialog.FileName);
            }
        }

        private void LoadPlaylistButtonClick(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    string[] lines = File.ReadAllLines(fileName);
                    foreach(string line in lines)
                    {
                        playlistListBox.Items.Add(line);
                    }
                }
                else
                {
                    MessageBox.Show("Playlist file not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading");
            }
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            progressSlider.IsEnabled = true;
            progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                progressSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }
    }
}