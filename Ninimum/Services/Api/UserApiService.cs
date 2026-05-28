
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
        private const string CHECK_PHONE_NUMBER = $"{BASE_URL}user/checkPhoneNumber";
        private const string LOGOUT_USER = $"{BASE_URL}user/logout";
        private const string REGISTER_USER = $"{BASE_URL}user/register";
        private const string GET_USER_INFO = $"{BASE_URL}user/getUserInfo";
        private const string GET_USER_LIST = $"{BASE_URL}user/getUserByIdList";
        private const string UPDATE_USER_INFO = $"{BASE_URL}user/updateUserInfo";
        private const string UPDATE_USER_PHONE_NUMBER = $"{BASE_URL}user/updateUserPhoneNumber";
        private const string DELETE_USER_ACCOUNT = $"{BASE_URL}user/deleteUser/";
        private const string VERIFY_NUMBER = $"{BASE_URL}message/verifyPhoneNumber";
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

        public async Task<string> GetAddressFromYandexAsync(double latitude, double longitude)
        {
            try
            {
                const string apiKey = "bb9a670d-13db-4cec-8fc9-a03c8b2b4ece";

                var url =
                    $"https://geocode-maps.yandex.ru/v1/?" +
                    $"apikey={apiKey}" +
                    $"&geocode={longitude},{latitude}" +
                    $"&lang=en_US" +
                    $"&format=json";

                using var httpClient = new HttpClient();

                var json = await httpClient.GetStringAsync(url);

                dynamic result = JsonConvert.DeserializeObject(json);

                string address =
                    result.response.GeoObjectCollection.featureMember[0]
                        .GeoObject.metaDataProperty.GeocoderMetaData.text;

                return address;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yandex address error: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<VerifyPhoneNumberResponse> VerifyNumber(VerifyPhoneNumberRequest data)
        {
            var response = new VerifyPhoneNumberResponse();

            try
            {
                var receivedData = await PostAsync(VERIFY_NUMBER, data);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<VerifyPhoneNumberResponse>(receivedData);
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

        public async Task<CheckPhoneNumberResponse> CheckPhoneNumber(CheckPhoneNumberRequest data)
        {
            var response = new CheckPhoneNumberResponse();

            try
            {
                var receivedData = await PostAsync(CHECK_PHONE_NUMBER, data);

                if (!string.IsNullOrWhiteSpace(receivedData))
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<CheckPhoneNumberResponse>(receivedData);
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