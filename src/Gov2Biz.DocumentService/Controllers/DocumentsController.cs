using Microsoft.AspNetCore.Mvc;
using MediatR;
using Gov2Biz.DocumentService.CQRS.Commands;
using Gov2Biz.DocumentService.CQRS.Queries;
using Gov2Biz.Shared.Responses;

namespace Gov2Biz.DocumentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        public async Task<ApiResponse<DocumentDto>> Upload([FromForm] UploadDocumentRequest request)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await request.File.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var command = new UploadDocumentCommand(
                    request.File.FileName,
                    request.File.ContentType,
                    request.File.Length,
                    fileBytes,
                    request.EntityType,
                    request.EntityId,
                    request.DocumentType,
                    request.UploadedBy
                );

                var result = await _mediator.Send(command);
                return new ApiResponse<DocumentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<DocumentDto>> GetDocument(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetDocumentQuery(id));
                return new ApiResponse<DocumentDto> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentDto> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet]
        public async Task<ApiResponse<PagedResult<DocumentDto>>> GetDocuments(
            [FromQuery] string? entityType = null,
            [FromQuery] int? entityId = null,
            [FromQuery] string? documentType = null,
            [FromQuery] int? uploadedBy = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _mediator.Send(new GetDocumentsQuery(entityType, entityId, documentType, uploadedBy, pageNumber, pageSize));
                return new ApiResponse<PagedResult<DocumentDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<DocumentDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<ApiResponse<List<DocumentDto>>> GetEntityDocuments(string entityType, int entityId, [FromQuery] string? documentType = null)
        {
            try
            {
                var result = await _mediator.Send(new GetEntityDocumentsQuery(entityType, entityId, documentType));
                return new ApiResponse<List<DocumentDto>> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DocumentDto>> { Success = false, Message = ex.Message };
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var result = await _mediator.Send(new DownloadDocumentQuery(id));
                return File(result.Content, result.ContentType, result.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse<bool>> DeleteDocument(int id, [FromQuery] int deletedBy)
        {
            try
            {
                var result = await _mediator.Send(new DeleteDocumentCommand(id, deletedBy));
                return new ApiResponse<bool> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }

    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
    }
}
