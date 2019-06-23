using System;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace IgnoreFiles.Models
{
    public class IgnoreTreeModel : BindableBase
    {
        private bool _showAllFiles;
        private string _searchText;
        private readonly string _pattern;
        private HashSet<FileTreeModel> _visibleNodes;
        private HashSet<FileTreeModel> _filterMatches;
        private string _lastSearchText;
        private bool _lastShowAllFiles;

        public IgnoreTreeModel(string rootDirectory, string pattern)
        {
            this._pattern = pattern.TrimStart('/', '\\', '!');
            this.TreeRoot = FileTree.ForDirectory(rootDirectory);
            this.ShouldBeVisible = this.CheckVisibility;
            this.IsSearchMatch = f => true;

            this.NodeFilter = this.FilterNodes;
            this.FileCount = this.TreeRoot.AllFiles.Count(x => x.IsFile && this.ShouldBeVisible(x));
            this.SearchIcon = WpfUtil.GetIconForImageMoniker(KnownMonikers.Search, 16, 16);
            this.ShowAllFilesIcon = WpfUtil.GetIconForImageMoniker(KnownMonikers.ShowAllFiles, 16, 16);
            this.SyncToSolutionExplorerIcon = WpfUtil.GetIconForImageMoniker(KnownMonikers.Sync, 16, 16);
        }

        public ImageSource SyncToSolutionExplorerIcon { get; }

        private bool CheckVisibility(FileTreeModel arg)
        {
            this.EnsureFilterRun();
            return this._visibleNodes.Contains(arg);
        }

        private bool FilterNodes(FileTreeModel arg)
        {
            this.EnsureFilterRun();
            return this._filterMatches.Contains(arg);
        }

        private void EnsureFilterRun()
        {
            if (string.Equals(this._lastSearchText, this._searchText) && this._lastShowAllFiles == this._showAllFiles && this._visibleNodes != null)
            {
                return;
            }

            string searchText = this._searchText;
            bool showAllFiles = this._showAllFiles;
            HashSet<FileTreeModel> visibleNodes = new HashSet<FileTreeModel>();
            HashSet<FileTreeModel> filterMatches = new HashSet<FileTreeModel>();
            HashSet<FileTreeModel> directGlobMatches = new HashSet<FileTreeModel>();

            foreach (FileTreeModel model in this.TreeRoot.AllFiles)
            {
                if (visibleNodes.Contains(model))
                {
                    continue;
                }

                bool globMatch = Helpers.CheckGlobbing(model.FullPath, this._pattern);

                if (globMatch)
                {
                    directGlobMatches.Add(model);
                    filterMatches.Add(model);
                    FileTreeModel parent = model.Parent;

                    while (parent != null)
                    {
                        //If something else has already added our parent, it's already added the whole parent tree, bail.
                        if (!filterMatches.Add(parent))
                        {
                            break;
                        }

                        parent = parent.Parent;
                    }
                }
                else
                {
                    FileTreeModel parent = model.Parent;
                    while (parent != null)
                    {
                        //If we're included by a direct glob match (even though this model isn't one itself)...
                        if (directGlobMatches.Contains(parent))
                        {
                            filterMatches.Add(model);
                            globMatch = true;
                            parent = model.Parent;

                            while (parent != null)
                            {
                                //If something else has already added our parent, it's already added the whole parent tree, bail.
                                if (!filterMatches.Add(parent))
                                {
                                    break;
                                }

                                parent = parent.Parent;
                            }

                            break;
                        }

                        parent = parent.Parent;
                    }
                }


                bool isVisible = showAllFiles || globMatch;
                bool explicitMatch = globMatch;

                if (!string.IsNullOrEmpty(searchText))
                {
                    bool searchMatch = model.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
                    explicitMatch |= searchMatch;
                    isVisible &= searchMatch;
                }

                model.IsExpanded = globMatch;

                if (isVisible)
                {
                    visibleNodes.Add(model);

                    FileTreeModel parent = model.Parent;

                    while (parent != null)
                    {
                        //If something else has already added our parent, it's already added the whole parent tree, bail.
                        if (!visibleNodes.Add(parent) && (!explicitMatch || parent.IsExpanded))
                        {
                            break;
                        }

                        if (explicitMatch)
                        {
                            parent.IsExpanded = true;
                        }

                        parent = parent.Parent;
                    }
                }
            }

            if (string.Equals(searchText, this._searchText) && showAllFiles == this._showAllFiles)
            {
                this._filterMatches = filterMatches;
                this._visibleNodes = visibleNodes;
                this._lastShowAllFiles = showAllFiles;
                this._lastSearchText = searchText;
            }
        }

        public Func<FileTreeModel, bool> NodeFilter { get; }

        public FileTree TreeRoot { get; }

        public Func<FileTreeModel, bool> ShouldBeVisible { get; }

        public bool ShowAllFiles
        {
            get { return this._showAllFiles; }
            set { this.Set(ref this._showAllFiles, value); }
        }

        public int FileCount { get; }

        public string SearchText
        {
            get { return this._searchText; }
            set { this.Set(ref this._searchText, value, StringComparer.Ordinal); }
        }

        public ImageSource SearchIcon { get; }

        public ImageSource ShowAllFilesIcon { get; }

        public Func<FileTreeModel, bool> IsSearchMatch { get; }

        public bool SyncToSolutionExplorer
        {
            get
            {
                return IgnorePackage.Options.SyncToSolutionExplorer;
            }
            set
            {
                IgnorePackage.Options.SyncToSolutionExplorer = value;
                IgnorePackage.Options.SaveSettingsToStorage();
            }
        }
    }
}
