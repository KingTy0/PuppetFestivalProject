// File: Data/ApplicationUser.cs
//
// PURPOSE: Represents a user account in the system.
// Inherits from IdentityUser which provides: Id, Email, UserName,
// PasswordHash, EmailConfirmed, and many other fields automatically.
// Add custom fields here later (e.g., DisplayName, StandAssignment).


using Microsoft.AspNetCore.Identity;

namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Application-specific user entity. Extends ASP.NET Core Identity's
/// built-in user class. All custom user properties (beyond email,
/// password, etc.) should be added as properties here. When you add
/// a property, create a new EF Core migration to update the database
/// schema: <c>dotnet ef migrations add AddMyNewField</c>.
/// </summary>

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
}

