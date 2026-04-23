using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.Converts;

public class WeightSumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<GradeComponent> formula && formula != null)
        {
            double sum = formula.Sum(c => c.Weight);
                
            return sum.ToString("F1");
        }
            
        return "0.0";
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
