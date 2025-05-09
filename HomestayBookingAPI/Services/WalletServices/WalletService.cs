using System.Text.RegularExpressions;
using HomestayBookingAPI.Data;
using HomestayBookingAPI.Models;
using HomestayBookingAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace HomestayBookingAPI.Services.WalletServices
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(ApplicationDbContext context, ILogger<WalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created new wallet for user {userId}");
            }

            return wallet;
        }

        public async Task<double> GetBalanceAsync(string userId)
        {
            var wallet = await GetOrCreateWalletAsync(userId);
            return wallet.Balance;
        }

        public async Task<bool> HasSufficientFundsAsync(string userId, double amount)
        {
            var wallet = await GetOrCreateWalletAsync(userId);
            return wallet.Balance >= amount;
        }

        public async Task<WalletTransaction> AddTransactionAsync(string userId, double amount, TransactionType type, string description, int? bookingId = null, int? paymentId = null)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var wallet = await GetOrCreateWalletAsync(userId);

                    // Kiểm tra số dư nếu là giao dịch thanh toán
                    if (type == TransactionType.Payment && wallet.Balance < amount)
                    {
                        throw new InvalidOperationException("Số dư không đủ để thực hiện giao dịch");
                    }

                    // Cập nhật số dư
                    switch (type)
                    {
                        case TransactionType.Deposit:
                        case TransactionType.Refund:
                            wallet.Balance += amount;
                            break;
                        case TransactionType.Payment:
                        case TransactionType.Withdrawal:
                            wallet.Balance -= amount;
                            break;
                    }

                    wallet.UpdatedAt = DateTime.UtcNow;
                    _context.Wallets.Update(wallet);

                    // Tạo giao dịch mới
                    var walletTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = amount,
                        Type = type,
                        Description = description,
                        BookingId = bookingId,
                        PaymentId = paymentId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WalletTransactions.Add(walletTransaction);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Added new {type} transaction of {amount} for user {userId}");
                    return walletTransaction;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error adding transaction for user {userId}");
                    throw;
                }
            }
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int page = 1, int pageSize = 10)
        {
            var wallet = await GetOrCreateWalletAsync(userId);

            return await _context.WalletTransactions
                .Where(wt => wt.Wallet.UserId == userId)
                .OrderByDescending(wt => wt.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private string HashPin(string pin)
        {
            
            return BCrypt.Net.BCrypt.HashPassword(pin);
        }

        private bool IsWeakPin(string pin)
        {
            // Danh sách các PIN dễ đoán
            var weakPins = new List<string>
            {
                "000000", "111111", "222222", "333333", "444444",
                "555555", "666666", "777777", "888888", "999999",
                "123456", "654321", "121212", "123123"
            };

            // PIN chứa chuỗi số lặp lại (ví dụ: 112233, 112211)
            bool hasRepeatingPattern = Regex.IsMatch(pin, @"(\d{2,})\1+");

            // PIN là dãy số liên tiếp
            bool isSequential = "0123456789".Contains(pin) || "9876543210".Contains(pin);

            return weakPins.Contains(pin) || hasRepeatingPattern || isSequential;
        }

        public async Task<bool> SetPinAsync(string userId, string pin)
        {
            try{
                if (!Regex.IsMatch(pin, @"^\d{6}$"))
                {
                    throw new ArgumentException("PIN phải chứa đúng 6 chữ số");
                }

                // Kiểm tra PIN dễ đoán
                if (IsWeakPin(pin))
                {
                    throw new ArgumentException("PIN quá dễ đoán. Vui lòng chọn PIN an toàn hơn");
                }

                var wallet = await GetOrCreateWalletAsync(userId);

                // Hash PIN trước khi lưu
                wallet.PinHash = HashPin(pin);
                wallet.UpdatedAt = DateTime.UtcNow;

                _context.Wallets.Update(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Set PIN for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting PIN for user {userId}");
                return false;
            }
        }

        public async Task<bool> VerifyPinAsync(string userId, string pin)
        {
            var wallet = await GetOrCreateWalletAsync(userId);

            if (string.IsNullOrEmpty(wallet.PinHash))
            {
                _logger.LogWarning($"User {userId} has not set a PIN yet");
                return false;
            }

            // Verify PIN using BCrypt
            bool isPinValid = BCrypt.Net.BCrypt.Verify(pin, wallet.PinHash);
            return isPinValid;
        }

        public async Task<bool> HasSetPinAsync(string userId)
        {
            var wallet = await GetOrCreateWalletAsync(userId);
            return !string.IsNullOrEmpty(wallet.PinHash);
        }
    }
}