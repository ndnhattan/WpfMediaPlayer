using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Configuration;
using MahApps.Metro.IconPacks;


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
        
        private List<string> recentlyPlayed = new List<string>();
        private const int MaxRecentFiles = 5;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int MOD_CTRL = 0x0002; // Control key
        private const int WM_HOTKEY = 0x0312;

        private HwndSource _source;

        private bool isPlaying = false;
        // Lưu lại để phát tiếp lần sau
        private string savedPlaylist;
        private string savedVideo;
        private string savedPosition;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            // Lấy màu hiện tại của nút
            currentButtonColor = ShuffleBtn.Background;
            // Load cấu hình khi khởi động
            LoadConfiguration();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _source = PresentationSource.FromVisual(this) as HwndSource;
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        private void RegisterHotKey()
        {
            const int PLAY_PAUSE_HOTKEY_ID = 9000;
            const int NEXT_HOTKEY_ID = 9001;

            RegisterHotKey(_source.Handle, PLAY_PAUSE_HOTKEY_ID, MOD_CTRL, (int)KeyInterop.VirtualKeyFromKey(Key.Space));
            RegisterHotKey(_source.Handle, NEXT_HOTKEY_ID, MOD_CTRL, (int)KeyInterop.VirtualKeyFromKey(Key.N));
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                switch (wParam.ToInt32())
                {
                    case 9000: 
                        if (isPlaying)
                            PauseMedia();
                        else 
                            PlayMedia();

                        handled = true;
                        break;
                    case 9001:
                        PlayNextFile();

                        handled = true;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _source.RemoveHook(HwndHook);
            _source = null;

            try
            {
                savedVideo = currentPlaylistIndex.ToString();
                savedPosition = progressSlider.Value.ToString();
                SaveConfiguration();
            } catch (Exception ex) {
                
            }

            //UnregisterHotKey(_source.Handle, 9000); // Unregister hotkeys when closing the window
            //UnregisterHotKey(_source.Handle, 9001);
        }


        private void PlayMedia()
        {
            if (mediaPlayer.Source == null && playlistListBox.Items.Count > 0)
            {
                string selectedFile = playlistListBox.Items[currentPlaylistIndex].ToString();
                mediaPlayer.Source = new Uri(selectedFile);
            }
            mediaPlayer.Play();
            timer.Start();
            isPlaying = true;
        }

        bool isPause = true;
        private void PauseMedia()
        {
            mediaPlayer.Pause();
            timer.Stop();
            isPlaying = false;
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (isPause)
            {
                PlayMedia();
                isPause = false;
            }
            else
            {
                PauseMedia();
                isPause = false;
            }
            
        }

        /*private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            progressSlider.Value = 0;
            isPlaying = false;
        }*/

        private void VolumeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeSlider.Value;
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3;*.mp4;*.wav;*.avi;*.mkv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(openFileDialog.FileName);
            }
        }

        private void AddToPlayListButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
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
            string currentFile = playlistListBox.Items[currentPlaylistIndex].ToString();
            if (!recentlyPlayed.Contains(currentFile))
            {
                recentlyPlayed.Insert(0, currentFile); 
                if (recentlyPlayed.Count > MaxRecentFiles)
                {
                    recentlyPlayed.RemoveAt(recentlyPlayed.Count - 1);
                }    
            }

            UpdateRecentlyPlayedUI();

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

            mediaPlayer.Position = TimeSpan.Zero;

        }

        private void UpdateRecentlyPlayedUI()
        {
            recentlyPlayedListBox.Items.Clear();
            foreach(string file in recentlyPlayed)
            {
                recentlyPlayedListBox.Items.Add(file);  
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
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
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
                System.Windows.MessageBox.Show("Error saving");
            }
        }

        private void LoadPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Text Files|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                savedPlaylist = openFileDialog.FileName;
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
                    //System.Windows.MessageBox.Show("Playlist file not found.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading");
            }
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            progressSlider.IsEnabled = true;
            progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            //mediaPlayer.Position = TimeSpan.Zero;
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

            // Kiểm tra xem NaturalDuration có giá trị hợp lệ không
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            else
            {
                progressSlider.Maximum = 0;
            }

            timer.Stop();
            progressSlider.Value = 0;
            timer.Start();
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
            // Kiểm tra xem NaturalDuration có giá trị hợp lệ không
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            else
            {
                progressSlider.Maximum = 0;
            }

            timer.Stop();
            progressSlider.Value = 0;
            timer.Start();
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
            UpdateShuffleButtonIcon();
        }

        private void UpdateShuffleButtonIcon()
        {
            if (isShuffleMode)
            {
                btnShuffleIcon.Kind = PackIconMaterialKind.ShuffleVariant;
            }
            else
            {
                btnShuffleIcon.Kind = PackIconMaterialKind.Shuffle; 
            }
        }

        private void progressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
            mediaPlayer.Pause();
        }

        private async void progressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);

            if (isPlaying)
            {
                await Task.Delay(50);
                mediaPlayer.Play();
            }
        }

        private async void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider && isPlaying)
            {
                mediaPlayer.Play();
                mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
                await Task.Delay(50);
                mediaPlayer.Pause();
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["savedPlaylist"].Value = savedPlaylist;
                config.AppSettings.Settings["savedVideo"].Value = savedVideo;
                config.AppSettings.Settings["savedPosition"].Value = savedPosition;
                config.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
               
            }
        }

        private async void LoadConfiguration()
        {
            try
            {
                savedPlaylist = ConfigurationManager.AppSettings["savedPlaylist"];
                savedVideo = ConfigurationManager.AppSettings["savedVideo"];
                savedPosition = ConfigurationManager.AppSettings["savedPosition"];
                if (savedPlaylist != null && savedVideo != null && savedPosition != null
                    && savedPlaylist.Length > 0 && savedPosition.Length > 0 && savedVideo.Length > 0)
                {
                    int savedVideoIntValue = int.Parse(savedVideo);
                    double savedPositionDoubleValue = Convert.ToDouble(savedPosition);

                    // Thực hiện các bước để khôi phục thông tin vào ứng dụng
                    LoadPlaylistButtonClick(savedPlaylist);
                    currentPlaylistIndex = savedVideoIntValue;
                    if (currentPlaylistIndex >= playlistListBox.Items.Count)
                    {
                        currentPlaylistIndex = 0;
                    }
                    PlayMedia();
                    progressSlider.Value = savedPositionDoubleValue;
                    mediaPlayer.Position = TimeSpan.FromSeconds(savedPositionDoubleValue);
                    mediaPlayer.Play();
                }

            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show("a");
                currentPlaylistIndex = 0;
                playlistListBox.Items.Clear();
            }
        }
    }
}