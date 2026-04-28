using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundPanel
{
    public partial class MediaEditMenu : Form
    {
        public MediaEditMenu()
        {
            InitializeComponent();
        }

        private void customButton1_Click(object sender, EventArgs e)
        {
            EditSoundForm esf = new EditSoundForm();
            esf.Show();
        }

        private void customButton2_Click(object sender, EventArgs e)
        {
            // LibVLC kontrolü için CheckVLCVideoForm'u aç
            CheckVLCVideoForm checkForm = new CheckVLCVideoForm();
            checkForm.Show();
        }
    }
}
