// <copyright file="MainWindow.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdPatientFinder.ViewModels;

namespace Aloe.Apps.Medock.MdPatientFinder;

/// <summary>
/// 患者検索画面。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }
}
