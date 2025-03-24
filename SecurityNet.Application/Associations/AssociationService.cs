using Microsoft.EntityFrameworkCore;
using SecurityNet.Application.Associations.DataTransferObjects;
using SecurityNet.Infrastructure;
using SecurityNet.Infrastructure.DbContexts;

namespace SecurityNet.Application.Associations;

public interface IAssociationService {
    Task<List<AssociationDto>> GetAssociations();
    Task<AssociationDto?> GetAssociation(int associationId);
    Task<AssociationDto> AddAssociation(AssociationDto associationDto);
    Task<AssociationDto> UpdateAssociation(AssociationDto associationDto);
    Task DeleteAssociation(int associationId);
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

    public async Task<AssociationDto> AddAssociation(AssociationDto associationDto) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        Association association = new() {
            Name = associationDto.Name,
            Website = associationDto.Website,
            Active = associationDto.Active,
            Trash = false,
            CreatedAt = DateTime.Now
        };

        await securityNetDbContext.Associations.AddAsync(association, _cancellationToken);
        await securityNetDbContext.SaveChangesAsync(_cancellationToken);
        associationDto.AssociationId = association.AssociationId;

        return associationDto;
    }

    public async Task<AssociationDto> UpdateAssociation(AssociationDto associationDto) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);

        int rowsAffected = await securityNetDbContext.Associations
            .Where(a => a.AssociationId == associationDto.AssociationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Name, associationDto.Name)
                .SetProperty(a => a.Website, associationDto.Website)
                .SetProperty(a => a.Active, associationDto.Active)
                .SetProperty(a => a.UpdatedAt, DateTime.Now), _cancellationToken);
        
        if (rowsAffected == 0) throw new InvalidOperationException("No rows affected");
        return associationDto;
    }

    public async Task DeleteAssociation(int associationId) {
        await using SecurityNetDbContext securityNetDbContext = await _securityNetDbContextFactory.CreateDbContextAsync(_cancellationToken);
        
        int rowsAffected = await securityNetDbContext.Associations
            .Where(a => a.AssociationId == associationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Trash, true)
                .SetProperty(a => a.DeletedAt, DateTime.Now), _cancellationToken);
        
        if (rowsAffected == 0) throw new InvalidOperationException("No rows affected");
    }
}
