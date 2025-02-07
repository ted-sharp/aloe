using System.Windows.Controls;
using System.Windows.Documents;

namespace Aloe.Common.AloeCoreLib.Wpf.Extensions;

public static class RichTextBoxExtensions
{
    public static string GetText(this RichTextBox richTextBox)
    {
        var textRange = new TextRange(
            richTextBox.Document.ContentStart,
            richTextBox.Document.ContentEnd
        );

        return textRange.Text;
    }
}
