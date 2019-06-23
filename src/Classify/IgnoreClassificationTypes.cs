using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace IgnoreFiles
{
    public static class IgnoreClassificationTypes
    {
        public const string Keyword = "Ignore_keyword";
        public const string Path = "Ignore_path";
        public const string PathNoMatch = "Ignore_path_no_match";

        [Export, Name(Keyword)]
        public static ClassificationTypeDefinition IgnoreClassificationBold { get; set; }

        [Export, Name(Path)]
        public static ClassificationTypeDefinition IgnoreClassificationPath { get; set; }

        [Export, Name(PathNoMatch)]
        public static ClassificationTypeDefinition IgnoreClassificationPathNoMatch { get; set; }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = IgnoreClassificationTypes.Keyword)]
    [Name(IgnoreClassificationTypes.Keyword)]
    [Order(After = Priority.Default)]
    [UserVisible(true)]
    internal sealed class IgnoreBoldFormatDefinition : ClassificationFormatDefinition
    {
        public IgnoreBoldFormatDefinition()
        {
            this.ForegroundBrush = Brushes.OrangeRed;
            this.IsBold = true;
            this.DisplayName = "Ignore Keyword";
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = IgnoreClassificationTypes.Path)]
    [Name(IgnoreClassificationTypes.Path)]
    [Order(After = Priority.Default)]
    [UserVisible(true)]
    internal sealed class IgnorePathFormatDefinition : ClassificationFormatDefinition
    {
        public IgnorePathFormatDefinition()
        {
            this.ForegroundBrush = Brushes.SteelBlue;
            this.DisplayName = "Ignore Path";
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = IgnoreClassificationTypes.PathNoMatch)]
    [Name(IgnoreClassificationTypes.PathNoMatch)]
    [Order(After = Priority.Default)]
    [UserVisible(true)]
    internal sealed class IgnorePathNoMatchFormatDefinition : ClassificationFormatDefinition
    {
        public IgnorePathNoMatchFormatDefinition()
        {
            this.ForegroundOpacity = 0.4;
            this.DisplayName = "Ignore Path No Match";
        }
    }
}
