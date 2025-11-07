public class CustomClaimsDto
{
    public string Role { get; set; }
}

public class LoginDto
{
    public string Username { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }

    //public Dictionary<string, object> CustomClaims { get; set; }
    public CustomClaimsDto CustomClaims { get; set; }
}
