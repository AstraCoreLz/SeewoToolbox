using System.ComponentModel;
using System.Runtime.CompilerServices;
using SeewoToolbox.ViewModels;

namespace SeewoToolbox.Models;

public class SoftwareItem : ViewModelBase
{
    private string _name = string.Empty;
    private string _url = string.Empty;
    private string _filename = string.Empty;
    private bool _isSelected;

    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    public string Url
    {
        get => _url;
        set => Set(ref _url, value);
    }

    public string Filename
    {
        get => _filename;
        set => Set(ref _filename, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }
}
