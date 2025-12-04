using Gov2Biz.DocumentService.Data;
using Gov2Biz.Shared.Models;
using Gov2Biz.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gov2Biz.DocumentService.CQRS.Handlers
{
    public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
    {
        private readonly DocumentDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public UploadDocumentHandler(DocumentDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var filePath = await _fileStorage.SaveFileAsync(request.FileName, request.ContentType, request.FileContent, cancellationToken);

            var document = new Document
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                FilePath = filePath,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                DocumentType = request.DocumentType,
                UploadedBy = request.UploadedBy,
                UploadedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return await MapToDto(document);
        }

        private async Task<DocumentDto> MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                DocumentType = document.DocumentType,
                EntityType = document.EntityType,
                EntityId = document.EntityId,
                UploadedAt = document.UploadedAt,
                UploadedByName = $"User {document.UploadedBy}"
            };
        }
    }

    public class GetDocumentHandler : IRequestHandler<GetDocumentQuery, DocumentDto>
    {
        private readonly DocumentDbContext _context;

        public GetDocumentHandler(DocumentDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentDto> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

            if (document == null)
                throw new KeyNotFoundException($"Document with ID {request.Id} not found");

            return await MapToDto(document);
        }

        private async Task<DocumentDto> MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                DocumentType = document.DocumentType,
                EntityType = document.EntityType,
                EntityId = document.EntityId,
                UploadedAt = document.UploadedAt,
                UploadedByName = $"User {document.UploadedBy}"
            };
        }
    }

    public class GetDocumentsHandler : IRequestHandler<GetDocumentsQuery, Gov2Biz.Shared.Responses.PagedResult<DocumentDto>>
    {
        private readonly DocumentDbContext _context;

        public GetDocumentsHandler(DocumentDbContext context)
        {
            _context = context;
        }

        public async Task<Gov2Biz.Shared.Responses.PagedResult<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Documents.Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(request.EntityType))
                query = query.Where(d => d.EntityType == request.EntityType);

            if (request.EntityId.HasValue)
                query = query.Where(d => d.EntityId == request.EntityId.Value);

            if (!string.IsNullOrEmpty(request.DocumentType))
                query = query.Where(d => d.DocumentType == request.DocumentType);

            if (request.UploadedBy.HasValue)
                query = query.Where(d => d.UploadedBy == request.UploadedBy.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = new List<DocumentDto>();
            foreach (var document in documents)
            {
                dtos.Add(await MapToDto(document));
            }

            return new Gov2Biz.Shared.Responses.PagedResult<DocumentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private async Task<DocumentDto> MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                DocumentType = document.DocumentType,
                EntityType = document.EntityType,
                EntityId = document.EntityId,
                UploadedAt = document.UploadedAt,
                UploadedByName = $"User {document.UploadedBy}"
            };
        }
    }

    public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, bool>
    {
        private readonly DocumentDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DeleteDocumentHandler(DocumentDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

            if (document == null)
                throw new KeyNotFoundException($"Document with ID {request.Id} not found");

            document.IsDeleted = true;
            document.UpdatedAt = DateTime.UtcNow;

            _context.Documents.Update(document);
            await _context.SaveChangesAsync(cancellationToken);

            await _fileStorage.DeleteFileAsync(document.FilePath, cancellationToken);

            return true;
        }
    }

    public class DownloadDocumentHandler : IRequestHandler<DownloadDocumentQuery, DocumentDownloadDto>
    {
        private readonly DocumentDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DownloadDocumentHandler(DocumentDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<DocumentDownloadDto> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

            if (document == null)
                throw new KeyNotFoundException($"Document with ID {request.Id} not found");

            var content = await _fileStorage.GetFileAsync(document.FilePath, cancellationToken);

            return new DocumentDownloadDto
            {
                FileName = document.FileName,
                ContentType = document.ContentType,
                Content = content
            };
        }
    }

    public class GetEntityDocumentsHandler : IRequestHandler<GetEntityDocumentsQuery, List<DocumentDto>>
{
    private readonly DocumentDbContext _context;

    public GetEntityDocumentsHandler(DocumentDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentDto>> Handle(GetEntityDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Documents.Where(d => !d.IsDeleted && 
                                               d.EntityType == request.EntityType && 
                                               d.EntityId == request.EntityId);

        if (!string.IsNullOrEmpty(request.DocumentType))
            query = query.Where(d => d.DocumentType == request.DocumentType);

        var documents = await query.OrderByDescending(d => d.UploadedAt).ToListAsync(cancellationToken);

        var dtos = new List<DocumentDto>();
        foreach (var document in documents)
        {
            dtos.Add(await MapToDto(document));
        }

        return dtos;
    }

    private async Task<DocumentDto> MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DocumentType = document.DocumentType,
            EntityType = document.EntityType,
            EntityId = document.EntityId,
            UploadedAt = document.UploadedAt,
            UploadedByName = $"User {document.UploadedBy}"
        };
    }
}
}
