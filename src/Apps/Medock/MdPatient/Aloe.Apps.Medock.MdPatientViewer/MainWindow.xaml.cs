// <copyright file="MainWindow.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdPatientViewer.ViewModels;

namespace Aloe.Apps.Medock.MdPatientViewer;

/// <summary>
/// 患者閲覧画面。
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
