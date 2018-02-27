namespace Nop.Core.Domain.Shipping
{
    public enum NinjaVanStatus
    {
        PendingPickup = 10,
        Completed = 20,
        Cancelled = 30,
        ReturnedToSender = 40,
        EnrouteToSortingHub = 50,
        OnVehicleForDelivery = 60,
        PendingReschedule = 70
    }
}
