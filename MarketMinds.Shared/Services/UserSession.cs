public static class UserSession
{
    public static int? CurrentUserId { get; set; } // Set this after successful login
    public static string? CurrentUserRole { get; set; } // Set this after successful login
}