using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.DocumentService.CQRS.Commands
{
    public record UploadDocumentCommand(
        string FileName,
        string ContentType,
        long FileSize,
        byte[] FileContent,
        string EntityType,
        int EntityId,
        string DocumentType,
        int UploadedBy
    ) : IRequest<DocumentDto>;

    public record DeleteDocumentCommand(int Id, int DeletedBy) : IRequest<bool>;

    public record UpdateDocumentCommand(
        int Id,
        string DocumentType,
        string? Notes = null
    ) : IRequest<DocumentDto>;
}
