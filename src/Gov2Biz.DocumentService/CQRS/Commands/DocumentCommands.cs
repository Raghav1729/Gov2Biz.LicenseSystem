using Gov2Biz.Shared.DTOs;
using MediatR;

namespace Gov2Biz.DocumentService.CQRS.Commands
{
    public record UpdateDocumentCommand(
        int Id,
        string DocumentType,
        string? Notes = null
    ) : IRequest<DocumentDto>;
}
