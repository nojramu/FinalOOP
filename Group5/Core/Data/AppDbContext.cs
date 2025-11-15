using Group5.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<OfficialBorrowListRecord> OfficialBorrowListRecords => Set<OfficialBorrowListRecord>();
        public DbSet<BorrowedItem> BorrowedItems => Set<BorrowedItem>();
        public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
        public DbSet<TempSignup> TempSignups => Set<TempSignup>();
        public DbSet<BorrowForm> BorrowForms => Set<BorrowForm>();
        public DbSet<BorrowFormItem> BorrowFormItems => Set<BorrowFormItem>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    }
}


