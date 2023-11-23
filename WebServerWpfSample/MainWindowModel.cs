using System.ComponentModel;
using System.Runtime.CompilerServices;
using Westwind.Utilities;

namespace WebServerWpfSample;

public class MainWindowModel : INotifyPropertyChanged
{
        
    public int RequestCount
    {
        get { return _requestCount; }
        set
        {
            if (value == _requestCount) return;
            _requestCount = value;
            OnPropertyChanged();
        }
    }
    private int _requestCount = 0;


  
    public string ServerStatus
    {
        get { return _ServerStatus; }
        set
        {
            if (_ServerStatus == value)
                return;
            _ServerStatus = value;
            if (PropertyChanged == null)
                return;
            var _Args = new PropertyChangedEventArgs("ServerStatus");
            PropertyChanged(this, _Args);
        }
    }
    private string _ServerStatus = "not running";



    public string RequestText
    {
        get { return _requestText; }
        set
        {
            if (value == _requestText) return;
            _requestText = value;
            OnPropertyChanged();
        }
    }
    private string _requestText = "*** Press 'Start Server' to get going.";

    public void AddRequestLine(string requestLine, int lines = 19)
    {
        lock (RequestText)
        {
            var textLines = RequestText.GetLines().TakeLast(lines).ToList();
            textLines.Add(requestLine);
            RequestText = string.Join('\n', textLines);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}