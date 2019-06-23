using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace IgnoreFiles.Models
{
    public class FileTreeModel : BindableBase
    {
        private bool _isExpanded;
        private string _name;
        private List<FileTreeModel> _children;

        public FileTreeModel(string fullPath, FileTree root, bool isFile)
        {
            this.IsExpanded = true;
            this._children = new List<FileTreeModel>();
            this.Root = root;
            this.Name = Path.GetFileName(fullPath);
            this.IsFile = isFile;
            this.FullPath = fullPath;
            int lastSlash = fullPath.LastIndexOfAny(new[] { '/', '\\' });

            //Normally this would be -1, but if the path starts with / or \, we don't want to make an empty entry
            if(lastSlash > 0)
            {
                string parentFullPath = fullPath.Substring(0, lastSlash).Trim('/', '\\');

                if(!string.IsNullOrEmpty(parentFullPath))
                {
                    this.Parent = root.GetModelFor(parentFullPath, false);
                    this.Parent?.Children?.Add(this);
                }
            }
        }

        public void ItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (this.IsFile)
            {
                IgnorePackage.DTE.ItemOperations.OpenFile(this.FullPath);
            }
        }

        public ImageSource CachedIcon { get; set; }

        public List<FileTreeModel> Children => this._children;

        public bool IsFile { get; }

        public string Name
        {
            get { return this._name; }
            set { this.Set(ref this._name, value, StringComparer.Ordinal); }
        }

        public FileTreeModel Parent { get; }

        public FileTree Root { get; }

        public string FullPath { get; }

        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set { this.Set(ref this._isExpanded, value); }
        }

        public void SortChildren()
        {
            this._children = this._children.OrderBy(x => x.IsFile).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
