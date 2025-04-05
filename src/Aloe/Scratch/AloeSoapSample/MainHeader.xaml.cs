using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AloeSoapSample;

/// <summary>
/// MainHeader.xaml の相互作用ロジック
/// </summary>
public partial class MainHeader : UserControl
{
    public MainHeader()
    {
        this.InitializeComponent();
    }
}

public class DiseaseInfo
{
    public string StartDate { get; set; }
    public string DiseaseName { get; set; }
    public string Outcome { get; set; }
}
