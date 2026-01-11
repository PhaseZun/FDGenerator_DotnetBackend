using System.Text.Json.Serialization;

public class LoginResponse
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
