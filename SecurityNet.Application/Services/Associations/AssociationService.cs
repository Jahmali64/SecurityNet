using Microsoft.EntityFrameworkCore;
using SecurityNet.Application.Services.Associations.DataTransferObjects;
using SecurityNet.Domain.Entities;
using SecurityNet.Infrastructure.DbContexts;

namespace SecurityNet.Application.Services.Associations;

public interface IAssociationService {
    Task<List<AssociationDto>> GetAssociations();
    Task<AssociationDto?> GetAssociation(int associationId);
    Task<AssociationDto> AddAssociation(CreateAssociationDto request);
    Task<int> UpdateAssociation(int associationId, CreateAssociationDto request);
    Task<int> DeleteAssociation(int associationId);
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

        return await securityNetDbContext.Associations.Where(a => !a.Trash).Select(a => new AssociationDto {
            AssociationId = a.AssociationId,
            Name = a.Name ?? string.Empty,
            Website = a.Website ?? string.Empty,
            Active = a.Active
        }).ToListAsync(_cancellationToken);
    }

    public async Task<AssociationDto?> GetAssociation(int associationId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Associations.Where(a => a.AssociationId == associationId && !a.Trash).Select(a => new AssociationDto {
            AssociationId = a.AssociationId,
            Name = a.Name ?? string.Empty,
            Website = a.Website ?? string.Empty,
            Active = a.Active
        }).FirstOrDefaultAsync(_cancellationToken);
    }

    public async Task<AssociationDto> AddAssociation(CreateAssociationDto request) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        Association association = new() {
            Name = request.Name,
            Website = request.Website,
            Active = request.Active,
            Trash = false,
            CreatedAt = DateTime.Now
        };

        await securityNetDbContext.Associations.AddAsync(association, _cancellationToken);
        await securityNetDbContext.SaveChangesAsync(_cancellationToken);

        return new AssociationDto {
            AssociationId = association.AssociationId,
            Name = association.Name,
            Website = association.Website,
            Active = association.Active
        };
    }

    public async Task<int> UpdateAssociation(int associationId, CreateAssociationDto request) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        return await securityNetDbContext.Associations
            .Where(a => a.AssociationId == associationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Name, request.Name)
                .SetProperty(a => a.Website, request.Website)
                .SetProperty(a => a.Active, request.Active)
                .SetProperty(a => a.UpdatedAt, DateTime.Now), _cancellationToken);
    }

    public async Task<int> DeleteAssociation(int associationId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        return await securityNetDbContext.Associations
            .Where(a => a.AssociationId == associationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Trash, true)
                .SetProperty(a => a.DeletedAt, DateTime.Now), _cancellationToken);
    }
}
