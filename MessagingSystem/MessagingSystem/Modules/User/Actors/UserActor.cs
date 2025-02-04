using Akka.Actor;
using MessagingSystem.Modules.Messaging.Actors;

namespace MessagingSystem.Modules.User.Actors
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

	public record UserConnectedMessage(string UserName, string ConnectionId);
    public record UserDisconnectedMessage(string UserName, string ConnectionId);


    public class UserActor : ReceiveActor
    {
        protected string _userName;
        protected string _email;
        protected string _password;
        protected DateTime _lastSeen;

        protected bool IsOnline => _connectionIds.Count > 0;

        protected List<string> _connectionIds;

        public UserActor(string userName, string email, string password)
        {
            _connectionIds = new();

            _userName = userName;
            _email = email;
            _password = password;
            _lastSeen = DateTime.Now;

            Receive<LoginMessage>(message => HandleLogin(message));
            Receive<CheckIsOnlineMessage>(message => HandleCheckIsOnline(message));
            Receive<ChangeProfileMessage>(message => HandleChangeProfile(message));
            Receive<GetUserProfileMessage>(message => HandleGetUserProfile(message));
            Receive<LogoutMessage>(message => HandleLogout(message));

            //Receive<UserConnectedMessage>(message => _isOnline = true);
            //Receive<UserDisconnectedMessage>(message => _isOnline = false);

            Receive<UserConnectedMessage>(message => HandleConnected(message));
            Receive<UserDisconnectedMessage>(message => HandleDisconnected(message));

            Receive<GetUserConnectionIdsMessage>(message => Sender.Tell(new GetUserConnectionIdsResponseMessage(_connectionIds.ToArray())));
        }

        private void HandleDisconnected(UserDisconnectedMessage message)
        {
            _connectionIds.Remove(message.ConnectionId);
            if (_connectionIds.Count == 0)
            {
                _lastSeen = DateTime.Now;
            }
        }

        private void HandleConnected(UserConnectedMessage message)
        {
            _connectionIds.Add(message.ConnectionId);
            _lastSeen = DateTime.Now;
        }

        private void HandleLogout(LogoutMessage message)
        {
            //IsOnline = false;
            Sender.Tell(new LogoutResponseMessage(true, "Logged out"));
        }

        private void HandleGetUserProfile(GetUserProfileMessage message)
        {
            if (message.UserName == _userName)
            {
                Sender.Tell(new GetUserProfileResponseMessage(true, _userName, _email, _connectionIds.Count > 0, _lastSeen));
            }
            else
            {
                Sender.Tell(new GetUserProfileResponseMessage(true, string.Empty, string.Empty, false, DateTime.Now));
            }
        }

        private void HandleChangeProfile(ChangeProfileMessage message)
        {
            if (message.UserName == _userName)
            {
                _email = message.Email ?? _email;
                _password = message.Password ?? _password;
            }

            Sender.Tell(new ChangeProfileResponseMessage(true, "Profile updated"));
        }

        private void HandleCheckIsOnline(CheckIsOnlineMessage message)
        {
            Sender.Tell(new IsOnlineResponseMessage(IsOnline));
        }

        private void HandleLogin(LoginMessage message)
        {
            if (message.UserName == _userName && message.Password == _password)
            {
                //IsOnline = true;
                Sender.Tell(new LoginResponseMessage(true, "Login successful", $"Access Token here"));
            }
            else
            {
                Sender.Tell(new LoginResponseMessage(false, "Invalid credentials", string.Empty));
            }
        }

        public static Props Props(string name, string email, string password) =>
                Akka.Actor.Props.Create(() => new UserActor(name, email, password));
    }
}
