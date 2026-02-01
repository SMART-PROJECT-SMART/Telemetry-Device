using Core.Common.Enums;

namespace TelemetryDevices.Dto.DeviceManager
{
    public class SleeveChangedNotificationDto
    {
        public SleeveChangedNotificationDto(CrudOperation operation, string name)
        {
            Operation = operation;
            Name = name;
        }

        public CrudOperation Operation { get; set; }
        public string Name { get; set; }
    }
}
