using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;
public class Drone
{
    public Guid DroneID { get; set; } //char(36)
    public DroneType DroneType { get; set; }
    public Guid DroneTypeID { get; set; } //char(36)
    public string DroneNameVN { get; set; } //nvarchar(255)
    public string DroneNameEN { get; set; } //nvarchar(255)
    public string Manufacturer { get; set; } //nvarchar(100)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public float Height { get; set; } //float
    public float Weight { get; set; } //float
    public DroneStatus Status { get; private set; }
    public string ImgURL { get; set; } //text

    public void Activate()
    {
        if (Status != DroneStatus.INACTIVE)
            throw new DomainException($"Cannot activate drone from status {Status}");

        Status = DroneStatus.ACTIVE;
    }

    public void Inactivate()
    {
        if (Status != DroneStatus.ACTIVE)
            throw new DomainException($"Cannot inactivate drone from status {Status}");

        Status = DroneStatus.INACTIVE;
    }

    public void Deprecate()
    {
        if (Status == DroneStatus.DEPRECATED)
            throw new DomainException("Drone is already deprecated.");

        Status = DroneStatus.DEPRECATED;
    }

    public void TransitionTo(DroneStatus targetStatus)
    {
        if (Status == targetStatus)
            return;

        switch (targetStatus)
        {
            case DroneStatus.ACTIVE:
                Activate();
                break;
            case DroneStatus.INACTIVE:
                Inactivate();
                break;
            case DroneStatus.DEPRECATED:
                Deprecate();
                break;
            default:
                throw new DomainException($"Unsupported drone status {targetStatus}");
        }
    }

}
