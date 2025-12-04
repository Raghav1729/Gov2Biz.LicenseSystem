using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.DocumentService.CQRS.Queries
{
    public record GetDocumentQuery(int Id) : IRequest<DocumentDto>;
    
    public record GetDocumentsQuery(
        string? EntityType = null,
        int? EntityId = null,
        string? DocumentType = null,
        int? UploadedBy = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Gov2Biz.Shared.Responses.PagedResult<DocumentDto>>;

    public record GetEntityDocumentsQuery(
        string EntityType,
        int EntityId,
        string? DocumentType = null
    ) : IRequest<List<DocumentDto>>;

    public record DownloadDocumentQuery(int Id) : IRequest<DocumentDownloadDto>;
}

public class DocumentDownloadDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
