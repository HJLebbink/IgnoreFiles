using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace IgnoreFiles
{
    internal class IgnoreQuickInfoController : IIntellisenseController
    {
        private ITextView m_textView;
        private IList<ITextBuffer> m_subjectBuffers;
        private IgnoreQuickInfoControllerProvider m_provider;

        internal IgnoreQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, IgnoreQuickInfoControllerProvider provider)
        {
            this.m_textView = textView;
            this.m_subjectBuffers = subjectBuffers;
            this.m_provider = provider;

            this.m_textView.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = this.m_textView.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(this.m_textView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => this.m_subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

                if (!this.m_provider.QuickInfoBroker.IsQuickInfoActive(this.m_textView))
                {
                    this.m_provider.QuickInfoBroker.TriggerQuickInfo(this.m_textView, triggerPoint, true);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (this.m_textView == textView)
            {
                this.m_textView.MouseHover -= this.OnTextViewMouseHover;
                this.m_textView = null;
            }
        }


        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
