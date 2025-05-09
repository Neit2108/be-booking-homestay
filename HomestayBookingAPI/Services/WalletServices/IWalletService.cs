using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;

namespace HomestayBookingAPI.Services.WalletServices
{
    public interface IWalletService
    {
        Task<Wallet> GetOrCreateWalletAsync(string userId);
        Task<double> GetBalanceAsync(string userId);
        Task<bool> HasSufficientFundsAsync(string userId, double amount);
        Task<WalletTransaction> AddTransactionAsync(string userId, double amount, TransactionType type, string description, int? bookingId = null, int? paymentId = null);
        Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int page = 1, int pageSize = 10);
        Task<bool> SetPinAsync(string userId, string pin);
        Task<bool> VerifyPinAsync(string userId, string pin);
        Task<bool> HasSetPinAsync(string userId);
    }
}