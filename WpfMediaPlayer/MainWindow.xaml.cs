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
        private bool isShuffleMode = false;
        Brush currentButtonColor;
        private bool isDraggingSlider = false; // Đang kéo thanh tua
        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            // Lấy màu hiện tại của nút
            currentButtonColor = ShuffleBtn.Background;
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
            if (isShuffleMode)
            {
                // Tạo danh sách các chỉ số của bài hát trong playlist
                List<int> playlistIndices = new List<int>();
                for (int i = 0; i < playlistListBox.Items.Count; i++)
                {
                    playlistIndices.Add(i);
                }

                // Loại bỏ chỉ số của bài hát vừa chạy xong khỏi danh sách
                playlistIndices.Remove(currentPlaylistIndex);

                // Chọn ngẫu nhiên một chỉ số từ danh sách còn lại
                Random random = new Random();
                int randomIndex = random.Next(playlistIndices.Count);

                // Chuyển đến bài hát ngẫu nhiên
                currentPlaylistIndex = playlistIndices[randomIndex];

                string nextFile = playlistListBox.Items[currentPlaylistIndex].ToString();
                mediaPlayer.Source = new Uri(nextFile);
                mediaPlayer.Play();
                progressSlider.Value = 0;
            }
            else
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
                progressSlider.Value = 0;
            }
            
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
            if (!isDraggingSlider)
            {
                // Người dùng không tua, cập nhật giá trị thanh tua từ media player
                if (mediaPlayer.Source != null)
                {
                    progressSlider.Value = mediaPlayer.Position.TotalSeconds;
                }
            }
        }

        private void PlayNextFile()
        {
            currentPlaylistIndex++;
            if (currentPlaylistIndex >= playlistListBox.Items.Count)
            {
                currentPlaylistIndex = 0;
            }
            string nextFile = playlistListBox.Items[currentPlaylistIndex].ToString();
            mediaPlayer.Source = new Uri(nextFile);
            mediaPlayer.Play();
            timer.Stop();
            progressSlider.Value = 0;
        }

        private void PlayPreviousFile()
        {
            currentPlaylistIndex--;
            if (currentPlaylistIndex < 0)
            {
                currentPlaylistIndex = playlistListBox.Items.Count - 1;
            }
            string previousFile = playlistListBox.Items[currentPlaylistIndex].ToString();
            mediaPlayer.Source = new Uri(previousFile);
            mediaPlayer.Play();
            timer.Stop();
            progressSlider.Value = 0;
        }

        private void PlayNextButtonClick(object sender, RoutedEventArgs e)
        {
            PlayNextFile();
        }

        private void PlayPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            PlayPreviousFile();
        }

        private void ShuffleModeButtonClick(object sender, RoutedEventArgs e)
        {
            isShuffleMode = !isShuffleMode;
            // thay đổi màu của button khi bật hoặc tắt shuffle mode
            UpdateShuffleButtonColor();
        }

        private void UpdateShuffleButtonColor()
        {
            if (isShuffleMode)
            {
                ShuffleBtn.Background = Brushes.Green; 
            }
            else
            {
                ShuffleBtn.Background = currentButtonColor; 
            }
        }

        private void progressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void progressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }
    }

}