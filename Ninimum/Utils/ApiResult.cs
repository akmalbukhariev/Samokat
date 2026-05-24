
namespace Utils
{
    public enum ApiResult
    {
        SUCCESS = 100,
        FAILED = 101,

        USER_EXIST = 140,
        USER_NOT_EXIST = 141,
        COMPANY_NOT_EXIST = 142,
        COMPANY_EXIST = 143,
        BRANCH_NOT_EXIST = 144,
        BRANCH_EXIST = 145,
        POSTER_EXIST = 146,
        POSTER_NOT_EXIST = 147,
        POSTER_COMMENT_NOT_EXIST = 148,
        POSTER_COMMENT_EXIST = 149,
        POSTER_FEEDBACK_EXIST = 150,
        POSTER_FEEDBACK_NOT_EXIST = 151,
        PROMOTION_EXIST = 152,
        PROMOTION_NOT_EXIST = 153,
        USER_PASSWORD_NOT_MATCHED = 154,
        NOT_FOUND = 155,
        FOUND = 156,
        BLOCK_USER = 157,
        DELETE_USER = 158,

        TOKEN_INVALID = 200,
        TOKEN_EXPIRED_TIME = 201,
        TOKEN_UNSUPPORTED_JWT = 202,
        LOGIN_INVALID_TOKEN = 250,
        LOGIN_DUPLICATE = 251,
        LOGIN = 252,
        LOGIN_INACTIVE = 253,
        LOGIN_BANNED = 254,
        LOGIN_FAILED = 255,
        PASSWORD_IS_NOT_MATCHED = 256,
        VERIFY_PHONE_NUMBER_FAILED = 257,

        AUTHENTICATION_ERROR = 300,
        INTERNAL_ERROR = 301,
        SERVER_ERROR = 302,
        TOKEN_EMPTY = 360,

        API_SERVICE_ERROR = 361,
        JSON_PARSING_ERROR = 362,
        UNKNOWN_ERROR = 363
    }
     
    public static class ResultExtensions
    {
       private static readonly Dictionary<ApiResult, string> _messages = new()
       {
            { ApiResult.SUCCESS, "Success." },
            { ApiResult.FAILED, "Failed." },
            { ApiResult.USER_EXIST, "User exist" },
            { ApiResult.USER_NOT_EXIST, "User is not exist" },
            { ApiResult.COMPANY_NOT_EXIST, "Company is not exist" },
            { ApiResult.COMPANY_EXIST, "Company is exist" },
            { ApiResult.BRANCH_NOT_EXIST, "Branch is not exist" },
            { ApiResult.BRANCH_EXIST, "Branch is exist" },
            { ApiResult.POSTER_EXIST, "Poster is exist" },
            { ApiResult.POSTER_NOT_EXIST, "Poster is not exist" },
            { ApiResult.POSTER_COMMENT_NOT_EXIST, "Poster comment is not exist" },
            { ApiResult.POSTER_COMMENT_EXIST, "Poster comment is exist" },
            { ApiResult.POSTER_FEEDBACK_EXIST, "Poster feedback is exist" },
            { ApiResult.POSTER_FEEDBACK_NOT_EXIST, "Poster feedback is not exist" },
            { ApiResult.PROMOTION_EXIST, "Promotion is exist" },
            { ApiResult.PROMOTION_NOT_EXIST, "Promotion is not exist" },
            { ApiResult.USER_PASSWORD_NOT_MATCHED, "Password is not matched!" },
            { ApiResult.NOT_FOUND, "Not found!" },
            { ApiResult.BLOCK_USER, "User blocked!" },
            { ApiResult.DELETE_USER, "User deleted!" },
            { ApiResult.FOUND, "Found!" },
            { ApiResult.TOKEN_INVALID, "Invalid token information." },
            { ApiResult.TOKEN_EXPIRED_TIME, "This token is expired." },
            { ApiResult.TOKEN_UNSUPPORTED_JWT, "Unsupported token information." },
            { ApiResult.LOGIN_INVALID_TOKEN, "Token information cannot be verified." },
            { ApiResult.LOGIN_DUPLICATE, "Duplicate login." },
            { ApiResult.LOGIN, "Please log in first." },
            { ApiResult.LOGIN_INACTIVE, "Please log in first." },
            { ApiResult.LOGIN_FAILED, "Login failed. No response from server."},
            { ApiResult.LOGIN_BANNED, "User is banned. Access denied." },
            { ApiResult.PASSWORD_IS_NOT_MATCHED, "Password is not matched" },
            { ApiResult.VERIFY_PHONE_NUMBER_FAILED, "The phone verification failed." },
            { ApiResult.AUTHENTICATION_ERROR, "Your authentication information cannot be verified." },
            { ApiResult.INTERNAL_ERROR, "Something went wrong on our end. We're working to fix it." },
            { ApiResult.SERVER_ERROR, "A system error has occurred. Please contact your administrator." },
            { ApiResult.TOKEN_EMPTY, "Empty token" },
            { ApiResult.API_SERVICE_ERROR, "Empty or invalid response from server" },
            { ApiResult.JSON_PARSING_ERROR, "JSON Parsing Error" },
            { ApiResult.UNKNOWN_ERROR, "Unknown error occurred" }
        };

        public static string GetMessage(this ApiResult result)
        {
            return _messages.TryGetValue(result, out var message) ? message : "Unknown result code.";
        }

        public static string GetCodeToString(this ApiResult result)
        {
            return ((int)result).ToString();
        }

        public static ApiResult? GetResultByCode(int code)
        {
            if (Enum.IsDefined(typeof(ApiResult), code))
            {
                return (ApiResult)code;
            }
            return null;
        }
    }
}
