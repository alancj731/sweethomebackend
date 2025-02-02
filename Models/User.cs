using System.ComponentModel.DataAnnotations;

public class User
{   
    [Key]
    public int Id { get; set; }

    // unique
    public required string Email { get; set; }

    // unique
    public string? UserName { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }
    public bool Verified { get; set; }
    public bool Approved { get; set; }

    public string StorageFolder { get; set; } = "";
}
