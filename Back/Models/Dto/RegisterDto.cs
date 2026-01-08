using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    [MinLength(5, ErrorMessage = "Username mora imati najmanje 5 karaktera")]
    public string Username { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera")]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Lozinka mora sadržati bar jedno slovo i jedan broj"
    )]
    public string Password { get; set; }

    [Required]
    [MinLength(2)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(2)]
    public string LastName { get; set; }

    [Required]
    public CustomClaimsDto CustomClaims { get; set; }
}
