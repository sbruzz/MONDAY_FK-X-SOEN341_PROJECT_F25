using Xunit;
using Microsoft.Extensions.Configuration;
using CampusEvents.Services;

namespace CampusEvents.Tests;

public class TicketSigningServiceTests
{
    private TicketSigningService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Security:TicketSigningKey"] = "test-key-that-is-at-least-32-characters-long-for-security"
            })
            .Build();

        return new TicketSigningService(config);
    }

    [Fact]
    public void SignTicket_ShouldCreateValidToken()
    {
        // Arrange
        var service = CreateService();
        var eventId = 1;
        var ticketId = 100;
        var uniqueCode = "test-code-123";
        var eventDate = DateTime.UtcNow.AddDays(1);

        // Act
        var signedToken = service.SignTicket(eventId, ticketId, uniqueCode, eventDate);

        // Assert
        Assert.NotNull(signedToken);
        Assert.NotEmpty(signedToken);
    }

    [Fact]
    public void VerifyTicket_ValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var service = CreateService();
        var eventId = 1;
        var ticketId = 100;
        var uniqueCode = "test-code-123";
        var eventDate = DateTime.UtcNow.AddDays(1);
        var signedToken = service.SignTicket(eventId, ticketId, uniqueCode, eventDate);

        // Act
        var result = service.VerifyTicket(signedToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Payload);
        Assert.Equal(eventId, result.Payload.EventId);
        Assert.Equal(ticketId, result.Payload.TicketId);
        Assert.Equal(uniqueCode, result.Payload.UniqueCode);
    }

    [Fact]
    public void VerifyTicket_TamperedToken_ShouldReturnFailure()
    {
        // Arrange
        var service = CreateService();
        var eventId = 1;
        var ticketId = 100;
        var uniqueCode = "test-code-123";
        var eventDate = DateTime.UtcNow.AddDays(1);
        var signedToken = service.SignTicket(eventId, ticketId, uniqueCode, eventDate);

        // Tamper with the signature (replace last character)
        var tamperedToken = signedToken.Substring(0, signedToken.Length - 3) + "XXX";

        // Act
        var result = service.VerifyTicket(tamperedToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void VerifyTicket_ExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var service = CreateService();
        var eventId = 1;
        var ticketId = 100;
        var uniqueCode = "test-code-123";
        var eventDate = DateTime.UtcNow.AddDays(-2); // Event was 2 days ago (token expires 24h after)
        var signedToken = service.SignTicket(eventId, ticketId, uniqueCode, eventDate);

        // Act
        var result = service.VerifyTicket(signedToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("expired", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShortKey_ShouldThrowException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Security:TicketSigningKey"] = "short-key" // Less than 32 chars
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TicketSigningService(config));
    }

    [Fact]
    public void Constructor_MissingKey_ShouldThrowException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TicketSigningService(config));
    }
}
