namespace PuppetFestApp.web.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string SM = "SM";
    public const string FOH = "FOH";
    public const string Driver = "Driver"
    
    public static readonly string[] All = [Admin, SM, FOH, Driver];
    public static readonly string[] EditRoles = [Admin, SM];
    public static readonly string[] Sales Roles = [Admin, SM, FOH];
    public static readonly string[] DeliveryCheckRoles= [Admin, SM, FOH, Driver];
    public static readonly string[] UserManagementRoles = [Admin];

    public static bool IsKnownRole(string? role) => All.Contains(role);

}