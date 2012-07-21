using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace RakeRunner.VsExtension
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    public class OptionPage : DialogPage
    {
        public string RakePath { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                OptionsControl page = new OptionsControl();
                page.OptionPage = this;
                page.Initialize();
                return page;
            }
        }
    }
}
