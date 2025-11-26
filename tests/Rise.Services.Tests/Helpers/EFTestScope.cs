using Microsoft.EntityFrameworkCore.Storage;
using Rise.Persistence;

namespace Rise.Services.Tests.Helpers;

public class EfTestScope : IAsyncDisposable
{
    public ApplicationDbContext DbContext { get; }
    public IDbContextTransaction Transaction { get; }

    private EfTestScope(ApplicationDbContext db, IDbContextTransaction transaction)
    {
        DbContext = db;
        Transaction = transaction;
    }
    public static async Task<EfTestScope> CreateScope(EFFixture fixture)
    {
        var db = fixture.CreateApplicationDbContext();
        var trans = await db.Database.BeginTransactionAsync();

        return new EfTestScope(db, trans);
    }
    public async ValueTask DisposeAsync()
    {
        await Transaction.RollbackAsync();
        await Transaction.DisposeAsync();
        await DbContext.DisposeAsync();
    }
}