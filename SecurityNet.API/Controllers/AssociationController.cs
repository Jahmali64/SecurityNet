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
    [ProducesResponseType(typeof(List<AssociationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AssociationDto>>> GetAssociations() {
        _logger.LogInformation("Getting associations");

        try {
            List<AssociationDto> associations = await _associationService.GetAssociations();
            return Ok(associations);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to get associations. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AssociationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssociationDto>> GetAssociation(int id) {
        _logger.LogInformation("Getting association with Id {id}", id);

        if (id < 1) {
            return BadRequest("Invalid id");
        }

        try {
            AssociationDto? association = await _associationService.GetAssociation(id);
            if (association is not null) return Ok(association);
            
            _logger.LogInformation("Association not found");
            return NotFound();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to get association with Id {id}. Message: {message}", id, ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssociationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssociationDto>> PostAssociation([FromBody] CreateAssociationDto? request) {
        _logger.LogInformation("Creating association");

        if (request is null) {
            _logger.LogInformation("request is null");
            return BadRequest("Invalid request");
        }

        if (!ModelState.IsValid) {
            _logger.LogInformation("request is invalid");
            return BadRequest(ModelState);
        }

        try {
            AssociationDto association = await _associationService.AddAssociation(request);
            return CreatedAtAction(nameof(GetAssociation), new { id = association.AssociationId }, association);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to add association. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutAssociation(int id, [FromBody] CreateAssociationDto? request) {
        _logger.LogInformation("Updating association with Id {id}", id);

        if (id < 1) {
            _logger.LogInformation("Invalid id: {id}", id);
            return BadRequest("Invalid id");
        }

        if (request is null) {
            _logger.LogInformation("request is null");
            return BadRequest("Invalid request");
        }

        if (!ModelState.IsValid) {
            _logger.LogInformation("request is invalid");
            return BadRequest(ModelState);
        }

        try {
            int rowsAffected = await _associationService.UpdateAssociation(id, request);
            if (rowsAffected > 0) return NoContent();

            _logger.LogInformation("Association not found");
            return NotFound();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to update association. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAssociation(int id) {
        _logger.LogInformation("Deleting association with Id {id}", id);

        if (id < 1) {
            _logger.LogInformation("Invalid id: {id}", id);
            return BadRequest("Invalid id");
        }

        try {
            int rowsAffected = await _associationService.DeleteAssociation(id);
            if (rowsAffected > 0) return NoContent();

            _logger.LogInformation("Association with id {id} not found", id);
            return NotFound();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to delete association. Message: {message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
