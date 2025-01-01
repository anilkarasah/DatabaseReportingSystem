using DatabaseReportingSystem.Shared;
using DatabaseReportingSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseReportingSystem.Context;

public class SystemDbContext(IConfiguration configuration) : DbContext
{
    private readonly IConfiguration _configuration = configuration;

    public DbSet<User> Users { get; set; }

    public DbSet<UserLicense> UserLicenses { get; set; }

    public DbSet<Chat> Chats { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<ModelResponse> ModelResponses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("System"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region [ Users ]

        modelBuilder.Entity<User>()
            .HasKey(user => user.Id);

        modelBuilder.Entity<User>()
            .OwnsOne<ConnectionCredentials>(user => user.ConnectionCredentials);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Licenses)
            .WithOne(license => license.User)
            .HasForeignKey(license => license.UserId);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Chats)
            .WithOne(chat => chat.User)
            .HasForeignKey(chat => chat.UserId);

        modelBuilder.Entity<User>()
            .Property(user => user.Email)
            .HasMaxLength(Constants.Context.EmailSize);

        modelBuilder.Entity<User>()
            .Property(user => user.PasswordHash)
            .HasMaxLength(Constants.Context.PasswordSize);

        #endregion

        #region [ User Licenses ]

        modelBuilder.Entity<UserLicense>()
            .HasKey(license => license.Id);

        modelBuilder.Entity<UserLicense>()
            .HasIndex(license => license.UserId);

        modelBuilder.Entity<UserLicense>()
            .Property(license => license.LicenseCode)
            .HasMaxLength(Constants.Context.LicenseCodeSize);

        #endregion

        #region [ Chats ]

        modelBuilder.Entity<Chat>()
            .HasKey(chat => chat.Id);

        modelBuilder.Entity<Chat>()
            .HasMany(chat => chat.Messages)
            .WithOne(message => message.Chat)
            .HasForeignKey(message => message.ChatId);

        modelBuilder.Entity<Chat>()
            .Property(chat => chat.DatabaseManagementSystem)
            .HasMaxLength(Constants.Context.DatabaseManagementSystemSize);

        #endregion

        #region [ Chat Messages ]

        modelBuilder.Entity<ChatMessage>()
            .HasKey(message => message.MessageId);

        modelBuilder.Entity<ChatMessage>()
            .HasMany(message => message.ModelResponses)
            .WithOne(response => response.Message)
            .HasForeignKey(response => response.MessageId);

        #endregion

        #region [ Model Responses ]

        modelBuilder.Entity<ModelResponse>()
            .HasKey(response => new { response.MessageId, response.ModelName });

        modelBuilder.Entity<ModelResponse>()
            .Property(response => response.ModelName)
            .HasMaxLength(Constants.Context.ModelNameSize);

        #endregion
    }
}
