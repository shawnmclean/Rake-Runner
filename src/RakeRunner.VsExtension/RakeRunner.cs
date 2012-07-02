using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using RakeRunner.Library.Exceptions;
using RakeRunner.Library.Services;

namespace Company.RakeRunner_VsExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidRakeRunnerPkgString)]
    public sealed class RakeRunner : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public RakeRunner()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                //the main menu
                CommandID menuCommandMainMenuId = new CommandID(GuidList.guidRakeRunnerCmdSet, (int)PkgCmdIDList.icmdCommandRakeMenu);
                OleMenuCommand menuMain = new OleMenuCommand(menuAboutClicked, menuCommandMainMenuId);
                menuMain.BeforeQueryStatus += menuMain_BeforeQueryStatus;
                mcs.AddCommand(menuMain);

                // Show About dialog
                CommandID menuCommandAboutId = new CommandID(GuidList.guidRakeRunnerCmdSet, (int)PkgCmdIDList.icmdCommandAbout);
                MenuCommand menuAbout = new MenuCommand(menuAboutClicked, menuCommandAboutId);
                mcs.AddCommand(menuAbout);
            }
        }

        void menuMain_BeforeQueryStatus(object sender, EventArgs e)
        {
                OleMenuCommand menuCommand = sender as OleMenuCommand;
                if (menuCommand != null)
                {
                    var dte = (EnvDTE.DTE) Package.GetGlobalService(typeof (SDTE));
                    string directory = "";
                    //get the solution directory
                    if (dte.Solution != null)
                        directory = System.IO.Path.GetDirectoryName(dte.Solution.FullName);

                    RakeService service = new RakeService();
                    try
                    {
                        var tasks = service.GetRakeTasks(directory);
                        menuCommand.Visible = true;
                    }
                    catch(RakeFailedException rfe)
                    {
                        //if there is an exception in getting rake tasks, dont show the menu
                        menuCommand.Visible = false;
                    }


                    //IntPtr hierarchyPtr, selectionContainerPtr;
                    //uint projectItemId;
                    //IVsMultiItemSelect mis;
                    //IVsMonitorSelection monitorSelection =
                    //    (IVsMonitorSelection) Package.GetGlobalService(typeof (SVsShellMonitorSelection));
                    //monitorSelection.GetCurrentSelection(out hierarchyPtr, out projectItemId, out mis,
                    //                                     out selectionContainerPtr);

                    //IVsHierarchy hierarchy =
                    //    Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof (IVsHierarchy)) as IVsHierarchy;
                    //if (hierarchy != null)
                    //{
                    //    object value;
                    //    hierarchy.GetProperty(projectItemId, (int) __VSHPROPID.VSHPROPID_Name, out value);

                    //    if (value != null && value.ToString().EndsWith(".dbml"))
                    //    {
                    //        menuCommand.Visible = true;
                    //    }
                    //    else
                    //    {
                    //        menuCommand.Visible = false;
                    //    }
                    //}
                }
        }

        private void menuAboutClicked(object sender, EventArgs eventArgs)
        {
            MessageBox.Show("LOL");
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "Rake Runner",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }

    }
}
