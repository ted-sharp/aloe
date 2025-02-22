using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv
{
    /// <summary>
    /// ReservationCalendar.xaml の相互作用ロジック
    /// </summary>
    public partial class ReservationCalendar : UserControl
    {
        public ReservationCalendar()
        {
            this.InitializeComponent();
        }
    }

    /// <summary>
    /// ItemsSourceから表示内容に変換します。
    /// </summary>
    public class CalendarDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length <= 0) || (values[0] == DependencyProperty.UnsetValue))
            {
                return DependencyProperty.UnsetValue;
            }

            var date = ((DateTime)values[0]).Date;

            if (values[1] is ObservableCollection<CalendarDayItemModel> calendarDataCollection)
            {
                var chunkItems = calendarDataCollection.Where(
                    x => x.DisplayDate.Date == date);

                return chunkItems;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 各日付の内容です。
    /// </summary>
    public class CalendarDayItemModel : INotifyPropertyChanged
    {
        #region Props

        private DateTime displayDate;

        /// <summary>
        /// 表示したい日付
        /// </summary>
        public DateTime DisplayDate
        {
            get { return this.displayDate; }
            private set
            {
                this.displayDate = value;
                this.RaisePropertyChanged(nameof(this.DisplayDate));
            }
        }

        private object displayContent;

        /// <summary>
        /// 表示する文字列
        /// </summary>
        public object DisplayContent
        {
            get { return this.displayContent; }
            private set
            {
                this.displayContent = value;
                this.RaisePropertyChanged(nameof(this.DisplayContent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Props

        /// <summary>
        /// Ctor.
        /// </summary>
        public CalendarDayItemModel() { }

        #region Static

        ///// <summary>
        ///// カレンダーの内部に表示するモデルを生成します。
        ///// </summary>
        ///// <param name="orderPatients">その月の受診者情報</param>
        ///// <returns>カレンダーの内容</returns>
        //public static IEnumerable<CalendarDayItemModel> CreateCalendarDayItems(
        //    DateTime startDate,
        //    IEnumerable<CalendarResult> calendarResults)
        //{
        //    var calendarDayItems = new List<CalendarDayItemModel>();

        //    var orderedOrderPatients = orderPatients
        //        ?.OrderBy(x => x.CheckupDate)
        //        ?.ThenBy(x => x.ReservationDateTime)
        //        ?.ToList()
        //        ?.OrEmpty();

        //    var calendarDateCalendarResultsList = calendarResults?.GroupBy(x => x.CalendarDate);
        //    var calendarDateCalendarMemosList = calendarMemos?.GroupBy(x => x.CalendarDate);
        //    var searchDateOrderPatientsList = orderedOrderPatients?.GroupBy(x => x.SearchDate);

        //    while (startDate <= endDate)
        //    {
        //        var calendarDateCalendarResults = calendarDateCalendarResultsList?.FirstOrDefault(x => x.Key == startDate);
        //        var calendarDateCalendarMemo = calendarDateCalendarMemosList?.FirstOrDefault(x => x.Key == startDate)?.FirstOrDefault();
        //        var searchDateOrderPatients = searchDateOrderPatientsList?.FirstOrDefault(x => x.Key == startDate);

        //        var item = CalendarDayItemModel.CreateCalendarDayItem(
        //            startDate,
        //            categoryCodes,
        //            calendarDateCalendarResults,
        //            calendarDateCalendarMemo,
        //            searchDateOrderPatients,
        //            isSexDetail);
        //        calendarDayItems.Add(item);

        //        startDate = startDate.AddDays(1);
        //    }

        //    return calendarDayItems;
        //}

        ///// <summary>
        ///// カレンダーに印字する文字列を生成します。
        ///// </summary>
        ///// <param name="orderPatients">その日の受診者情報</param>
        ///// <returns>カレンダーに表示する文字列</returns>
        //public static CalendarDayItemModel CreateCalendarDayItem(
        //    DateTime date,
        //    IEnumerable<int> categoryCodes,
        //    IEnumerable<CalendarResult> calendarResults,
        //    CalendarMemo calendarMemo,
        //    IEnumerable<CheckupOrderPatientViewForCalendar> orderPatients,
        //    bool isSexDetail)
        //{
        //    var summaries = CreateCalendarDayItemSummaries(
        //        categoryCodes,
        //        calendarResults,
        //        orderPatients);

        //    var content = CreateCalendarDayItemContent(
        //        summaries,
        //        calendarMemo,
        //        isSexDetail);

        //    var item = new CalendarDayItemModel
        //    {
        //        DisplayDate = date,
        //        DisplayContent = content,
        //    };

        //    return item;
        //}

        ///// <summary>
        ///// カテゴリ毎の集計を行います。
        ///// </summary>
        //private static List<CalendarDayItemDetail> CreateCalendarDayItemSummaries(
        //    IEnumerable<int> categoryCodes,
        //    IEnumerable<CalendarResult> calendarResults,
        //    IEnumerable<CheckupOrderPatientViewForCalendar> orderPatients)
        //{
        //    var summaries = new Dictionary<int /* categoryCode */, CalendarDayItemDetail>();

        //    var groupedCheckupItemsList = CalendarCheckupItems.GroupBy(x => x.CalendarCategoryCode);
        //    var groupedCheckupCoursesList = CalendarCheckupCourses.GroupBy(x => x.CalendarCategoryCode);

        //    foreach (var calendarResult in calendarResults.OrEmpty())
        //    {
        //        var calendarCode = calendarResult.CalendarCategoryCode;
        //        var detail = summaries.GetOrDefault(calendarCode)
        //                     ?? new CalendarDayItemDetail();
        //        detail.Count += calendarResult.CalendarCount;
        //        detail.Max = calendarResult.CalendarCountMax;
        //        detail.MaxOrigin = calendarResult.CalendarCountMaxOrigin;
        //        detail.IsEmpty = false;
        //        summaries[calendarCode] = detail;
        //    }

        //    foreach (var orderPatient in orderPatients.OrEmpty())
        //    {
        //        var isMale = SexEnum.IsMale(orderPatient.Sex);
        //        var isFemale = SexEnum.IsFemale(orderPatient.Sex);

        //        // 項目でカウント
        //        foreach (var checkupItems in groupedCheckupItemsList)
        //        {
        //            var calendarCode = checkupItems.Key;
        //            var existsCode = categoryCodes?.Any(x => x == calendarCode) ?? false;
        //            if (!existsCode)
        //            {
        //                continue;
        //            }

        //            var detail = summaries.GetOrDefault(calendarCode)
        //                         ?? new CalendarDayItemDetail();

        //            if ((calendarCode == (int)CalendarCategoryType.MedicalCheckup) &&
        //                Global.Policy.IsEnabledCalendarCountByCheckupOrder)
        //            {
        //                detail.Count++;
        //                if (isMale) { detail.MaleCount++; }
        //                if (isFemale) { detail.FemaleCount++; }
        //            }
        //            else
        //            {
        //                var existsOrder = orderPatient.CalendarCodeStrings.Contains(calendarCode.ToString());
        //                if (existsOrder)
        //                {
        //                    detail.Count++;
        //                    if (isMale) { detail.MaleCount++; }
        //                    if (isFemale) { detail.FemaleCount++; }
        //                }
        //            }

        //            summaries[calendarCode] = detail;
        //        }

        //        // コースでカウント
        //        foreach (var checkupCourses in groupedCheckupCoursesList)
        //        {
        //            var calendarCode = checkupCourses.Key;
        //            var existsCode = categoryCodes?.Any(x => x == calendarCode) ?? false;
        //            if (!existsCode)
        //            {
        //                continue;
        //            }

        //            var detail = summaries.GetOrDefault(calendarCode)
        //                         ?? new CalendarDayItemDetail();

        //            if ((calendarCode == (int)CalendarCategoryType.MedicalCheckup) &&
        //                Global.Policy.IsEnabledCalendarCountByCheckupOrder)
        //            {
        //                detail.Count++;
        //                if (isMale) { detail.MaleCount++; }
        //                if (isFemale) { detail.FemaleCount++; }
        //            }
        //            else
        //            {
        //                var existsOrder = orderPatient.CalendarCodeStrings.Contains(calendarCode.ToString());
        //                if (existsOrder)
        //                {
        //                    detail.Count++;
        //                    if (isMale) { detail.MaleCount++; }
        //                    if (isFemale) { detail.FemaleCount++; }
        //                }
        //            }

        //            summaries[calendarCode] = detail;
        //        }
        //    }

        //    foreach (var summary in summaries)
        //    {
        //        var categoryCode = summary.Key;
        //        var category = CalendarCategory.FindCategory(categoryCode);
        //        summary.Value.CategoryName = category?.CalendarCategoryName;
        //        summary.Value.AscOrder = category?.AscOrder ?? -1;
        //    }

        //    return summaries
        //        .OrderBy(x => x.Value.AscOrder)
        //        .Select(x => x.Value)
        //        .ToList();
        //}

        //private static object CreateCalendarDayItemContent(
        //    List<CalendarDayItemDetail> summaries,
        //    CalendarMemo calendarMemo,
        //    bool isSexDetail)
        //{
        //    var grid = new Grid();

        //    var textBlockStyle = Application.Current.FindResource("CommonTextBlockStyle") as Style;

        //    // カテゴリ、カウント(M, F)
        //    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        //    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
        //    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

        //    var row = 0;
        //    foreach (var summary in summaries)
        //    {
        //        var detail = summary;

        //        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

        //        var toolTipText = new TextBlock
        //        {
        //            Text = detail.ToToolTipString(),
        //        };

        //        var foreground = detail.GetForegroundBrush();
        //        var background = detail.GetBackgroundBrush();

        //        var margin = new Thickness(5, 0, 5, 0);

        //        var panel = new StackPanel
        //        {
        //            Background = background,
        //        };
        //        panel.SetGrid(row, 0, columnSpan: 3);
        //        grid.Children.Add(panel);

        //        var categoryText = new TextBlock
        //        {
        //            Text = summary.CategoryName,
        //            ToolTip = toolTipText,
        //            Style = textBlockStyle,
        //            Foreground = foreground,
        //            Background = background,
        //            HorizontalAlignment = HorizontalAlignment.Right,
        //            Margin = margin,
        //        };
        //        categoryText.SetGrid(row, 0);
        //        grid.Children.Add(categoryText);

        //        if (isSexDetail)
        //        {
        //            var maleCountText = new TextBlock
        //            {
        //                Text = "M: " + detail.MaleCount,
        //                Style = textBlockStyle,
        //                Foreground = detail.GetMaleCountBrush(),
        //                Background = background,
        //                HorizontalAlignment = HorizontalAlignment.Left,
        //                Margin = margin,
        //            };
        //            maleCountText.SetGrid(row, 1);
        //            grid.Children.Add(maleCountText);

        //            var femaleCountText = new TextBlock
        //            {
        //                Text = "F: " + detail.FemaleCount,
        //                Style = textBlockStyle,
        //                Foreground = detail.GetFemaleCountBrush(),
        //                Background = background,
        //                HorizontalAlignment = HorizontalAlignment.Left,
        //                Margin = margin,
        //            };
        //            femaleCountText.SetGrid(row, 2);
        //            grid.Children.Add(femaleCountText);
        //        }
        //        else
        //        {
        //            var countText = new TextBlock
        //            {
        //                Text = detail.Count.ToString(),
        //                ToolTip = toolTipText,
        //                Style = textBlockStyle,
        //                Foreground = foreground,
        //                Background = background,
        //                HorizontalAlignment = HorizontalAlignment.Right,
        //                Margin = margin,
        //            };
        //            countText.SetGrid(row, 1, columnSpan: 2);
        //            grid.Children.Add(countText);
        //        }

        //        row++;
        //    }

        //    var hasMemo = calendarMemo?.HasMemo ?? false;
        //    if (hasMemo)
        //    {
        //        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

        //        var toolTipText = new TextBlock
        //        {
        //            Text = calendarMemo.CalendarMemo123,
        //        };

        //        var memoText = new TextBlock
        //        {
        //            Text = "※" + calendarMemo.CalendarMemo123Peek,
        //            ToolTip = toolTipText,
        //            Style = textBlockStyle,
        //        };
        //        memoText.SetGrid(row, 0, columnSpan: 3);
        //        grid.Children.Add(memoText);
        //        row++;
        //    }

        //    return grid;
        //}

        #endregion Static
    }

}
