using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using URBE.Pokemon.API.Models.Database;

namespace URBE.Pokemon.API.Services;

public class UrbeContext : DbContext 
{
    private static bool isInit = false;
    private static readonly object sync = new();

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<MailConfirmationRequest> MailConfirmationRequests => Set<MailConfirmationRequest>();
    public DbSet<ExecutionLogEntry> ExecutionLog => Set<ExecutionLogEntry>();
    public DbSet<Server> Servers => Set<Server>();

    public UrbeContext(DbContextOptions<UrbeContext> options) : base(options)
    {
        if (isInit is false)
            lock (sync)
                if (isInit is false)
                {
                    if (DebugFlags.ClearDatabase)
                        Database.EnsureDeleted();

                    Helper.CreateAppDataDirectory();
                    Database.Migrate();
                    isInit = true;
                }

        ChangeTracker.StateChanged += ChangeTracker_StateChanged;
    }

    private void ChangeTracker_StateChanged(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
    {
        if (e.NewState != e.OldState
            && e.NewState is not EntityState.Unchanged or EntityState.Detached or EntityState.Deleted
            && e.Entry.Entity is MutableDbModel mdm)
        {
            if (e.NewState is EntityState.Added)
                mdm.CreationDate = DateTimeOffset.Now;
            mdm.LastModifiedDate = DateTimeOffset.Now;
        }
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (ChangeTracker.HasChanges())
        {
            foreach (var entity in ChangeTracker.Entries())
                if (entity.State is not EntityState.Unchanged && entity.Entity is MutableDbModel mutable)
                    if (entity.State is EntityState.Modified)
                        mutable.LastModifiedDate = DateTimeOffset.Now;
                    else if (entity.State is EntityState.Added)
                        mutable.CreationDate = mutable.LastModifiedDate = DateTimeOffset.Now;
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        ConfigureUser(mb.Entity<User>());
        ConfigureSession(mb.Entity<Session>());
        ConfigureLog(mb.Entity<ExecutionLogEntry>());
        ConfigureMailConfirmationRequest(mb.Entity<MailConfirmationRequest>());
        ConfigureServers(mb.Entity<Server>());
        ConfigurePokemonLists(mb.Entity<PokemonList>());
        ConfigurePokemonReferences(mb.Entity<PokemonReference>());
    }

    private static void ConfigurePokemonReferences(EntityTypeBuilder<PokemonReference> mb)
    {
        mb.Property(x => x.Id).ValueGeneratedOnAdd();
        mb.HasKey(x => x.Id);
        mb.HasOne(x => x.User).WithMany(x => x.VisitHistory).HasForeignKey(x => x.UserId);
        mb.HasOne(x => x.List).WithMany(x => x.Pokemon).HasForeignKey(x => x.ListId);
        //mb.ToTable(x => x.HasCheckConstraint("Check_IsRelated", "UserId != null OR ListId != null"));
    }

    private static void ConfigurePokemonLists(EntityTypeBuilder<PokemonList> mb)
    {
        mb.Property(x => x.Id).HasConversion(Id<PokemonList>.Converter);
        mb.HasKey(x => x.Id);
        mb.HasOne(x => x.User).WithMany(x => x.PokemonLists).HasForeignKey(x => x.UserId);
    }

    private static void ConfigureServers(EntityTypeBuilder<Server> mb)
    {
        mb.Property(x => x.Id).HasConversion(Id<Server>.Converter);
        mb.Property(x => x.HeartbeatInterval).HasConversion(Helper.TimeSpanToLongConverter);
        mb.HasKey(x => x.Id);
    }

    private static void ConfigureLog(EntityTypeBuilder<ExecutionLogEntry> mb)
    {
        mb.ToTable(nameof(ExecutionLog));
        mb.Property(x => x.UserId).HasConversion(Id<User>.Converter);
        mb.Property(x => x.SessionId).HasConversion(Id<Session>.Converter);
        mb.Property(x => x.Id).ValueGeneratedOnAdd();
    }

    private static void ConfigureMailConfirmationRequest(EntityTypeBuilder<MailConfirmationRequest> mb)
    {
        mb.Property(x => x.Id).HasConversion(Id<MailConfirmationRequest>.Converter);
    }

    private static void ConfigureUser(EntityTypeBuilder<User> mb)
    {
        mb.Property(x => x.Id).HasConversion(Id<User>.Converter);
        mb.HasMany(x => x.Sessions).WithOne(x => x.User).HasForeignKey(x => x.UserId);
        mb.HasOne(x => x.MailConfirmationRequest).WithOne(x => x.User).HasForeignKey<MailConfirmationRequest>(x => x.UserId);
    }
    
    private static void ConfigureSession(EntityTypeBuilder<Session> mb)
    {
        mb.Property(x => x.Id).HasConversion(Id<Session>.Converter);
        mb.Property(x => x.Expiration).HasConversion(Helper.TimeSpanToLongConverter);
    }
}
