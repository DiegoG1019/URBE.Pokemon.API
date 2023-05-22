namespace URBE.Pokemon.API.Models.Requests;

public record class NewUserRequest(string Username, string Email, string Password);
