﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Utilities;

namespace IgnoreFiles
{
    [Export(typeof(IDropHandlerProvider))]
    [DropFormat("CF_VSSTGPROJECTITEMS")]
    [DropFormat("FileDrop")]
    [Name("IgnoreDropHandler")]
    [ContentType(IgnoreContentTypeDefinition.IgnoreContentType)]
    [Order(Before = "DefaultFileDropHandler")]
    internal class IgnoreDropHandlerProvider : IDropHandlerProvider
    {
        [Import]
        ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public IDropHandler GetAssociatedDropHandler(IWpfTextView view)
        {
            ITextDocument document;

            if (this.TextDocumentFactoryService.TryGetTextDocument(view.TextBuffer, out document))
            {
                return view.Properties.GetOrCreateSingletonProperty(() => new IgnoreDropHandler(view, document.FilePath));
            }

            return null;
        }
    }
}