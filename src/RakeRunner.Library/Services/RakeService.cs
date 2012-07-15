﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RakeRunner.Library.Exceptions;
using RakeRunner.Library.Models;

namespace RakeRunner.Library.Services
{
    /// <summary>
    ///
    /// </summary>
    public class RakeService
    {
        /// <summary>
        /// callback for whenever the rake process fires an event
        /// </summary>
        private Action<string> rakeOutputCallback;

        private Action<string> rakeErrorOutputCallback;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsRakeInstalled()
        {
            //TODO: Need to know how to check if rake is installed.
            return true;
        }

        /// <summary>
        /// Execute a particular rake task in the rake file found in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="taskName"></param>
        /// <exception cref="InvalidOperationException">Thrown when rake is not installed</exception>
        /// <exception cref="RakeFailedException">Thrown when there was an error executing the rake command</exception>
        public void RunRakeTask(string directory, string taskName, Action<string> outputCallback, Action<string> errorCallback)
        {
            if (IsRakeInstalled())
            {
                //set the callback
                rakeOutputCallback = outputCallback;
                rakeErrorOutputCallback = errorCallback;

                runRakeProcess(directory, taskName, true);

                //remove the callback after task is finished
                rakeOutputCallback = null;
                rakeErrorOutputCallback = null;
            }
            else
            {
                throw new InvalidOperationException("Rake is not installed.");
            }
        }

        /// <summary>
        /// Get a list of rake tasks for the rake file found in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when rake is not installed</exception>
        /// <exception cref="RakeFailedException">Thrown when there was an error executing the rake command</exception>
        public List<RakeTask> GetRakeTasks(string directory)
        {
            if (IsRakeInstalled())
            {
                //-P will get the tasks, no description
                var stringTasks = runRakeProcess(directory, "-P", false);

                //an example of the rake task return is "rake build" or "rake version:minor"
                const string RAKE_WORD = "rake";

                //parsing the strings into list of RakeTask
                return (from stringTask in stringTasks
                        //ensure the line starts with the rake keyword
                        where stringTask.StartsWith(RAKE_WORD, true, CultureInfo.InvariantCulture)
                        select new RakeTask
                        {
                            //the task comes after the rake word and a space
                            Task = stringTask.Substring(RAKE_WORD.Length + 1)
                        }).ToList();
            }
            else
            {
                throw new InvalidOperationException("Rake is not installed.");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="rakeParam"></param>
        /// <returns></returns>
        private List<string> runRakeProcess(string directory, string rakeParam, bool useCallbacks)
        {
            try
            {
                // "/c" tells the command line program to execute the arguments on process start
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c rake " + rakeParam);
                procStartInfo.WorkingDirectory = directory;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;

                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(procStartInfo);
                if (useCallbacks)
                {
                    //event for when data is recieved
                    proc.OutputDataReceived += proc_OutputDataReceived;
                    proc.ErrorDataReceived += proc_ErrorDataReceived;

                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();
                }
                //proc.Start();
                proc.WaitForExit();

                if (useCallbacks)
                    return null;

                if (proc.ExitCode == 0)
                {
                    //split the string by the new lines generated by the rake program
                    return Regex.Split(proc.StandardOutput.ReadToEnd(), "\r\n|\r|\n").ToList();
                }
                else
                {
                    throw new RakeFailedException(proc.StandardError.ReadToEnd());
                }
            }
            catch (Exception objException)
            {
                //TODO: add logging
                throw;
            }
        }

        private void proc_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (rakeErrorOutputCallback != null)
            {
                rakeErrorOutputCallback(e.Data);
            }
        }

        private void proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (rakeOutputCallback != null)
            {
                rakeOutputCallback(e.Data);
            }
        }
    }
}