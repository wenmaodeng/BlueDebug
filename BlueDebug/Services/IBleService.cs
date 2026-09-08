using BlueDebug.Models;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BlueDebug.Services;

public interface IBleService
{
    bool IsScanning { get; }
    bool IsConnected { get; }
    IDevice? ConnectedDevice { get; }
    ObservableCollection<BleDevice> ScannedDevices { get; }
    ObservableCollection<BleService> DiscoveredServices { get; }
    ObservableCollection<LogEntry> Logs { get; }
    ObservableCollection<HeartRateData> HeartRateHistory { get; }

    event EventHandler<string>? OnLogAdded;
    event EventHandler<HeartRateData>? OnHeartRateUpdated;
    event EventHandler<byte[]>? OnCharacteristicChanged;

    Task StartScanAsync();
    Task StopScanAsync();
    Task<bool> ConnectAsync(BleDevice device);
    Task DisconnectAsync();
    Task DiscoverServicesAsync();
    Task<byte[]?> ReadAsync(BleCharacteristic characteristic);
    Task WriteAsync(BleCharacteristic characteristic, byte[] data);
    Task SubscribeAsync(BleCharacteristic characteristic);
    Task UnsubscribeAsync(BleCharacteristic characteristic);
    Task RequestMtuAsync(int mtu);
}