// File: Data/AppPolicies.cs
//
// PURPOSE: Centralizes authorization policy names so pages, navigation,
// and Program.cs can all reference the same policy constants.

namespace PuppetFestAPP.Web.Data;

public static class AppPolicies
{
    public const string CanViewProducts = "CanViewProducts";
    public const string CanEditProducts = "CanEditProducts";
    public const string CanEnterSales = "CanEnterSales";
    public const string CanManageDeliveryChecks = "CanManageDeliveryChecks";
    public const string CanManageUsers = "CanManageUsers";
}
