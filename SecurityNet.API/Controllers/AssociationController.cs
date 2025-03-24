using Microsoft.AspNetCore.Mvc;
using SecurityNet.Application.Associations;
using SecurityNet.Application.Associations.DataTransferObjects;

namespace SecurityNet.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AssociationController : ControllerBase {
    private readonly IAssociationService _associationService;
    private readonly ILogger<AssociationController> _logger;

    public AssociationController(IAssociationService associationService, ILogger<AssociationController> logger) {
        _associationService = associationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssociationDto>>> GetAssociations() {
        _logger.LogInformation("Getting associations");

        try {
            List<AssociationDto> associations = await _associationService.GetAssociations();
            return Ok(associations);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to get associations");
            return StatusCode(500, "Internal server error");
        }
    }
}
