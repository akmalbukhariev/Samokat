
using Utils;

namespace Models.Responses
{
    public class Response<ResultData> where ResultData : class, new()
    {
        public string? resultCode {  get; set; }
        public string? resultMsg { get; set; }
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
        public string? resultCode { get; set; }
        public string? resultMsg { get; set; }
        public string? apiVersion { get; set; }
        public string? webVersion { get; set; }
    }
}
