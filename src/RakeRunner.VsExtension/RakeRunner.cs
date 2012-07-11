using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using RakeRunner.Library.Exceptions;
using RakeRunner.Library.Models;
using RakeRunner.Library.Services;

namespace RakeRunner
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
    // add these 2 Annotations to execute Initialize() immediately when a project is loaded
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [Guid(GuidList.guidRakeRunnerPkgString)]
    public class RakeRunner : 
        Package,
        IVsSolutionEvents3,
        IVsFileChangeEvents,
        IVsSelectionEvents,
        IDisposable
    {
        private DTE _dte;
        private RakeService rakeService = null;
        private uint _vsSolutionEventsCookie, _vsIVsFileChangeEventsCookie, _vsMonitorCookie;

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


            rakeService = new RakeService();
            
            this._dte = (DTE)this.GetService(typeof(DTE));

            IVsMonitorSelection ms = this.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (ms != null)
                ms.AdviseSelectionEvents(this, out _vsMonitorCookie);

            var solution = this.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution != null)
                solution.AdviseSolutionEvents(this, out _vsSolutionEventsCookie);


            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                //the main menu
                CommandID command = new CommandID(GuidList.guidRakeRunnerCmdSet, (int)PkgCmdIDList.icmdCommandRakeMenu);
                var menu = new OleMenuCommand(null, command);
                mcs.AddCommand(menu);
                
            }
        }

        #endregion



        #region Implementation of IVsSolutionEvents

        public int OnAfterOpenSolution([InAttribute] Object pUnkReserved, [InAttribute] int fNewSolution)
        {
            monitorFileChange();
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution([InAttribute] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([InAttribute] IVsHierarchy pStubHierarchy, [InAttribute] IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fAdded)
        {

            
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution([InAttribute] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject([InAttribute] IVsHierarchy pHierarchy, [InAttribute] int fRemoving, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution([InAttribute] Object pUnkReserved, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject([InAttribute] IVsHierarchy pRealHierarchy, [InAttribute] ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution([InAttribute] Object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fired before opening all nested projects owned by a parent hierarchy.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pHierarchy">[in] Pointer to parent project.</param>
        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fired after opening all nested projects owned by a parent hierarchy.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pHierarchy">[in] Pointer to parent project.</param>
        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fired before closing all nested projects owned by a parent hierarchy.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pHierarchy">[in] Pointer to parent project.</param>
        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fired after closing all nested projects owned by a parent hierarchy.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pHierarchy">[in] Pointer to parent project.</param>
        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            unmonitorFileChange();

            // Unregister from receiving solution events
            if (VSConstants.VSCOOKIE_NIL != _vsSolutionEventsCookie)
            {
                IVsSolution sol = this.GetService(typeof(SVsSolution)) as IVsSolution;
                if (sol != null)
                {
                    sol.UnadviseSolutionEvents(_vsSolutionEventsCookie);
                    _vsSolutionEventsCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
        }

        #endregion

        #region Files and Directories

        /// <summary>
        /// Notifies clients of changes made to one or more files.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="cChanges">[in] Number of files changed.</param><param name="rgpszFile">[in, size_is(cChanges)] Array of file names.</param><param name="rggrfChange">[in, size_is(cChanges)] Array of flags indicating the type of changes. See <see cref="T:Microsoft.VisualStudio.Shell.Interop._VSFILECHANGEFLAGS"/>.</param>
        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notifies clients of changes made to a directory.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pszDirectory">[in] Name of the directory that had a change.</param>
        public int DirectoryChanged(string pszDirectory)
        {
            return VSConstants.S_OK;
        }


        private void monitorFileChange()
        {
            var solutionFileName = getSolutionFileName();

            if (!string.IsNullOrEmpty(solutionFileName))
            {
                var monitorFolder = Path.GetDirectoryName(solutionFileName);

                IVsFileChangeEx fileChangeService = this.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
                if (VSConstants.VSCOOKIE_NIL != _vsIVsFileChangeEventsCookie)
                {
                    fileChangeService.UnadviseFileChange(_vsIVsFileChangeEventsCookie);
                }
                fileChangeService.AdviseFileChange(monitorFolder, 1, this, out _vsIVsFileChangeEventsCookie);
                Debug.WriteLine("==== Monitoring: " + monitorFolder + " " + _vsIVsFileChangeEventsCookie);

            }
        }

        private void unmonitorFileChange()
        {
            if (VSConstants.VSCOOKIE_NIL != _vsIVsFileChangeEventsCookie)
            {
                IVsFileChangeEx fileChangeService = this.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
                fileChangeService.UnadviseDirChange(_vsIVsFileChangeEventsCookie);
                Debug.WriteLine("==== Stop Monitoring: " + _vsIVsFileChangeEventsCookie.ToString());
                _vsIVsFileChangeEventsCookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        /// <summary>
        /// Returns the filename of the solution
        /// </summary>
        private string getSolutionFileName()
        {
            IVsSolution sol = (IVsSolution)this.GetService(typeof(SVsSolution));
            
            string solutionDirectory, solutionFile, solutionUserOptions;
            if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
            {
                return solutionFile;
            }
            else
            {
                return null;
            }
        }
        #endregion


        #region Implementation of IVsSelectionEvents

        /// <summary>
        /// Reports that the project hierarchy, item and/or selection container has changed.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pHierOld">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsHierarchy"/> interface of the project hierarchy for the previous selection.</param><param name="itemidOld">[in] Identifier of the project item for previous selection. For valid <paramref name="itemidOld"/> values, see VSITEMID.</param><param name="pMISOld">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsMultiItemSelect"/> interface to access a previous multiple selection.</param><param name="pSCOld">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.ISelectionContainer"/> interface to access Properties window data for the previous selection.</param><param name="pHierNew">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsHierarchy"/> interface of the project hierarchy for the current selection.</param><param name="itemidNew">[in] Identifier of the project item for the current selection. For valid <paramref name="itemidNew"/> values, see VSITEMID.</param><param name="pMISNew">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsMultiItemSelect"/> interface for the current selection.</param><param name="pSCNew">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.ISelectionContainer"/> interface for the current selection.</param>
        public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            string fullPath = "";
            if(pHierNew != null)
            {   
                // Get the full path
                pHierNew.GetCanonicalName(itemidNew, out fullPath);
            }
            //if path is null, get it from solution level
            if (string.IsNullOrEmpty(fullPath))
            {
                fullPath = getSolutionFileName();
            }
            var directory = Path.GetDirectoryName(fullPath);
            //set the current dir var so we can use it in other parts of the service
            currentDir = directory;
            // Setup the rake menu for this path
            setupRakeTasksMenu(directory);
            return VSConstants.S_OK; 
        }

        /// <summary>
        /// Reports that an element value has changed.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="elementid">[in] DWORD value representing a particular entry in the array of element values associated with the selection context. For valid <paramref name="elementid"/> values, see <see cref="T:Microsoft.VisualStudio.VSConstants.VSSELELEMID"/>.</param><param name="varValueOld">[in] VARIANT that contains the previous element value. This parameter contains element-specific data, such as a pointer to the <see cref="T:Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget"/> interface if <paramref name="elementid"/> is set to SEID_ResultsList or a pointer to the <see cref="T:Microsoft.VisualStudio.OLE.Interop.IOleUndoManager"/> interface if <paramref name="elementid"/> is set to SEID_UndoManager.</param><param name="varValueNew">[in] VARIANT that contains the new element value. This parameter contains element-specific data, such as a pointer to the IOleCommandTarget interface if <paramref name="elementid"/> is set to SEID_ResultsList or a pointer to the IOleUndoManager interface if <paramref name="elementid"/> is set to SEID_UndoManager.</param>
        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reports that the command UI context has changed.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="dwCmdUICookie">[in] DWORD representation of the GUID identifying the command UI context passed in as the <paramref name="rguidCmdUI"/> parameter in the call to <see cref="M:Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.GetCmdUIContextCookie(System.Guid@,System.UInt32@)"/>.</param><param name="fActive">[in] Flag that is set to true if the command UI context identified by <paramref name="dwCmdUICookie"/> has become active and false if it has become inactive.</param>
        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region Custom Code

        private Dictionary<string, List<RakeTask>>  taskCache = new Dictionary<string, List<RakeTask>>();
        private List<int> currentCommandsInMenu = new List<int>();
        private string currentDir = "";

        private void updateCache(string dir)
        {
            try
            {
                //get the tasks
                var tasks = rakeService.GetRakeTasks(dir);

                //check if the cache exist then replace it or add it
                if (taskCache.ContainsKey(dir))
                {
                    taskCache[dir] = tasks;
                }
                else
                {
                    taskCache.Add(dir, tasks);
                }
            }
            catch
            {
                
            }

        }

        private void setupRakeTasksMenu(string dir)
        {
            try
            {
                List<RakeTask> tasks;
                //update if cache not exist
                if(!taskCache.ContainsKey(dir))
                {
                    updateCache(dir);
                }
                //get the tasks from the cache for the directory
                taskCache.TryGetValue(dir, out tasks);

                OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
               

                // Add our command handlers for menu (commands must exist in the .vsct file)
                if (null != mcs)
                {
                    //if there is any task in menu, remove them all.
                    foreach (var command in currentCommandsInMenu)
                    {
                        var cmdID = new CommandID(
                        GuidList.guidRakeRunnerCmdSet, command);
                        var menu = mcs.FindCommand(cmdID);
                        if (menu != null)
                            mcs.RemoveCommand(menu);
                        
                    }
                    //clear the commands list
                    currentCommandsInMenu.Clear();

                    if (tasks != null)
                    {
                        //add the tasks
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            int commandId = (int) PkgCmdIDList.icmdTasksList + i;
                            var cmdID = new CommandID(
                                GuidList.guidRakeRunnerCmdSet, commandId);
                            var mc = new OleMenuCommand(RakeTaskSelected, cmdID);
                            mc.Text = tasks[i].Task;
                            mcs.AddCommand(mc);
                            //store the commandid so we can remove when re-creating the menu
                            currentCommandsInMenu.Add(commandId);
                        }
                    }
                    //var tasks = rakeService.GetRakeTasks()
                }
            }
            catch
            {
                
            }
        }

        private void RakeTaskSelected(object sender, EventArgs eventArgs)
        {
            var menuCommand = sender as OleMenuCommand;
            if (null != menuCommand)
            {
                int taskIndex = menuCommand.CommandID.ID - (int)PkgCmdIDList.icmdTasksList;

                if (taskIndex >= 0 && taskIndex < taskCache[currentDir].Count)
                {
                    var selection = taskCache[currentDir][taskIndex];
                    
                    System.Windows.Forms.MessageBox.Show(
                        string.Format(CultureInfo.CurrentCulture,
                                      "Selected {0}", selection.Task));
                    runRakeTask(selection, currentDir);
                }
            }
        }

        private void runRakeTask(RakeTask task, string dir)
        {
            rakeService.RunRakeTask(dir, task.Task);
        }

        private void WriteOutputWindow(string outputText)
        {
            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // If we fail to get it we can exit now. 
            if (null == outputWindow)
            {
                Trace.WriteLine("Failed to get a reference to IVsOutputWindow");
                return;
            }
            // Now get the window pane for the general output. 
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane;
            IVsOutputWindowPane windowPane = null;
            if (Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)))
            {
                Trace.WriteLine("Failed to get a reference to the Output window General pane");
                return;
            }
            if (Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.OutputString(outputText)))
            {
                Trace.WriteLine("Failed to write on the Output window");
            }
        } 
        #endregion
    }
}
