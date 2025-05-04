namespace HomestayBookingAPI.Services.BookingLifecycleServices
{
    public interface IBookingLifecycleService
    {
        Task ProcessCompletedBookingsAsync();
        Task CleanupOldBookingsAsync();
    }
}
