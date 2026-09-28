using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BlueDebug.Models;

public partial class HeartRateData : ObservableObject
{
    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private int _bpm;

    [ObservableProperty]
    private string _rawHex = "";

    [ObservableProperty]
    private bool _contactDetected;

    [ObservableProperty]
    private double _rrInterval;
}