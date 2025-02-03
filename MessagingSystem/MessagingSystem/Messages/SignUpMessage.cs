namespace MessagingSystem.Messages
{
	public record SignUpMessage(string UserName, string Email, string Password);

	public record SignUpResponseMessage(bool Success, string Message);

	public record LoginMessage(string UserName, string Password);

	public record LoginResponseMessage(bool Success, string Message, string AccessToken);

	public record CheckIsOnlineMessage(string UserName = "");

	public record IsOnlineResponseMessage(bool IsOnline);

	public record ChangeProfileMessage(string UserName = "", string? Email = null, string? Password = null);

	public record ChangeProfileResponseMessage(bool Success, string Message);

	public record GetUserProfileMessage(string UserName = "");


	public record GetUserProfileResponseMessage(bool Success, string UserName, string Email, bool IsOnline, DateTime lastSeen);

	public record LogoutMessage(string UserName = "");

	public record LogoutResponseMessage(bool Success, string Message);
}
