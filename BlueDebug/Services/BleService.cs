using BlueDebug.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;

namespace BlueDebug.Services;

public class BleService : IBleService
{
    private readonly IBluetoothLE _ble;
    private readonly IAdapter _adapter;
    private System.Timers.Timer? _scanTimer;

    public bool IsScanning => _adapter.IsScanning;
    public bool IsConnected => ConnectedDevice != null;
    public IDevice? ConnectedDevice { get; private set; }

    public ObservableCollection<BleDevice> ScannedDevices { get; } = new();
    public ObservableCollection<BleService> DiscoveredServices { get; } = new();
    public ObservableCollection<LogEntry> Logs { get; } = new();
    public ObservableCollection<HeartRateData> HeartRateHistory { get; } = new();

    public event EventHandler<string>? OnLogAdded;
    public event EventHandler<HeartRateData>? OnHeartRateUpdated;
    public event EventHandler<byte[]>? OnCharacteristicChanged;

    public BleService()
    {
        _ble = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;
        _adapter.DeviceConnected += OnDeviceConnected;
        _adapter.DeviceDisconnected += OnDeviceDisconnected;
    }

    private void OnDeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ScannedDevices.Any(d => d.Id == e.Device.Id.ToString())) return;
            var device = new BleDevice
            {
                NativeDevice = e.Device,
                Name = string.IsNullOrEmpty(e.Device.Name) ? "未知设备" : e.Device.Name,
                MacAddress = e.Device.Id.ToString(),
                Rssi = e.Device.Rssi,
                IsConnectable = true
            };
            ScannedDevices.Add(device);
        });
    }

    private void OnDeviceConnected(object? sender, DeviceEventArgs e)
    {
        ConnectedDevice = e.Device;
        AddLog("SYS", "", "", $"已连接 {e.Device.Name}");
    }

    private void OnDeviceDisconnected(object? sender, DeviceEventArgs e)
    {
        ConnectedDevice = null;
        AddLog("SYS", "", "", $"已断开 {e.Device.Name}");
    }

    public async Task StartScanAsync()
    {
        ScannedDevices.Clear();
        await _adapter.StartScanningForDevicesAsync();
        _scanTimer = new System.Timers.Timer(30000);
        _scanTimer.Elapsed += async (s, e) => await StopScanAsync();
        _scanTimer.AutoReset = false;
        _scanTimer.Start();
    }

    public async Task StopScanAsync()
    {
        _scanTimer?.Stop();
        _scanTimer?.Dispose();
        if (_adapter.IsScanning)
            await _adapter.StopScanningForDevicesAsync();
    }

    public async Task<bool> ConnectAsync(BleDevice device)
    {
        if (device.NativeDevice == null) return false;
        try
        {
            await _adapter.ConnectToDeviceAsync(device.NativeDevice);
            return true;
        }
        catch (Exception ex)
        {
            AddLog("SYS", "", "", $"连接失败: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (ConnectedDevice != null)
            await _adapter.DisconnectDeviceAsync(ConnectedDevice);
        DiscoveredServices.Clear();
    }

    public async Task DiscoverServicesAsync()
    {
        if (ConnectedDevice == null) return;
        DiscoveredServices.Clear();
        var services = await ConnectedDevice.GetServicesAsync();
        foreach (var s in services)
        {
            var service = new BleService
            {
                NativeService = s,
                Name = GetServiceName(s.Id),
                Uuid = s.Id.ToString(),
                Type = IsStandardUuid(s.Id.ToString()) ? "标准服务" : "自定义"
            };

            var chars = await s.GetCharacteristicsAsync();
            foreach (var c in chars)
            {
                var props = c.Properties;
                var ch = new BleCharacteristic
                {
                    NativeCharacteristic = c,
                    Name = GetCharacteristicName(c.Id),
                    Uuid = c.Id.ToString(),
                    CanRead = props.HasFlag(CharacteristicPropertyType.Read),
                    CanWrite = props.HasFlag(CharacteristicPropertyType.Write) || props.HasFlag(CharacteristicPropertyType.WriteWithoutResponse),
                    CanNotify = props.HasFlag(CharacteristicPropertyType.Notify),
                    IsIndicate = props.HasFlag(CharacteristicPropertyType.Indicate),
                    Properties = props.ToString()
                };
                service.Characteristics.Add(ch);
            }
            DiscoveredServices.Add(service);
        }
        AddLog("SYS", "", "", $"服务发现完成: {DiscoveredServices.Count} 服务");
    }

    public async Task<byte[]?> ReadAsync(BleCharacteristic characteristic)
    {
        if (characteristic.NativeCharacteristic == null) return null;
        try
        {
            var data = await characteristic.NativeCharacteristic.ReadAsync();
            var hex = BitConverter.ToString(data).Replace("-", " ");
            AddLog("RX", characteristic.Uuid, hex, "读取成功");
            return data;
        }
        catch (Exception ex)
        {
            AddLog("SYS", characteristic.Uuid, "", $"读取失败: {ex.Message}");
            return null;
        }
    }

    public async Task WriteAsync(BleCharacteristic characteristic, byte[] data)
    {
        if (characteristic.NativeCharacteristic == null) return;
        try
        {
            await characteristic.NativeCharacteristic.WriteAsync(data);
            var hex = BitConverter.ToString(data).Replace("-", " ");
            AddLog("TX", characteristic.Uuid, hex, "写入成功");
        }
        catch (Exception ex)
        {
            AddLog("SYS", characteristic.Uuid, "", $"写入失败: {ex.Message}");
        }
    }

    public async Task SubscribeAsync(BleCharacteristic characteristic)
    {
        if (characteristic.NativeCharacteristic == null) return;
        characteristic.NativeCharacteristic.ValueUpdated += (s, e) =>
        {
            var data = e.Characteristic.Value;
            var hex = BitConverter.ToString(data).Replace("-", " ");
            AddLog("RX", characteristic.Uuid, hex, "");
            OnCharacteristicChanged?.Invoke(this, data);

            if (characteristic.Uuid.ToUpper().Contains("2A37"))
            {
                ParseHeartRate(data);
            }
        };
        await characteristic.NativeCharacteristic.StartUpdatesAsync();
        characteristic.IsSubscribed = true;
        AddLog("SYS", characteristic.Uuid, "", "开始通知");
    }

    public async Task UnsubscribeAsync(BleCharacteristic characteristic)
    {
        if (characteristic.NativeCharacteristic == null) return;
        await characteristic.NativeCharacteristic.StopUpdatesAsync();
        characteristic.IsSubscribed = false;
        AddLog("SYS", characteristic.Uuid, "", "停止通知");
    }

    public Task RequestMtuAsync(int mtu)
    {
        // Platform specific
        return Task.CompletedTask;
    }

    private void ParseHeartRate(byte[] data)
    {
        if (data.Length < 1) return;
        var flags = data[0];
        var is16Bit = (flags & 0x01) != 0;
        int offset = 1;
        int bpm = is16Bit ? BitConverter.ToUInt16(data, offset) : data[offset];
        offset += is16Bit ? 2 : 1;

        var hr = new HeartRateData
        {
            Timestamp = DateTime.Now,
            Bpm = bpm,
            RawHex = BitConverter.ToString(data).Replace("-", " "),
            ContactDetected = (flags & 0x06) != 0
        };

        if (data.Length > offset + 1)
        {
            hr.RrInterval = BitConverter.ToUInt16(data, offset) / 1024.0;
        }

        HeartRateHistory.Add(hr);
        if (HeartRateHistory.Count > 100) HeartRateHistory.RemoveAt(0);
        OnHeartRateUpdated?.Invoke(this, hr);
    }

    private void AddLog(string type, string uuid, string data, string desc)
    {
        var log = new LogEntry
        {
            Timestamp = DateTime.Now,
            Type = type,
            Uuid = uuid,
            Data = data,
            Description = desc
        };
        Logs.Add(log);
        if (Logs.Count > 500) Logs.RemoveAt(0);
        OnLogAdded?.Invoke(this, type);
    }

    private static bool IsStandardUuid(string uuid) => uuid.StartsWith("0000") && uuid.Contains("0000-1000-8000-00805f9b34fb");

    private static string GetServiceName(Guid id) => id.ToString().ToUpper() switch
    {
        var s when s.Contains("1800") => "Generic Access",
        var s when s.Contains("1801") => "Generic Attribute",
        var s when s.Contains("180A") => "Device Information",
        var s when s.Contains("180D") => "Heart Rate",
        var s when s.Contains("180F") => "Battery Service",
        var s when s.Contains("181A") => "Environmental Sensing",
        var s when s.Contains("FE59") => "Secure DFU",
        _ => "Unknown Service"
    };

    private static string GetCharacteristicName(Guid id) => id.ToString().ToUpper() switch
    {
        var s when s.Contains("2A37") => "Heart Rate Measurement",
        var s when s.Contains("2A38") => "Body Sensor Location",
        var s when s.Contains("2A39") => "Heart Rate Control Point",
        var s when s.Contains("2A19") => "Battery Level",
        var s when s.Contains("2A29") => "Manufacturer Name",
        var s when s.Contains("2A24") => "Model Number",
        _ => "Unknown Characteristic"
    };
}