using Android.Content;
using NinimumWarehouse.Services;
namespace NinimumWarehouse;
// Built-in PDA scanners in "broadcast" output mode. The action and data key differ per firmware,
// so the common ones are all accepted; any other text extra is used as a last resort.
internal sealed class ScannerReceiver : BroadcastReceiver
{
    public static readonly string[] Actions =
    [
        "android.intent.ACTION_DECODE_DATA", "scan.rcv.message", "urovo.rcv.message", "com.scanner.broadcast",
        "nlscan.action.SCANNER_RESULT", "com.android.server.scannerservice.broadcast", "com.sunmi.scanner.ACTION_DATA_CODE_RECEIVED",
        "com.android.serial.BARCODEPORT_RECEIVEDDATA_ACTION", "android.intent.action.SCANRESULT", "com.barcode.sendBroadcast",
        "com.xcheng.scanner.action.BARCODE_DECODING_BROADCAST", "android.scanner.broadcast", "com.ninimum.warehouse.SCAN",
    ];
    private static readonly string[] Keys =
    [
        "barcode_string", "barcode", "barcodeData", "data", "scannerdata", "SCAN_BARCODE1", "value", "decode_data",
        "scan_result", "BARCODEPORT_RECEIVEDDATA_EXTRA_DATA", "EXTRA_BARCODE_DECODING_DATA", "DATA", "barocode",
    ];

    public override void OnReceive(Context? context, Intent? intent)
    {
        var extras = intent?.Extras;
        if (extras is null) return;
        foreach (var key in Keys)
        {
            if (!extras.ContainsKey(key)) continue;
            var value = extras.Get(key);
            if (value is Java.Lang.String text) { HardwareScanner.Publish(text.ToString()); return; }
            if (extras.GetByteArray(key) is { Length: > 0 } bytes)
            {
                int length = extras.ContainsKey("length") ? extras.GetInt("length", bytes.Length) : bytes.Length;
                HardwareScanner.Publish(System.Text.Encoding.UTF8.GetString(bytes, 0, Math.Clamp(length, 0, bytes.Length)));
                return;
            }
        }
        foreach (var key in extras.KeySet() ?? [])
            if (extras.Get(key) is Java.Lang.String text && text.Length() > 0) { HardwareScanner.Publish(text.ToString()); return; }
    }
}
