
using Models.Requests;
using Models.Responses;
using Newtonsoft.Json;
using RestSharp;
using Utils;

namespace Api.Services
{
    public class UserApiService : ApiService
    {
        #region Url
        private const string BASE_URL = "";
        //private const string BASE_URL = "/ninimum/api/v1/";
        private const string LOGIN_USER = $"{BASE_URL}user/login";
        private const string CHECK_USER = $"{BASE_URL}user/checkUser/";
        private const string LOGOUT_USER = $"{BASE_URL}user/logout";
        private const string REGISTER_USER = $"{BASE_URL}user/register";
        private const string GET_USER_INFO = $"{BASE_URL}user/getUserInfo";
        private const string GET_USER_LIST = $"{BASE_URL}user/getUserByIdList";
        private const string UPDATE_USER_INFO = $"{BASE_URL}user/updateUserInfo";
        private const string UPDATE_USER_PHONE_NUMBER = $"{BASE_URL}user/updateUserPhoneNumber";  
        private const string DELETE_USER_ACCOUNT = $"{BASE_URL}user/deleteUser/";
        #endregion

        public UserApiService(RestClient client)
            : base(client)
        {

        }

        public async Task<LoginUserResponse> Login(LoginUserRequest data)
        {
            var response = await LoginAsync<LoginUserResponse>(LOGIN_USER, data);

            return response ?? new LoginUserResponse
            {
                resultCode = ApiResult.LOGIN_FAILED.GetCodeToString(),
                resultMsg = ApiResult.LOGIN_FAILED.GetMessage()
            };
        }

        public async Task<Response> CheckUser(string phoneNumber)
        {
            var response = new Response();

            try
            {
                var receivedData = await GetAsync($"{CHECK_USER}{phoneNumber}", false);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        public async Task<Response> DeleteUseAccount(string reasons)
        {
            var response = new Response();

            try
            {
                var encodedReasons = Uri.EscapeDataString(reasons ?? string.Empty);
 
                var url = $"{DELETE_USER_ACCOUNT}{encodedReasons}";

                var receivedData = await DeleteAsync(url);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        public async Task<Response> LogOut()
        {
            var response = new Response();

            try
            {
                var receivedData = await PostAsync(LOGOUT_USER, null, false);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        public async Task<Response> UpdateUserPhoneNumber(string new_phone_number)
        {
            var response = new Response();

            try
            {
                var receivedData = await PostAsync($"{UPDATE_USER_PHONE_NUMBER}/{new_phone_number}", null, false);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        public async Task<Response> RegisterUser(RegisterUserRequest data)
        {
            var response = new Response();

            try
            {
                var receivedData = await PostAsync(REGISTER_USER, data);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        /*public async Task<GetUserInfoResponse> GetUserInfo()
        {
            var response = new GetUserInfoResponse();

            try
            {
                var receivedData = await GetAsync(GET_USER_INFO);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<GetUserInfoResponse>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }

        public async Task<Response> UpdateUserInfo(UpdateUserInfoRequest data)
        {
            var response = new Response();

            try
            {
                var receivedData = await PostAsync(UPDATE_USER_INFO, data);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"API: {ex.Message}";
            }

            return response;
        }*/

        public async Task<Response> UpdateUserProfileInfo(Stream imageStream, Dictionary<string, string>? additionalData)
        {
            var response = new Response();

            try
            {
                var receivedData = await PostImageAsync(UPDATE_USER_INFO, imageStream, additionalData, "profile_picture_data");

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<Response>(receivedData);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                response.resultMsg = ApiResult.API_SERVICE_ERROR.GetMessage();
            }
            catch (JsonException jsonEx)
            {
                response.resultCode = ApiResult.JSON_PARSING_ERROR.GetCodeToString();
                response.resultMsg = $"JSON Parsing Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                response.resultCode = ApiResult.API_SERVICE_ERROR.GetCodeToString();
                response.resultMsg = $"UpdateCompanyProfileInfo Error: {ex.Message}";
            }

            return response;
        }
    }
}