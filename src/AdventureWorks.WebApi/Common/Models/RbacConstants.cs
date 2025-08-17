namespace AdventureWorks.WebApi.Common.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string ReadOnly = "ReadOnly";
}

public static class Policies
{
    // Employee policies
    public const string ReadEmployees = "RequireReadEmployeesPermission";
    public const string CreateEmployees = "RequireCreateEmployeesPermission";
    public const string UpdateEmployees = "RequireUpdateEmployeesPermission";
    public const string DeleteEmployees = "RequireDeleteEmployeesPermission";

    // Admin policies
    public const string AdminOnly = "RequireAdminRole";
    public const string ManagerOnly = "RequireManagerRole";
}