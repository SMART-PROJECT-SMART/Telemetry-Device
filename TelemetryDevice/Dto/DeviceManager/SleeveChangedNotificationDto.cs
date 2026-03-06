using Core.Common.Enums;

namespace TelemetryDevices.Dto.DeviceManager
{
    public class SleeveChangedNotificationDto
    {
        public SleeveChangedNotificationDto(CrudOperation operation, int id, string name)
        {
            Operation = operation;
            Id = id;
            Name = name;
        }

        public CrudOperation Operation { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
