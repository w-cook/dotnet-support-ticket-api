using DotnetSupportTicketApi.Data;
using DotnetSupportTicketApi.DTOs;
using DotnetSupportTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DotnetSupportTicketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedPriorities = new(StringComparer.OrdinalIgnoreCase)
            {
                "Low",
                "Medium",
                "High",
                "Critical"
            };

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
            {
                "Open",
                "InProgress",
                "Resolved",
                "Closed"
            };

        private readonly AppDbContext _dbContext;

        public TicketsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketResponse>> GetById([FromRoute] int id)
        {
            var ticket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"TicketId '{id}' does not exist.");
            }

            return Ok(MapTicketToResponse(ticket));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll([FromQuery] GetTicketsRequest request)
        {
            IQueryable<Ticket> query = _dbContext.Tickets;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var normalizedStatus = NormalizeStatus(request.Status);

                if (!AllowedStatuses.Contains(normalizedStatus))
                {
                    return BadRequest($"Status '{normalizedStatus}' is not valid.");
                }

                query = query.Where(t => t.Status == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(request.Priority))
            {
                var normalizedPriority = NormalizePriority(request.Priority);

                if (!AllowedPriorities.Contains(normalizedPriority))
                {
                    return BadRequest($"Priority '{normalizedPriority}' is not valid.");
                }

                query = query.Where(t => t.Priority == normalizedPriority);
            }

            if (request.CreatedByUserId.HasValue)
            {
                query = query.Where(t => t.CreatedByUserId == request.CreatedByUserId.Value);
            }

            if (request.AssignedToUserId.HasValue)
            {
                query = query.Where(t => t.AssignedToUserId == request.AssignedToUserId.Value);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapTicketToResponse(t))
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("{id:int}/comments")]
        public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(
            [FromRoute] int id)
        {
            var ticket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"TicketId '{id}' does not exist.");
            }

            IQueryable<Comment> query = _dbContext.Comments;

            query = query.Where(t => t.TicketId == id);

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => MapCommentToResponse(c))
                .ToListAsync();

            return Ok(comments);
        }

        [HttpGet("{id:int}/history")]
        public async Task<ActionResult<IEnumerable<StatusHistoryResponse>>> GetHistory(
            [FromRoute] int id)
        {
            var ticket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"TicketId '{id}' does not exist.");
            }

            IQueryable<StatusHistory> query = _dbContext.StatusHistoryEntries;

            query = query.Where(h => h.TicketId == id);

            var historyEntries = await query
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => MapHistoryToResponse(h))
                .ToListAsync();

            return Ok(historyEntries);
        }

        [HttpPost]
        public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request)
        {
            var createdByUserExists = await _dbContext.AppUsers
                .AnyAsync(u => u.Id == request.CreatedByUserId);

            if (!createdByUserExists)
            {
                return BadRequest($"CreatedByUserId '{request.CreatedByUserId}' does not exist.");
            }

            if (request.AssignedToUserId.HasValue)
            {
                var assignedUserExists = await _dbContext.AppUsers
                    .AnyAsync(u => u.Id == request.AssignedToUserId.Value);

                if (!assignedUserExists)
                {
                    return BadRequest($"AssignedToUserId '{request.AssignedToUserId.Value}' does not exist.");
                }
            }

            var normalizedPriority = NormalizePriority(request.Priority);

            if (!AllowedPriorities.Contains(normalizedPriority))
            {
                return BadRequest($"Priority '{normalizedPriority}' is not valid.");
            }

            var ticket = new Ticket
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = normalizedPriority,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId,
                AssignedToUserId = request.AssignedToUserId
            };

            _dbContext.Tickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, MapTicketToResponse(ticket));
        }

        [HttpPost("{id:int}/comments")]
        public async Task<ActionResult<CommentResponse>> Create(
            [FromRoute] int id,
            CreateCommentRequest request)
        {
            var ticket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"TicketId '{id}' does not exist.");
            }

            var author = await _dbContext.AppUsers
                .FirstOrDefaultAsync(u => u.Id == request.AuthorUserId);

            if (author == null)
            {
                return BadRequest($"AuthorUserId '{request.AuthorUserId}' does not exist.");
            }

            var comment = new Comment
            {
                TicketId = ticket.Id,
                Ticket = ticket,
                AuthorUserId = author.Id,
                AuthorUser = author,
                Body = request.Body.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, MapCommentToResponse(comment));
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<TicketResponse>> UpdateStatus(
            [FromRoute] int id,
            UpdateTicketStatusRequest request)
        {
            var ticket = await _dbContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound($"TicketId '{id}' does not exist.");
            }

            var normalizedStatus = NormalizeStatus(request.Status);

            if (!AllowedStatuses.Contains(normalizedStatus))
            {
                return BadRequest($"Status '{normalizedStatus}' is not valid.");
            }

            if (string.Equals(ticket.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(MapTicketToResponse(ticket));
            }

            var oldStatus = ticket.Status;

            ticket.Status = normalizedStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            var historyEntry = new StatusHistory
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus,
                NewStatus = normalizedStatus,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.StatusHistoryEntries.Add(historyEntry);

            await _dbContext.SaveChangesAsync();

            return Ok(MapTicketToResponse(ticket));
        }

        private static TicketResponse MapTicketToResponse(Ticket ticket)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                CreatedByUserId = ticket.CreatedByUserId,
                AssignedToUserId = ticket.AssignedToUserId
            };
        }

        private static CommentResponse MapCommentToResponse(Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                AuthorUserId = comment.AuthorUserId,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt
            };
        }

        private static StatusHistoryResponse MapHistoryToResponse(StatusHistory entry)
        {
            return new StatusHistoryResponse
            {
                Id = entry.Id,
                TicketId = entry.TicketId,
                OldStatus = entry.OldStatus,
                NewStatus = entry.NewStatus,
                ChangedAt = entry.ChangedAt
            };
        }

        private static string NormalizePriority(string priority)
        {
            return priority.Trim().ToLowerInvariant() switch
            {
                "low" => "Low",
                "medium" => "Medium",
                "high" => "High",
                "critical" => "Critical",
                _ => priority
            };
        }

        private static string NormalizeStatus(string status)
        {
            return status.Trim().ToLowerInvariant() switch
            {
                "open" => "Open",
                "inprogress" => "InProgress",
                "resolved" => "Resolved",
                "closed" => "Closed",
                _ => status
            };
        }
    }
}
