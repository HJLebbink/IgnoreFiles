using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace IgnoreFiles
{
    public class IgnoreClassifier : IClassifier
    {
        private IClassificationType _symbol, _comment, _path, _pathNoMatch;
        private static Regex _commentRegex = new Regex(@"(?<!\\)(#.+)", RegexOptions.Compiled);
        private static Regex _pathRegex = new Regex(@"(?<path>^[^:#\r\n]+)", RegexOptions.Compiled);
        private static Regex _symbolRegex = new Regex(@"^(?<name>syntax)(?::[^#:]+)", RegexOptions.Compiled);
        private ConcurrentDictionary<string, bool> _cache = new ConcurrentDictionary<string, bool>();
        private Queue<Tuple<string, SnapshotSpan>> _queue = new Queue<Tuple<string, SnapshotSpan>>();
        private string _root;
        private ITextBuffer _buffer;
        private bool _isResetting, _isAsynchronous = true;
        private Timer _timer;

        public IgnoreClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer, string fileName)
        {
            this._buffer = buffer;
            this._root = Path.GetDirectoryName(fileName);
            this._comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            this._path = registry.GetClassificationType(IgnoreClassificationTypes.Path);
            this._pathNoMatch = registry.GetClassificationType(IgnoreClassificationTypes.PathNoMatch);
            this._symbol = registry.GetClassificationType(IgnoreClassificationTypes.Keyword);

            this._timer = new Timer(250);
            this._timer.Elapsed += this.TimerElapsed;
        }

        public bool HasMatches(SnapshotSpan span)
        {
            try
            {
                this._isAsynchronous = false;
                return this.GetClassificationSpans(span).Any(t => t.ClassificationType.IsOfType(IgnoreClassificationTypes.PathNoMatch));
            }
            finally { this._isAsynchronous = true; }
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> list = new List<ClassificationSpan>();

            string text = span.GetText();

            if (string.IsNullOrWhiteSpace(text))
                return list;

            var comment = _commentRegex.Match(text);

            if (comment.Success)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + comment.Index, comment.Length);
                list.Add(new ClassificationSpan(result, this._comment));

                // Whole line is a comment, so just return here
                if (comment.Index == 0)
                    return list;
            }

            var symbolMatch = _symbolRegex.Match(text);
            if (symbolMatch.Success)
            {
                var keyword = this.GetSpan(span, symbolMatch.Groups["name"], this._symbol);
                list.Add(keyword);

                // Whole line is a symbol, so just return here
                return list;
            }

            var pathMatch = _pathRegex.Match(text);

            if (!pathMatch.Success)
                return list;

            var pathType = this.GetPathClassificationType(pathMatch.Groups["path"].Value.Trim(), span);

            var path = this.GetSpan(span, pathMatch.Groups["path"], pathType);
            if (path != null)
                list.Add(path);

            return list;
        }

        public void Reset()
        {
            if (!this._isResetting)
            {
                this._isResetting = true;
                this._cache.Clear();
                this._queue.Clear();
                var span = new SnapshotSpan(this._buffer.CurrentSnapshot, 0, this._buffer.CurrentSnapshot.Length);
                this.OnClassificationChanged(span);
                this._isResetting = false;
            }
        }

        private IClassificationType GetPathClassificationType(string pattern, SnapshotSpan span)
        {
            if (pattern.StartsWith("../"))
                return this._pathNoMatch;

            if (!this._cache.ContainsKey(pattern))
            {
                if (this._isAsynchronous)
                {
                    this._queue.Enqueue(Tuple.Create(pattern, span));
                    this._timer.Start();
                }
                else
                {
                    this.ProcessPath(pattern, span);
                    return this.GetPathClassificationType(pattern, span);
                }

                return this._path;
            }

            return this._cache[pattern] ? this._path : this._pathNoMatch;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this._queue.Count == 0)
                return;

            this._timer.Stop();

            Task.Run(() =>
            {
                try
                {
                    do
                    {
                        var t = this._queue.Dequeue();

                        if (this._buffer.CurrentSnapshot.Version == t.Item2.Snapshot.Version)
                            this.ProcessPath(t.Item1, t.Item2);

                    } while (this._queue.Count > 0);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        private void ProcessPath(string pattern, SnapshotSpan span)
        {
            bool hasFiles = IgnoreQuickInfo.GetFiles(this._root, pattern).Any();

            this._cache[pattern] = hasFiles;

            if (!hasFiles)
            {
                this.OnClassificationChanged(span);
            }
        }

        private void OnClassificationChanged(SnapshotSpan span)
        {
            if (this._buffer.CurrentSnapshot.Version == span.Snapshot.Version)
                ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(span));
        }

        private ClassificationSpan GetSpan(SnapshotSpan span, Group group, IClassificationType type)
        {
            if (group.Length > 0)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + group.Index, group.Length);
                return new ClassificationSpan(result, type);
            }

            return null;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
}