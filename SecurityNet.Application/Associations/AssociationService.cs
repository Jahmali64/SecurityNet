using Microsoft.EntityFrameworkCore;
using SecurityNet.Application.Associations.DataTransferObjects;
using SecurityNet.Infrastructure.DbContexts;

namespace SecurityNet.Application.Associations;

public interface IAssociationService {
    Task<List<AssociationDto>> GetAssociations();
    Task<AssociationDto?> GetAssociation(int associationId);
}

public sealed class AssociationService : IAssociationService {
    private readonly IDbContextFactory<SecurityNetDbContext> _securityNetDbContextFactory;
    private readonly CancellationToken _cancellationToken;

    public AssociationService(IDbContextFactory<SecurityNetDbContext> securityNetDbContextFactory, CancellationToken cancellationToken) {
        _securityNetDbContextFactory = securityNetDbContextFactory;
        _cancellationToken = cancellationToken;
    }

    public async Task<List<AssociationDto>> GetAssociations() {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Associations.Where(a => a.Active && !a.Trash).Select(a => new AssociationDto {
            AssociationId = a.AssociationId,
            Name = a.Name ?? string.Empty,
            Website = a.Website ?? string.Empty,
            Active = a.Active
        }).ToListAsync(_cancellationToken);
    }

    public async Task<AssociationDto?> GetAssociation(int associationId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        return await securityNetDbContext.Associations.Where(a => a.AssociationId == associationId && a.Active && !a.Trash).Select(a => new AssociationDto {
            AssociationId = a.AssociationId,
            Name = a.Name ?? string.Empty,
            Website = a.Website ?? string.Empty,
            Active = a.Active
        }).FirstOrDefaultAsync(_cancellationToken);
    }
}
