using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    public interface IListPickerPage
    {
        string HeaderText { get; set; }
        IList Items { get; }
        SelectionMode SelectionMode { get; set; }
        object SelectedItem { get; set; }
        IList SelectedItems { get; }
        DataTemplate FullModeItemTemplate { get; set; }
        //bool IsOpen { get; set; }
    }
}