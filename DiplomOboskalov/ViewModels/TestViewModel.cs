using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomOboskalov.ViewModels
{
    public class TestViewModel : ViewModelBase
    {
        public ISeries[] Series { get; set; }
            = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { 7 }, DataLabelsPaint = new SolidColorPaint(SKColors.Black), DataLabelsSize = 22, DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle, DataLabelsFormatter = point => "2П"},
                new PieSeries<double> { Values = new double[] { 7 }, DataLabelsPaint = new SolidColorPaint(SKColors.Black), DataLabelsSize = 22, DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle, DataLabelsFormatter = point => "4П"},
                new PieSeries<double> { Values = new double[] { 6 }, DataLabelsPaint = new SolidColorPaint(SKColors.Black), DataLabelsSize = 22, DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle, DataLabelsFormatter = point => "1А"},
                new PieSeries<double> { Values = new double[] { 3 }, DataLabelsPaint = new SolidColorPaint(SKColors.Black), DataLabelsSize = 22, DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle, DataLabelsFormatter = point => "4ДО"}
            };
    }
}
