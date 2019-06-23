using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using IgnoreFiles.Models;
using Microsoft.VisualStudio.Language.Intellisense;

namespace IgnoreFiles.Controls
{
    public partial class IgnoreTree : IInteractiveQuickInfoContent
    {
        private readonly Action _closeAction;

        public IgnoreTree()
        {
            this.InitializeComponent();
            this.ShouldBeThemed();
        }

        public IgnoreTree(string directory, string pattern, Action closeAction)
            : this()
        {
            this._closeAction = closeAction;
            this.ViewModel = new IgnoreTreeModel(directory, pattern);
            this.CloseCommand = ActionCommand.Create(this._closeAction);
            this.ToggleShowAllFilesCommand = ActionCommand.Create(() => this.ViewModel.ShowAllFiles = !this.ViewModel.ShowAllFiles);
            this.ToggleSyncCommand = ActionCommand.Create(() => this.ViewModel.SyncToSolutionExplorer = !this.ViewModel.SyncToSolutionExplorer);
        }

        public IgnoreTreeModel ViewModel
        {
            get { return this.Dispatcher.Invoke(() => this.DataContext as IgnoreTreeModel); }
            set { this.Dispatcher.Invoke(() => this.DataContext = value); }
        }

        public bool KeepQuickInfoOpen => this.IsMouseOverAggregated || this.IsKeyboardFocusWithin || this.IsKeyboardFocused || this.IsFocused;

        public bool IsMouseOverAggregated => this.IsMouseOver || this.IsMouseDirectlyOver;

        private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.ViewModel.SyncToSolutionExplorer)
            {
                return;
            }

            FileTreeModel selected = e.NewValue as FileTreeModel;

            if (selected != null)
            {
                UIHierarchy solutionExplorer = (UIHierarchy) IgnorePackage.DTE.Windows.Item(Constants.vsext_wk_SProjectWindow).Object;
                UIHierarchyItem rootNode = solutionExplorer.UIHierarchyItems.Item(1);

                Stack<Tuple<UIHierarchyItems, int, bool>> parents = new Stack<Tuple<UIHierarchyItems, int, bool>>();
                ProjectItem targetItem = IgnorePackage.DTE.Solution.FindProjectItem(selected.FullPath);

                if (targetItem == null)
                {
                    return;
                }

                UIHierarchyItems collection = rootNode.UIHierarchyItems;
                int cursor = 1;
                bool oldExpand = collection.Expanded;

                while (cursor <= collection.Count || parents.Count > 0)
                {
                    while (cursor > collection.Count && parents.Count > 0)
                    {
                        collection.Expanded = oldExpand;
                        Tuple<UIHierarchyItems, int, bool> parent = parents.Pop();
                        collection = parent.Item1;
                        cursor = parent.Item2;
                        oldExpand = parent.Item3;
                    }

                    if (cursor > collection.Count)
                    {
                        break;
                    }

                    UIHierarchyItem result = collection.Item(cursor);
                    ProjectItem item = result.Object as ProjectItem;

                    if (item == targetItem)
                    {
                        result.Select(vsUISelectionType.vsUISelectionTypeSelect);
                        return;
                    }

                    ++cursor;

                    bool oldOldExpand = oldExpand;
                    oldExpand = result.UIHierarchyItems.Expanded;
                    result.UIHierarchyItems.Expanded = true;
                    if (result.UIHierarchyItems.Count > 0)
                    {
                        parents.Push(Tuple.Create(collection, cursor, oldOldExpand));
                        collection = result.UIHierarchyItems;
                        cursor = 1;
                    }
                }
            }
        }

        private void ItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            FileTreeModel model = item?.DataContext as FileTreeModel;
            model?.ItemDoubleClicked(sender, e);
            this._closeAction?.Invoke();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this._closeAction?.Invoke();
                e.Handled = true;
            }
        }

        public ICommand CloseCommand { get; }

        public ICommand ToggleShowAllFilesCommand { get; }

        public ICommand ToggleSyncCommand { get; }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }
    }
}
