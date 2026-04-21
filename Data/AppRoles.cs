// File: Data/AppRoles.cs
//
// PURPOSE: Defines all application roles as compile-time constants.
// Using constants instead of raw strings prevents typos and keeps all
// role-based access rules consistent across pages, navigation, and seeding.

namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Central registry of all role names used in the application.
///
/// Permission hierarchy:
/// Admin > SM > FOH > Driver
///
/// Driver has the smallest permission set. Any Driver-level capability is
/// also available to FOH, SM, and Admin.
/// </summary>
public static class AppRoles
{
    /// <summary>Full system access, including user management.</summary>
    public const string Admin = "Admin";

    /// <summary>Stage manager / operations manager. Can edit operational data but cannot manage users.</summary>
    public const string SM = "SM";

    /// <summary>Front of house. Can view inventory/status data and enter sales.</summary>
    public const string FOH = "FOH";

    /// <summary>Driver / delivery role. Can view stock/location/status data and complete box/delivery checks.</summary>
    public const string Driver = "Driver";

    /// <summary>All current application roles in hierarchy order.</summary>
    public static readonly string[] AllRoles =
    [
        Admin,
        SM,
        FOH,
        Driver
    ];

    /// <summary>Roles that can view product, stock, location, and delivery status information.</summary>
    public static readonly string[] ViewProductRoles =
    [
        Admin,
        SM,
        FOH,
        Driver
    ];

    /// <summary>Roles that can create, edit, or delete product/inventory records.</summary>
    public static readonly string[] EditProductRoles =
    [
        Admin,
        SM
    ];

    /// <summary>Roles that can enter sales.</summary>
    public static readonly string[] SalesInputRoles =
    [
        Admin,
        SM,
        FOH
    ];

    /// <summary>Roles that can perform box and delivery checks.</summary>
    public static readonly string[] DeliveryCheckRoles =
    [
        Admin,
        SM,
        FOH,
        Driver
    ];

    /// <summary>Roles that can manage application users and roles.</summary>
    public static readonly string[] UserManagementRoles =
    [
        Admin
    ];
}
