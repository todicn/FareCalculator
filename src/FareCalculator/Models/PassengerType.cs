namespace FareCalculator.Models;

/// <summary>
/// Defines the types of passengers for fare calculation purposes, each with different discount eligibility.
/// </summary>
public enum PassengerType
{
    /// <summary>
    /// Adult passenger (18+ years) with no discount eligibility.
    /// </summary>
    Adult,

    /// <summary>
    /// Child passenger (typically under 12 years) eligible for significant discounts.
    /// </summary>
    Child,

    /// <summary>
    /// Senior citizen passenger (typically 65+ years) eligible for age-based discounts.
    /// </summary>
    Senior,

    /// <summary>
    /// Student passenger with valid student identification, eligible for educational discounts.
    /// </summary>
    Student,

    /// <summary>
    /// Passenger with disabilities, eligible for accessibility-based discounts.
    /// </summary>
    Disabled
} 