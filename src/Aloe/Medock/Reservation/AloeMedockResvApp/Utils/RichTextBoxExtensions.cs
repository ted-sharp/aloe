using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

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
