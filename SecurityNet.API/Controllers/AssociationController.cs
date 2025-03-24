using Microsoft.AspNetCore.Mvc;
using SecurityNet.Application.Associations;
using SecurityNet.Application.Associations.DataTransferObjects;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class AssociationController : ControllerBase {
    private readonly IAssociationService _associationService;
    private readonly ILogger<AssociationController> _logger;

    public AssociationController(IAssociationService associationService, ILogger<AssociationController> logger) {
        _associationService = associationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<List<AssociationDto>> GetAssociations() {
        List<AssociationDto> result = await _associationService.GetAssociations();

        _logger.LogInformation("Returning associations for {count}", result.Count);
        return result;
    }
}
