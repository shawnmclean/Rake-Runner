using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RakeRunner.VsExtension
{
    public partial class OptionsControl : UserControl
    {
        internal OptionPage OptionPage;
        public OptionsControl()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            Txt_RakePath.Text = OptionPage.RakePath;
        }

        private void btn_BrowseRakeFile_Click(object sender, EventArgs e)
        {
            if (FileDiag_RakePath.ShowDialog() == DialogResult.OK)
            {
                Txt_RakePath.Text = FileDiag_RakePath.FileName;
            }
        }

        private void Txt_RakePath_TextChanged(object sender, EventArgs e)
        {
            OptionPage.RakePath = Txt_RakePath.Text;
        }
    }
}
