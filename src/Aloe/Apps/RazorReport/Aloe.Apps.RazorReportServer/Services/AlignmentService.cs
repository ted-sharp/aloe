using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

public class AlignmentService
{
    private readonly GridCalculationService _gridCalcService;

    public AlignmentService(GridCalculationService gridCalcService)
    {
        _gridCalcService = gridCalcService;
    }

    public void AlignTop(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceRow = referenceElement.Position.Row;

        foreach (var element in elements.SkipLast(1))
        {
            var newPosition = new GridPosition(
                element.Position.Column,
                referenceRow,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }

    public void AlignMiddle(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceMiddle = referenceElement.Position.Row + (referenceElement.Position.RowSpan / 2);

        foreach (var element in elements.SkipLast(1))
        {
            var elementMiddle = element.Position.RowSpan / 2;
            var newRow = Math.Max(0, referenceMiddle - elementMiddle);

            var newPosition = new GridPosition(
                element.Position.Column,
                newRow,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }

    public void AlignBottom(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceBottom = referenceElement.Position.Row + referenceElement.Position.RowSpan;

        foreach (var element in elements.SkipLast(1))
        {
            var newRow = Math.Max(0, referenceBottom - element.Position.RowSpan);

            var newPosition = new GridPosition(
                element.Position.Column,
                newRow,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }

    public void AlignLeft(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceColumn = referenceElement.Position.Column;

        foreach (var element in elements.SkipLast(1))
        {
            var newPosition = new GridPosition(
                referenceColumn,
                element.Position.Row,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }

    public void AlignCenter(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceCenter = referenceElement.Position.Column + (referenceElement.Position.ColumnSpan / 2);

        foreach (var element in elements.SkipLast(1))
        {
            var elementCenter = element.Position.ColumnSpan / 2;
            var newColumn = Math.Max(0, referenceCenter - elementCenter);

            var newPosition = new GridPosition(
                newColumn,
                element.Position.Row,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }

    public void AlignRight(List<DesignElement> elements)
    {
        if (elements.Count < 2)
            return;

        var referenceElement = elements.Last();
        var referenceRight = referenceElement.Position.Column + referenceElement.Position.ColumnSpan;

        foreach (var element in elements.SkipLast(1))
        {
            var newColumn = Math.Max(0, referenceRight - element.Position.ColumnSpan);

            var newPosition = new GridPosition(
                newColumn,
                element.Position.Row,
                element.Position.ColumnSpan,
                element.Position.RowSpan
            );

            if (_gridCalcService.ValidatePosition(newPosition))
                element.Position = newPosition;
        }
    }
}
