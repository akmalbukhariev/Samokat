using NinimumWarehouse.Resources.Languages;
namespace NinimumWarehouse.Services;
public static class LocalizedMessages
{
    public static string Status(string code) => code switch {
        "WAITING" => AppResource.Waiting, "PICKING" => AppResource.Picking,
        "BLOCKED" => AppResource.Blocked, "READY" => AppResource.Ready, _ => AppResource.UnknownStatus };
    public static string Error(string code) => code switch {
        "WAREHOUSE_LOGIN_FAILED" => AppResource.LoginFailed,
        "WAREHOUSE_SESSION_REPLACED" or "SESSION" => AppResource.SessionEnded,
        "WAREHOUSE_PREPARATION_DISABLED" => AppResource.PreparationDisabled,
        "WAREHOUSE_ALREADY_CLAIMED" or "WAREHOUSE_NOT_OWNER" => AppResource.AlreadyClaimed,
        "WAREHOUSE_ORDER_UNAVAILABLE" => AppResource.UnavailableError,
        "WAREHOUSE_BARCODE_MISMATCH" => AppResource.BarcodeMismatch,
        "WAREHOUSE_TOO_MANY" => AppResource.TooMany,
        "WAREHOUSE_INCOMPLETE" => AppResource.Incomplete,
        "WAREHOUSE_INVALID_STATE" => AppResource.InvalidState,
        "WAREHOUSE_INVALID_INPUT" => AppResource.InvalidInput,
        "QUANTITY_INVALID" => AppResource.QuantityInvalid,
        "LOGIN_REQUIRED" => AppResource.LoginRequired,
        "REASON_REQUIRED" => AppResource.ReasonRequired,
        "BARCODE_REQUIRED" => AppResource.BarcodeRequired,
        "CAMERA" => AppResource.CameraPermission, "NETWORK" => AppResource.ConnectionError,
        _ => AppResource.ActionFailed };
}
