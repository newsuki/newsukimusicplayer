using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;

namespace MusicPlayer_AK3TRB
{
    public partial class LinkForm : Form
    {
        public string link { get; private set; }
        public Form1 ogForm;
        public LinkForm(Form1 originalForm)
        {
            InitializeComponent();
            ogForm = originalForm;
        }

        private void YouTubeLinkForm_Load(object sender, EventArgs e)
        {
            ogForm.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                link = textBox1.Text;
                ogForm.videoUrl = link;
                ogForm.Enabled = true;
                ogForm.PlayYoutube();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("The input must be a YouTube link!");
            }
        }

        private void YouTubeLinkForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ogForm.Enabled = true;
        }
    }
}
