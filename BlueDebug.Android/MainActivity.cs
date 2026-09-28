using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;
using System.Linq;

namespace BlueDebug.Android;

// Avalonia 12：Activity 改为非泛型，App 的关联与 AppBuilder 定制
// 都在 Application.cs 的 AvaloniaAndroidApplication<App> 中完成。
[Activity(
    Label = "BlueDebug",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/Icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    private const int BlePermissionRequestCode = 1001;

    // 运行时需要的权限：
    // - Android 12+ (API 31)：BLUETOOTH_SCAN / BLUETOOTH_CONNECT
    // - Android 12 以下：BLE 扫描结果被认为可推断位置，需要精确定位权限
    private static string[] RequiredPermissions =>
        Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? new[]
            {
                global::Android.Manifest.Permission.BluetoothScan,
                global::Android.Manifest.Permission.BluetoothConnect,
                global::Android.Manifest.Permission.AccessFineLocation,
            }
            : new[]
            {
                global::Android.Manifest.Permission.AccessFineLocation,
            };

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        RequestBlePermissionsIfNeeded();
    }

    private void RequestBlePermissionsIfNeeded()
    {
        var missing = RequiredPermissions
            .Where(p => CheckSelfPermission(p) != Permission.Granted)
            .ToArray();

        if (missing.Length > 0)
        {
            // 系统会弹出权限申请对话框，结果回调在 OnRequestPermissionsResult
            RequestPermissions(missing, BlePermissionRequestCode);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode != BlePermissionRequestCode) return;

        var denied = permissions
            .Zip(grantResults, (p, r) => (Permission: p, Granted: r == Permission.Granted))
            .Where(x => !x.Granted)
            .Select(x => x.Permission)
            .ToList();

        if (denied.Count > 0)
        {
            // 用户拒绝了部分权限：扫描/连接会在使用时失败并在控制台记录日志。
            // 不再循环弹窗，避免骚扰用户；如需引导可在 UI 中提示去系统设置开启。
            System.Diagnostics.Debug.WriteLine($"[BlueDebug] 蓝牙权限被拒绝: {string.Join(", ", denied)}");
        }
    }
}
