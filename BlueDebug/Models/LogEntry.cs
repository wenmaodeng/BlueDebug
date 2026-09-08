using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BlueDebug.Models;

public partial class LogEntry : ObservableObject
{
    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private string _type = ""; // TX, RX, SYS

    [ObservableProperty]
    private string _uuid = "";

    [ObservableProperty]
    private string _data = "";

    [ObservableProperty]
    private string _description = "";
}