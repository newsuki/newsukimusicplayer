using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using NAudio.Wave;
using System.IO;
using YoutubeExplode;
using System.Runtime.CompilerServices;
using YoutubeExplode.Videos.Streams;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;

namespace MusicPlayer_AK3TRB
{
    public partial class Form1 : Form
    {
        private List<SongInfo> playedSongs = new List<SongInfo>();
        private List<SongInfo> playlistSongs = new List<SongInfo>();

        public WaveOutEvent waveOut;
        private AudioFileReader audioFileReader;
        public string videoUrl;
        public bool youtubePlay = false;

        private string playlistFilePath = "playlist.dat";

        public Form1()
        {
            InitializeComponent();
            waveOut = new WaveOutEvent();
            progressBar.Minimum = 0;
            songBar.Minimum = 0;

            LoadPlaylist();

        }

        private void openFileMenuStripItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "MP3/WAV Files|*.mp3;*.wav|All Files|*.*";
            openFileDialog.Title = "Select a .mp3/.wav file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDialog.FileName;

                try
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = new WaveOutEvent();
                    audioFileReader = new AudioFileReader(selectedFileName);
                    songName.Text = selectedFileName;
                    songName.Font = new Font(songName.Font, FontStyle.Bold);
                    waveOut.Init(audioFileReader);
                    progressBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                    songBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;


                    UpdateStatusLabel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                waveOut.Play();
                UpdateProgressBarAsync();
                songStatus.Text = "Playing";
            }
            catch (Exception)
            {
                MessageBox.Show("Load a song first!");
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            waveOut.Stop();
            waveOut.Dispose();

            progressBar.Value = 0;
            songBar.Value = 0;

            songName.Text = " ";
            toolStripStatusLabel1.Text = " ";
            songStatus.Text = "No song playing";
        }

        private async Task UpdateProgressBarAsync()
        {
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                progressBar.Value = (int)audioFileReader.CurrentTime.TotalSeconds;
                songBar.Value = (int)audioFileReader.CurrentTime.TotalSeconds;

                UpdateStatusLabel();

                // Delay to avoid high CPU usage
                await Task.Delay(100);
            }

            if (audioFileReader.CurrentTime >= audioFileReader.TotalTime)
            {
                waveOut.Stop();
            }
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            waveOut.Stop();
            songStatus.Text = "Paused";
        }

        private void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            waveOut.Volume = volumeSlider1.Volume;
        }

        private void songBar_Scroll(object sender, EventArgs e)
        {
            audioFileReader.CurrentTime = TimeSpan.FromSeconds(songBar.Value);
            UpdateStatusLabel();
        }

        private void openLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LinkForm linkForm = new LinkForm(this);
            youtubePlay = true;
            linkForm.Show();

        }

        public async void PlayYoutube()
        {

            if (youtubePlay)
            {

                try
                {
                    var youtube = new YoutubeClient();
                    var video = await youtube.Videos.GetAsync(videoUrl);
                    var title = video.Title;
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                    var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                    var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
                    string tempFilePath = Path.GetTempFileName();
                    using (FileStream fileStream = File.OpenWrite(tempFilePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    audioFileReader = new AudioFileReader(tempFilePath);
                    playedSongs.Add(new SongInfo { Title = title, FilePath = tempFilePath });

                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = new WaveOutEvent();

                    waveOut.Init(audioFileReader);

                    songName.Text = video.Title;
                    songName.Font = new Font(songName.Font, FontStyle.Bold);
                    progressBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                    songBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                    UpdateStatusLabel();

                    waveOut.Play();
                    songStatus.Text = "Playing";

                    UpdateHistory();
                    await UpdateProgressBarAsync();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void UpdateHistory()
        {
            historyListbox.Items.Clear();
            foreach (var song in playedSongs)
            {
                historyListbox.Items.Add(song);
            }
        }

        private void UpdateStatusLabel()
        {
            toolStripStatusLabel1.Text = $"{audioFileReader.CurrentTime:mm\\:ss} / {audioFileReader.TotalTime:mm\\:ss}";
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            historyListbox.Visible = true;
        }

        private void historyListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (historyListbox.SelectedItem != null)
                {
                    SongInfo selectedSong = (SongInfo)historyListbox.SelectedItem;
                    audioFileReader = new AudioFileReader(selectedSong.FilePath);

                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = new WaveOutEvent();

                    waveOut.Init(audioFileReader);

                    songName.Text = selectedSong.Title;
                    songName.Font = new Font(songName.Font, FontStyle.Bold);
                    progressBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                    songBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                    UpdateStatusLabel();
                    songStatus.Text = "Playing";

                    waveOut.Play();

                    UpdateHistory();
                    UpdateProgressBarAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void Form1_Click(object sender, EventArgs e)
        {
            historyListbox.Visible = false;
            playlistListbox.Visible = false;
        }

        private void addToPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (audioFileReader != null)
                {
   
                    SongInfo currentSong = new SongInfo { Title = Path.GetFileNameWithoutExtension(songName.Text), FilePath = audioFileReader.FileName };

  
                    playlistSongs.Add(currentSong);


                    playlistListbox.Items.Add(currentSong);
                }
                else
                {
                    MessageBox.Show("No song is currently playing.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void playlistListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (playlistListbox.SelectedItem != null)
                {
                    SongInfo selectedSong = (SongInfo)playlistListbox.SelectedItem;

                    playlistSongs.Remove(selectedSong);

                    playlistListbox.Items.Remove(selectedSong);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void editPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playlistListbox.Visible = true;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private async void openPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (playlistSongs.Count > 0)
                {
                    foreach (var song in playlistSongs)
                    {
                        await PlaySongFromPlaylist(song);
                    }
                }
                else
                {
                    MessageBox.Show("Autoplay is not enabled or the playlist is empty.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private async Task PlaySongFromPlaylist(SongInfo song)
        {
            try
            {
                string filePath = song.FilePath;

                audioFileReader = new AudioFileReader(filePath);

                waveOut?.Stop();
                waveOut?.Dispose();

                waveOut = new WaveOutEvent();


                waveOut.Init(audioFileReader);

                songName.Text = song.Title;
                songName.Font = new Font(songName.Font, FontStyle.Bold);
                progressBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                songBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                songStatus.Text = "Playing";

                UpdateStatusLabel();

                waveOut.Play();

                await UpdateProgressBarAsync();

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ApplyControlColors(Control control, Color color)
        {
            control.ForeColor = color;

            foreach (Control childControl in control.Controls)
            {
                ApplyControlColors(childControl, color);
            }
        }

        private void lightDarkThemeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            lightDarkThemeToolStripMenuItem.Checked = !lightDarkThemeToolStripMenuItem.Checked;

            if (lightDarkThemeToolStripMenuItem.Checked)
            {
                this.BackColor = Color.Black;
                menuStrip1.BackColor = Color.Black;
                songBar.BackColor = Color.Black;
                button1.BackColor = Color.Gray;
                pauseButton.BackColor = Color.Gray;
                statusStrip1.BackColor = Color.Black;
                ApplyControlColors(this, Color.Green);
            }
            else
            {
                this.BackColor = SystemColors.Control; 
                menuStrip1.BackColor = SystemColors.Control;
                songBar.BackColor = SystemColors.Control;
                button1.BackColor = SystemColors.Control;
                pauseButton.BackColor = SystemColors.Control;
                statusStrip1.BackColor = SystemColors.Control;
                ApplyControlColors(this, SystemColors.ControlText);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePlaylist();
        }

        private void SavePlaylist()
        {
            try
            {
                using (var writer = new StreamWriter(playlistFilePath))
                {
                    foreach (var song in playlistSongs)
                    {
                        writer.WriteLine($"{song.Title},{song.FilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save playlist: {ex.Message}");
            }
        }

        private void LoadPlaylist()
        {
            try
            {
                if (File.Exists(playlistFilePath))
                {
                    playlistSongs.Clear(); 

                    using (var reader = new StreamReader(playlistFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 2)
                            {
                                playlistSongs.Add(new SongInfo { Title = parts[0], FilePath = parts[1] });
                            }
                        }
                    }

                    UpdatePlaylistListBox(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load playlist: {ex.Message}");
            }
        }

        private void UpdatePlaylistListBox()
        {
            playlistListbox.Items.Clear();
            foreach (var song in playlistSongs)
            {
                playlistListbox.Items.Add(song);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool isImage1Displayed = true;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (isImage1Displayed)
            {
                pictureBox1.Image = Properties.Resources.a2114423672_10; 
            }
            else
            {
                pictureBox1.Image = Properties.Resources.aaa8;
            }

            isImage1Displayed = !isImage1Displayed;
        }
    }
}
