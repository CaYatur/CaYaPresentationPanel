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
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            ModernTrackBar trk = new ModernTrackBar();
            trk.Location = new Point(20, 20);
            trk.Width = 300;
            trk.Value = 50;
            this.Controls.Add(trk);
        }

        private void modernTrackBar1_Load(object sender, EventArgs e)
        {

        }
    }
}
