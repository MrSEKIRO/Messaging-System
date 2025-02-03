using Akka.Actor;
using MessagingSystem.Messages;

namespace MessagingSystem.Actors
{
	public record GetUserActor(string UserName);

	public class UserManagementActor : ReceiveActor
	{
		private readonly Dictionary<string, IActorRef> _users;
		public UserManagementActor()
		{
			_users = new Dictionary<string, IActorRef>();


			Receive<SignUpMessage>(message => HandleSignUp(message));

			Receive<LoginMessage>(message => HandleLogin(message));
			Receive<CheckIsOnlineMessage>(message => HandleCheckIsOnline(message));
			Receive<GetUserProfileMessage>(message => HandleGetUserProfile(message));
			Receive<ChangeProfileMessage>(message => HandleChangeProfile(message));
			Receive<LogoutMessage>(message => HandleLogout(message));
			//Receive<GetAllUsersMessage>(message => HandleGetAllUsers(message));

			Receive<GetUserActor>(message => HandleGetUserActor(message));
		}

		private void HandleGetUserActor(GetUserActor message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				Sender.Tell(_users[message.UserName]);
			}

			//Sender.Tell(null);
		}

		private void HandleLogout(LogoutMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				_users[message.UserName].Forward(message);
			}
			else
			{
				Sender.Tell(new LogoutResponseMessage(false, "User not found"));
			}
		}

		private void HandleGetUserProfile(GetUserProfileMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				_users[message.UserName].Forward(message);
			}
			else
			{
				Sender.Tell(new GetUserProfileResponseMessage(false, string.Empty, string.Empty, false, DateTime.Now));
			}
		}

		private void HandleChangeProfile(ChangeProfileMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				_users[message.UserName].Forward(message);
			}
			else
			{
				Sender.Tell(new ChangeProfileResponseMessage(false, "User not found"));
			}
		}

		private void HandleCheckIsOnline(CheckIsOnlineMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				_users[message.UserName].Forward(message);
			}
			else
			{
				Sender.Tell(new IsOnlineResponseMessage(false));
			}
		}

		private void HandleLogin(LoginMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				_users[message.UserName].Forward(message);
			}
			else
			{
				Sender.Tell(new LoginResponseMessage(false, "User not found", string.Empty));
			}
		}

		private void HandleSignUp(SignUpMessage message)
		{
			if(_users.ContainsKey(message.UserName))
			{
				Sender.Tell(new SignUpResponseMessage(false, "User with same username already exists"));
			}
			else
			{
				var userActor = Context.ActorOf(Props.Create<UserActor>(message.UserName, message.Email, message.Password), $"user-{message.UserName}");

				_users.Add(message.UserName, userActor);

				Sender.Tell(new SignUpResponseMessage(true, "User created"));
			}
		}
	}
}
