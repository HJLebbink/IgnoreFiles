using System.Threading;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Text;

namespace IgnoreFiles
{
    class RemoveNonMatchSuggestedAction : BaseSuggestedAction
    {
        private SnapshotSpan _span;

        public RemoveNonMatchSuggestedAction(SnapshotSpan span)
        {
            this._span = span;
        }

        public override string DisplayText
        {
            get { return "Remove non-matching entry"; }
        }

        public override void Invoke(CancellationToken cancellationToken)
        {
            var line = this._span.Snapshot.GetLineFromPosition(this._span.Start.Position);

            using (var edit = this._span.Snapshot.TextBuffer.CreateEdit())
            {
                edit.Delete(line.Start.Position, line.LengthIncludingLineBreak);
                edit.Apply();
            }
        }
    }
}
