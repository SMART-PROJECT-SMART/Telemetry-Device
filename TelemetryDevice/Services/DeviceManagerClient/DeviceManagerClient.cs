using TelemetryDevices.Common;
using TelemetryDevices.Dto.DeviceManager;

namespace TelemetryDevices.Services.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly HttpClient _httpClient;

        public DeviceManagerClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(TelemetryDeviceConstants.HttpClients.DEVICE_MANAGER_HTTP_CLIENT);
        }

        public async Task<IEnumerable<DeviceManagerSleeveDto>> GetAllSleevesAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(TelemetryDeviceConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<DeviceManagerSleeveDto>();
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<DeviceManagerSleeveDto>>(cancellationToken);
        }
    }
}
