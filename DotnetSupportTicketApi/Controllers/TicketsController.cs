using DotnetSupportTicketApi.Data;
using DotnetSupportTicketApi.DTOs;
using DotnetSupportTicketApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetSupportTicketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
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
                return NotFound();
            }

            var response = new TicketResponse
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

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAll([FromQuery] GetTicketsRequest request)
        {
            IQueryable<Ticket> query = _dbContext.Tickets;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var normalizedStatus = request.Status.Trim();
                query = query.Where(t => t.Status == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(request.Priority))
            {
                var normalizedPriority = request.Priority.Trim();
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
                .Select(t => new TicketResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CreatedByUserId = t.CreatedByUserId,
                    AssignedToUserId = t.AssignedToUserId
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPost]
        public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request)
        {
            var createdByUserExists = await _dbContext.AppUsers
                .AnyAsync(u => u.Id == request.CreatedByUserId);

            if (!createdByUserExists)
            {
                return BadRequest($"CreatedByUserId {request.CreatedByUserId} does not exist.");
            }

            if (request.AssignedToUserId.HasValue)
            {
                var assignedUserExists = await _dbContext.AppUsers
                    .AnyAsync(u => u.Id == request.AssignedToUserId.Value);

                if (!assignedUserExists)
                {
                    return BadRequest($"AssignedToUserId {request.AssignedToUserId.Value} does not exist.");
                }
            }

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = string.IsNullOrWhiteSpace(request.Priority)
                    ? "Medium"
                    : request.Priority.Trim(),
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId,
                AssignedToUserId = request.AssignedToUserId
            };

            _dbContext.Tickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            var response = new TicketResponse
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

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, response);
        }
    }
}
