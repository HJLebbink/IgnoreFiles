using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IgnoreFiles.Models
{
    public class FileTree
    {
        private readonly string _rootDir;
        private List<FileTreeModel> _children = new List<FileTreeModel>();
        private readonly Dictionary<string, FileTreeModel> _lookup = new Dictionary<string, FileTreeModel>();

        public static IReadOnlyList<string> DirectoryIgnoreList { get; } = new List<string>
        {
            ".git"
        };

        public FileTree(string rootDirectory)
        {
            this._rootDir = rootDirectory;
        }

        public IEnumerable<FileTreeModel> AllFiles => this._lookup.Values;

        public IReadOnlyList<FileTreeModel> Children => this._children;

        public static FileTree ForDirectory(string rootDirectory)
        {
            FileTree root = new FileTree(rootDirectory);

            foreach (string file in Directory.EnumerateFileSystemEntries(rootDirectory, "*", SearchOption.AllDirectories))
            {
                string[] parts = file.Split('/', '\\');
                bool skip = false;
                bool isFile = File.Exists(file);
                int fileNameParts = isFile ? 1 : 0;

                for (int i = 1; !skip && i < parts.Length - fileNameParts; ++i)
                {
                    if (DirectoryIgnoreList.Contains(parts[i], StringComparer.OrdinalIgnoreCase))
                    {
                        skip = true;
                    }
                }

                if (!skip)
                {
                    root.GetModelFor(file, isFile);
                }
            }

            root.SortChildren();
            return root;
        }

        private void SortChildren()
        {
            this._children = this._children.OrderBy(x => x.IsFile).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (FileTreeModel child in this._children.Where(x => !x.IsFile))
            {
                child.SortChildren();
            }
        }

        public FileTreeModel GetModelFor(string fullPath, bool isFile)
        {
            if (fullPath.Length <= this._rootDir.Length)
            {
                return null;
            }

            FileTreeModel existingModel;
            if (!this._lookup.TryGetValue(fullPath, out existingModel))
            {
                existingModel = new FileTreeModel(fullPath, this, isFile);
                this._lookup[fullPath] = existingModel;

                if (existingModel.Parent == null)
                {
                    this._children.Add(existingModel);
                }
            }

            return existingModel;
        }
    }
}