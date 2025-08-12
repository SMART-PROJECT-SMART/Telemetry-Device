using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Simulation.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TelemetryFields
    {
        DragCoefficient,

        LiftCoefficient,

        ThrottlePercent,

        CruiseAltitude,

        Latitude,

        LandingGearStatus,

        Longitude,

        Altitude,

        CurrentSpeedKmph,

        YawDeg,

        PitchDeg,

        RollDeg,

        ThrustAfterInfluence,

        FuelAmount,

        DataStorageUsedGB,

        FlightTimeSec,

        SignalStrength,

        Rpm,

        EngineDegrees,

        NearestSleeveId,

        TailId
    }
}