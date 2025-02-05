
using Akka.Actor;
using Akka.Hosting;
using MessagingSystem.Modules.Messaging.Actors;
using MessagingSystem.Modules.User.Actors;
using Microsoft.AspNetCore.SignalR;

namespace MessagingSystem.Hubs
{
	public class ChatHub : Hub
	{
		private readonly IActorRef _messageRouterActor;
		private readonly IActorRef _userManagementActor;

		public ChatHub(ActorRegistry registry)
		{
			_messageRouterActor = registry.Get<MessageRouterActor>();
			_userManagementActor = registry.Get<UserManagementActor>();
		}

		public override async Task OnConnectedAsync()
		{
			//var userName = Context.User?.Claims?.FirstOrDefault(x => x.Type == "username")?.Value;
			var userName = Context.GetHttpContext()?.Request.Query["username"];

			if(userName.HasValue == true)
			{
				// get actor ref of user
				var userActor = _userManagementActor.Ask<IActorRef>(new GetUserActor(userName)).Result;
				if(userActor != null)
				{
					// send online message
					userActor.Tell(new UserConnectedMessage(userName, Context.ConnectionId));
				}
			}

			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			//var userName = Context.User?.Claims?.FirstOrDefault(x => x.Type == "username")?.Value;
			var userName = Context.GetHttpContext()?.Request.Query["username"];

			if(userName.HasValue == true)
			{
				// get actor ref of user
				var userActor = _userManagementActor.Ask<IActorRef>(new GetUserActor(userName)).Result;
				if(userActor != null)
				{
					// send online message
					userActor.Tell(new UserDisconnectedMessage(userName, Context.ConnectionId));
				}
			}

			await base.OnDisconnectedAsync(exception);
		}
	}
}
