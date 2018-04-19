﻿using System.Windows;
using NationalInstruments.Composition;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.ProjectExplorer.Design;
using NationalInstruments.Shell;
using NationalInstruments.SourceModel.Envoys;
using System.ComponentModel.Composition;
using ViewpointSystems.Svn.Plugin.SubMenu;

namespace ViewpointSystems.Svn.Plugin.Commit
{    
    public class CommitCommand 
    {
        [Import]
        public ICompositionHost Host { get; set; }

        public static readonly ICommandEx ShellSelectionRelayCommand = new ShellRelayCommand(Commit, CanCommit)
        {
            UniqueId = "ViewpointSystems.Svn.Plugin.Commint.ShellSelectionRelayCommand",
            LabelTitle = "Commit",

            // this will inform the system that this command should be parented under the given command in a popup menu
            PopupMenuParent = SvnCommands.SvnSubMenuCommand
        };

        public static bool CanCommit(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            return true;
        }

        /// <summary>
        /// SVN Commit
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="host"></param>
        /// <param name="site"></param>
        public static void Commit(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            var filePath = ((Envoy)parameter.Parameter).GetFilePath();
            //moved up here, after ShowDialog() envoy returns null
            var envoy = ((Envoy)parameter.Parameter);
            var projectItem = envoy.GetProjectItemViewModel(site);
            
            //note - no built in service to manage creating view/viewmodels, done by hand
            var commitViewModel = new CommitViewModel();
            commitViewModel.FilePath = filePath;
            var commitView = new CommitView(commitViewModel);
            commitView.Owner = (Window)site.RootVisual;
            commitView.ShowDialog();

            if (commitViewModel.OkButtonClicked)
            {
                var svnManager = host.GetSharedExportedValue<SvnManagerPlugin>();
                var success = svnManager.Commit(filePath, commitViewModel.CommitMessage);
                var debugHost = host.GetSharedExportedValue<IDebugHost>();
                if (success)
                {
                    //if uncommented and done here null ref
                    //var envoy = ((Envoy)parameter.Parameter);
                    //var projectItem = envoy.GetProjectItemViewModel(site);
                    if (null != projectItem)
                    {
                        projectItem.RefreshIcon();
                    }
                   
                    debugHost.LogMessage(new DebugMessage("Viewpoint.Svn", DebugMessageSeverity.Information, $"Commit {filePath}"));
                }
                else
                {
                    debugHost.LogMessage(new DebugMessage("Viewpoint.Svn", DebugMessageSeverity.Error, $"Failed to Commit {filePath}"));
                }
            }            
        }        
    }
}
