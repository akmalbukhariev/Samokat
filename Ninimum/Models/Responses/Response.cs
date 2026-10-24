
using Utils;

namespace Models.Responses
{
    public class Response<ResultData> where ResultData : class, new()
    {
        private string? message;
        public string? resultCode {  get; set; }
        // Shown to the customer as is, so it is always Uzbek/localized text (see ServerMessage).
        public string? resultMsg { get => ServerMessage.Show(resultCode, message); set => message = value; }
        public ResultData resultData { get; set; }
        public string? apiVersion { get; set; }        
        public string? webVersion { get; set; }

        public Response()
        {
            resultCode = ApiResult.FAILED.GetCodeToString();
            resultMsg = ApiResult.FAILED.GetMessage();
            resultData = new ResultData();
            apiVersion = "";
            webVersion = "";
        }
    }

    public class Response
    {
        private string? message;
        public string? resultCode { get; set; }
        public string? resultMsg { get => ServerMessage.Show(resultCode, message); set => message = value; }
        public string? apiVersion { get; set; }
        public string? webVersion { get; set; }
    }
}
