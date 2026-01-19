using System.Text.Json.Serialization;

public class LoginResponse
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
