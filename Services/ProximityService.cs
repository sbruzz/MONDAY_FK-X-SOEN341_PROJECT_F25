using CampusEvents.Data;
using CampusEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Services;

/// <summary>
/// Service for proximity calculations and carpool eligibility.
/// Provides geographic distance calculations and carpool matching based on location.
/// </summary>
/// <remarks>
/// This service uses the Haversine formula to calculate great-circle distances
/// between two points on Earth's surface using their latitude and longitude coordinates.
/// 
/// Key Features:
/// - Distance calculation between geographic coordinates
/// - Driver eligibility checking based on proximity
/// - Passenger eligibility checking with nearby offer matching
/// - Nearby offer discovery sorted by distance
/// - Travel time estimation
/// - Distance formatting for display
/// 
/// The service supports optional geocoding (placeholder implementation) for converting
/// addresses to coordinates. In production, this would integrate with a geocoding API
/// such as Google Maps, OpenStreetMap, or similar services.
/// 
/// Proximity calculations are used to:
/// - Match passengers with nearby carpool offers
/// - Filter carpool offers by distance
/// - Estimate travel times
/// - Improve user experience by showing relevant offers
/// </remarks>
public class ProximityService
{
    /// <summary>
    /// Database context for accessing carpool offers and related data.
    /// </summary>
    private readonly AppDbContext _context;
    
    /// <summary>
    /// Earth's radius in kilometers.
    /// Used in Haversine formula for distance calculations.
    /// Average radius: 6,371 km
    /// </summary>
    private const double EarthRadiusKm = 6371.0;
    
    /// <summary>
    /// Default proximity threshold in kilometers.
    /// Carpool offers within this distance are considered "nearby".
    /// Default: 10 km
    /// </summary>
    private const double DefaultProximityThresholdKm = 10.0;

    /// <summary>
    /// Initializes a new instance of ProximityService.
    /// </summary>
    /// <param name="context">Database context for accessing carpool data</param>
    public ProximityService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculate distance between two points using Haversine formula
    /// Returns distance in kilometers
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Convert degrees to radians
        var lat1Rad = DegreesToRadians(lat1);
        var lon1Rad = DegreesToRadians(lon1);
        var lat2Rad = DegreesToRadians(lat2);
        var lon2Rad = DegreesToRadians(lon2);

        // Haversine formula
        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Check if user is eligible to be a driver for an event
    /// Based on proximity to event and driver profile
    /// </summary>
    public async Task<(bool Eligible, string Reason, double? Distance)> CheckDriverEligibilityAsync(
        int userId,
        int eventId,
        double? userLatitude = null,
        double? userLongitude = null,
        double proximityThresholdKm = DefaultProximityThresholdKm)
    {
        // Check if user has an active driver profile
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (driver == null)
            return (false, "User is not registered as a driver", null);

        if (driver.Status != DriverStatus.Active)
            return (false, $"Driver account status: {driver.Status}", null);

        // Check if user already has an offer for this event
        var existingOffer = await _context.CarpoolOffers
            .FirstOrDefaultAsync(co => co.DriverId == driver.Id &&
                                      co.EventId == eventId &&
                                      co.Status == CarpoolOfferStatus.Active);

        if (existingOffer != null)
            return (false, "You already have an active carpool offer for this event", null);

        // If coordinates not provided, skip proximity check
        if (!userLatitude.HasValue || !userLongitude.HasValue)
            return (true, "Eligible (proximity check skipped)", null);

        // Get event coordinates (would need to be added to Event model or geocoded)
        var eventData = await _context.Events.FindAsync(eventId);
        if (eventData == null)
            return (false, "Event not found", null);

        // For now, assume event location doesn't have coordinates
        // In full implementation, you would geocode the event address
        // Returning eligible without distance check
        return (true, "Eligible (event coordinates not available)", null);
    }

    /// <summary>
    /// Check if user is eligible to be a passenger for an event
    /// Based on available carpool offers nearby
    /// </summary>
    public async Task<(bool Eligible, string Reason, List<CarpoolOffer>? NearbyOffers)> CheckPassengerEligibilityAsync(
        int userId,
        int eventId,
        double? userLatitude = null,
        double? userLongitude = null,
        double proximityThresholdKm = DefaultProximityThresholdKm)
    {
        // Get active carpool offers for the event
        var offers = await _context.CarpoolOffers
            .Include(co => co.Driver)
                .ThenInclude(d => d.User)
            .Where(co => co.EventId == eventId &&
                        co.Status == CarpoolOfferStatus.Active &&
                        co.SeatsAvailable > 0)
            .ToListAsync();

        if (offers.Count == 0)
            return (false, "No carpool offers available for this event", null);

        // Check if user is already in a carpool for this event
        var existingParticipation = await _context.CarpoolPassengers
            .AnyAsync(cp => cp.PassengerId == userId &&
                           cp.Offer.EventId == eventId &&
                           cp.Status == PassengerStatus.Confirmed);

        if (existingParticipation)
            return (false, "You are already in a carpool for this event", null);

        // If coordinates not provided, return all available offers
        if (!userLatitude.HasValue || !userLongitude.HasValue)
            return (true, $"{offers.Count} carpool(s) available", offers);

        // Filter offers by proximity
        var nearbyOffers = new List<CarpoolOffer>();

        foreach (var offer in offers)
        {
            if (offer.Latitude.HasValue && offer.Longitude.HasValue)
            {
                var distance = CalculateDistance(
                    userLatitude.Value,
                    userLongitude.Value,
                    offer.Latitude.Value,
                    offer.Longitude.Value);

                if (distance <= proximityThresholdKm)
                {
                    nearbyOffers.Add(offer);
                }
            }
            else
            {
                // Include offers without coordinates
                nearbyOffers.Add(offer);
            }
        }

        if (nearbyOffers.Count == 0)
            return (false, $"No carpool offers within {proximityThresholdKm}km", null);

        return (true, $"{nearbyOffers.Count} nearby carpool(s) available", nearbyOffers);
    }

    /// <summary>
    /// Get nearby carpool offers sorted by distance
    /// </summary>
    public async Task<List<(CarpoolOffer Offer, double? Distance)>> GetNearbyOffersAsync(
        int eventId,
        double userLatitude,
        double userLongitude,
        double maxDistanceKm = DefaultProximityThresholdKm)
    {
        var offers = await _context.CarpoolOffers
            .Include(co => co.Driver)
                .ThenInclude(d => d.User)
            .Include(co => co.Passengers)
            .Where(co => co.EventId == eventId &&
                        co.Status == CarpoolOfferStatus.Active &&
                        co.SeatsAvailable > 0)
            .ToListAsync();

        var offersWithDistance = new List<(CarpoolOffer Offer, double? Distance)>();

        foreach (var offer in offers)
        {
            if (offer.Latitude.HasValue && offer.Longitude.HasValue)
            {
                var distance = CalculateDistance(
                    userLatitude,
                    userLongitude,
                    offer.Latitude.Value,
                    offer.Longitude.Value);

                if (distance <= maxDistanceKm)
                {
                    offersWithDistance.Add((offer, distance));
                }
            }
            else
            {
                // Include offers without coordinates (distance unknown)
                offersWithDistance.Add((offer, null));
            }
        }

        // Sort by distance (null distances last)
        return offersWithDistance
            .OrderBy(x => x.Distance ?? double.MaxValue)
            .ToList();
    }

    /// <summary>
    /// Simple geocoding placeholder
    /// In production, would use Google Maps API, OpenStreetMap, or similar
    /// </summary>
    public async Task<(double? Latitude, double? Longitude)> GeocodeAddressAsync(string address)
    {
        // Placeholder - in real implementation, call geocoding API
        // For now, return null to skip proximity checks

        // Example hardcoded coordinates for testing:
        // Concordia University: 45.4972, -73.5789
        // McGill University: 45.5048, -73.5772

        await Task.CompletedTask; // Simulate async operation

        // Return null for now
        return (null, null);
    }

    /// <summary>
    /// Batch geocode multiple addresses
    /// </summary>
    public async Task<Dictionary<string, (double? Latitude, double? Longitude)>> GeocodeAddressesBatchAsync(List<string> addresses)
    {
        var results = new Dictionary<string, (double? Latitude, double? Longitude)>();

        foreach (var address in addresses)
        {
            var coords = await GeocodeAddressAsync(address);
            results[address] = coords;
        }

        return results;
    }

    /// <summary>
    /// Calculate estimated travel time based on distance
    /// Assumes average city driving speed of 40 km/h
    /// </summary>
    public TimeSpan EstimateTravelTime(double distanceKm, double averageSpeedKmh = 40.0)
    {
        var hours = distanceKm / averageSpeedKmh;
        return TimeSpan.FromHours(hours);
    }

    /// <summary>
    /// Format distance for display
    /// </summary>
    public string FormatDistance(double distanceKm)
    {
        if (distanceKm < 1.0)
            return $"{(int)(distanceKm * 1000)}m";

        return $"{distanceKm:F1}km";
    }
}
