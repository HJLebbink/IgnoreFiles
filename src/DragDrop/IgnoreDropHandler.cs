using System;
using System.IO;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace IgnoreFiles
{
    internal class IgnoreDropHandler : IDropHandler
    {
        private IWpfTextView _view;
        private string _draggedFileName;
        private string _documentFileName;

        public IgnoreDropHandler(IWpfTextView view, string fileName)
        {
            this._view = view;
            this._documentFileName = fileName;
        }

        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo)
        {
            var position = dragDropInfo.VirtualBufferPosition.Position;
            var line = this._view.GetTextViewLineContainingBufferPosition(position);
            string text = PackageUtilities.MakeRelative(this._documentFileName, this._draggedFileName)
                                          .Replace("\\", "/");

            // Insert a new line if dragged after existing text
            if (line.Start < position)
                text = Environment.NewLine + text;

            using (var edit = this._view.TextBuffer.CreateEdit())
            {
                edit.Insert(position, text);
                edit.Apply();
            }

            return DragDropPointerEffects.Copy;
        }

        public void HandleDragCanceled()
        { }

        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo)
        {
            return DragDropPointerEffects.All;
        }

        public bool IsDropEnabled(DragDropInfo dragDropInfo)
        {
            this._draggedFileName = GetImageFilename(dragDropInfo);

            return File.Exists(this._draggedFileName) || Directory.Exists(this._draggedFileName);
        }

        private static string GetImageFilename(DragDropInfo info)
        {
            var data = new DataObject(info.Data);

            if (info.Data.GetDataPresent("FileDrop"))
            {
                // The drag and drop operation came from the file system
                var files = data.GetFileDropList();

                if (files != null && files.Count == 1)
                {
                    return files[0];
                }
            }
            else if (info.Data.GetDataPresent("CF_VSSTGPROJECTITEMS"))
            {
                // The drag and drop operation came from the VS solution explorer
                return data.GetText();
            }

            return null;
        }
    }
}