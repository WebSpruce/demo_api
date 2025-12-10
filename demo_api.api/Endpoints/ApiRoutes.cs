namespace demo_api.api.Endpoints;

public class ApiRoutes
{
    private const string Base = "/api";
    private const string Version = "v1";
    private const string ApiBase = $"{Base}/{Version}";

    public static class Users
    {
        public const string GroupName = $"{ApiBase}/users";
    }
    public static class Invoices
    {
        public const string GroupName = $"{ApiBase}/invoices";
    }
    public static class Products
    {
        public const string GroupName = $"{ApiBase}/products";
    }
    public static class Clients
    {
        public const string GroupName = $"{ApiBase}/clients";
    }
    public static class InvoiceItems
    {
        public const string GroupName = $"{ApiBase}/invoiceitems";
    }
    public static class Companies
    {
        public const string GroupName = $"{ApiBase}/companies";
    }
   
}