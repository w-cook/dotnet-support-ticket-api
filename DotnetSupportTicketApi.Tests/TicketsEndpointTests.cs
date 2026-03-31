using System.Net.Http.Json;
using Azure;
using DotnetSupportTicketApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotnetSupportTicketApi.Tests
{
    public class TicketsEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TicketsEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostTickets_WithValidRequest_CreatesTicket()
        {
            var request = new CreateTicketRequest
            {
                Title = "Integration test ticket",
                Description = "Created from an integration test.",
                Priority = "High",
                CreatedByUserId = 1,
                AssignedToUserId = 2
            };

            var response = await _client.PostAsJsonAsync(
                "/api/tickets", 
                request);
            response.EnsureSuccessStatusCode();

            var createdTicket = await response.Content.ReadFromJsonAsync<TicketResponse>();

            Assert.NotNull(createdTicket);
            Assert.True(createdTicket!.Id > 0);
            Assert.Equal("Integration test ticket", createdTicket.Title);
            Assert.Equal("Created from an integration test.", createdTicket.Description);
            Assert.Equal("High", createdTicket.Priority);
            Assert.Equal("Open", createdTicket.Status);
            Assert.Equal(1, createdTicket.CreatedByUserId);
            Assert.Equal(2, createdTicket.AssignedToUserId);
        }

        [Fact]
        public async Task PatchTicketStatus_WithValidRequest_UpdatesStatusAndCreatesHistory()
        {
            var createRequest = new CreateTicketRequest
            {
                Title = "Status workflow test ticket",
                Description = "Created by integration test.",
                Priority = "Medium",
                CreatedByUserId = 1,
                AssignedToUserId = 2
            };

            var createResponse = await _client.PostAsJsonAsync(
                "/api/tickets", 
                createRequest);
            createResponse.EnsureSuccessStatusCode();

            var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponse>();

            Assert.NotNull(createdTicket);
            Assert.True(createdTicket!.Id > 0);
            Assert.Equal("Open", createdTicket.Status);

            var updateRequest = new UpdateTicketStatusRequest
            {
                Status = "Resolved"
            };

            var patchResponse = await _client.PatchAsJsonAsync(
                $"/api/tickets/{createdTicket.Id}/status", 
                updateRequest);
            patchResponse.EnsureSuccessStatusCode();

            var updatedTicket = await patchResponse.Content.ReadFromJsonAsync<TicketResponse>();

            Assert.NotNull(updatedTicket);
            Assert.Equal(createdTicket.Id, updatedTicket!.Id);
            Assert.Equal("Resolved", updatedTicket.Status);

            var historyResponse = await _client.GetAsync($"/api/tickets/{createdTicket.Id}/history");
            historyResponse.EnsureSuccessStatusCode();

            var historyEntries = await historyResponse.Content.ReadFromJsonAsync<List<StatusHistoryResponse>>();

            Assert.NotNull(historyEntries);
            Assert.NotEmpty(historyEntries);

            var matchingEntry = historyEntries.FirstOrDefault(h =>
                h.TicketId == createdTicket.Id &&
                h.OldStatus == "Open" &&
                h.NewStatus == "Resolved");

            Assert.NotNull(matchingEntry);
        }

        [Fact]
        public async Task PostComment_WithValidRequest_CreatesCommentAndCanRetrieve()
        {
            var createRequest = new CreateTicketRequest
            {
                Title = "Comment creation and retrieval workflow test ticket",
                Description = "Created by integration test.",
                Priority = "Medium",
                CreatedByUserId = 1,
                AssignedToUserId = 2
            };

            var createResponse = await _client.PostAsJsonAsync(
                "/api/tickets", 
                createRequest);
            createResponse.EnsureSuccessStatusCode();

            var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketResponse>();

            Assert.NotNull(createdTicket);
            Assert.True(createdTicket!.Id > 0);
            Assert.Equal("Open", createdTicket.Status);

            var postRequest = new CreateCommentRequest
            {
                AuthorUserId = 2,
                Body = "Comment created by integration test."
            };

            var postResponse = await _client.PostAsJsonAsync(
                $"/api/tickets/{createdTicket.Id}/comments", 
                postRequest);
            postResponse.EnsureSuccessStatusCode();

            var postedComment = await postResponse.Content.ReadFromJsonAsync<CommentResponse>();

            Assert.NotNull(postedComment);
            Assert.True(postedComment.Id > 0);
            Assert.Equal(createdTicket.Id, postedComment.TicketId);
            Assert.Equal(2, postedComment.AuthorUserId);
            Assert.Equal("Comment created by integration test.", postedComment.Body);

            var getResponse = await _client.GetAsync($"/api/tickets/{createdTicket.Id}/comments");
            getResponse.EnsureSuccessStatusCode();

            var commentEntries = await getResponse.Content.ReadFromJsonAsync<List<CommentResponse>>();

            Assert.NotNull(commentEntries);
            Assert.NotEmpty(commentEntries);

            var matchingEntry = commentEntries.FirstOrDefault(c =>
                c.Id == postedComment.Id &&
                c.TicketId == createdTicket.Id &&
                c.AuthorUserId == 2 &&
                c.Body == "Comment created by integration test.");

            Assert.NotNull(matchingEntry);
        }
    }
}