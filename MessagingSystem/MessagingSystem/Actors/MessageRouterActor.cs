using Akka.Actor;
using MessagingSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MessagingSystem.Actors
{
	public record SendDirectMessage(string SenderUserName, string RecipientUserName, string Content);
	public record SendGroupMessage(string SenderUserName, string GroupUserName, string Contenet);

	//public record ReceiveDirectMessage(string SenderUserName, string RecipientUserName, string Content);
	//public record ReceiveGroupMessage(string SenderUserName, string GroupUserName, string Content);

	public record GetUserConnectionIdsMessage();
	public record GetUserConnectionIdsResponseMessage(string[] ConnectionIds);

	public record GetGroupConnectionIdsMessage();
	public record GetGroupConnectionIdsResponseMessage(string[] ConnectionIds);


	public record DeliverMessage(string SenderUserName, string Content);

	public class MessageRouterActor : ReceiveActor
	{
		private readonly IActorRef _directMessageActor;
		private readonly IActorRef _groupMessageActor;

		public MessageRouterActor(IServiceProvider serviceProvider)
		{
			_directMessageActor = Context.ActorOf(Props.Create(() => new DirectMessageManagerActor(serviceProvider)), "directMessage");
			_groupMessageActor = Context.ActorOf(Props.Create(() => new GroupMessageManagerActor(serviceProvider)), "groupMessage");

			Receive<SendDirectMessage>(msg =>
			{
				_directMessageActor.Tell(msg);
			});

			Receive<SendGroupMessage>(msg =>
			{
				_groupMessageActor.Tell(msg);
			});
		}
	}

	public class DirectMessageManagerActor : ReceiveActor
	{
		private readonly IServiceProvider _serviceProvider;

		//private readonly IActorRef _messageStoreActor;
		//private readonly IActorRef _userManager;

		public DirectMessageManagerActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			//_messageStoreActor = Context.ActorOf(Props.Create(() => new MessageStoreActor()), "messageStore");

			Receive<SendDirectMessage>(message => HandleSendDirectMessage(message));
		}

		private void HandleSendDirectMessage(SendDirectMessage message)
		{
			// create a DirectMessageActor
			var actorNameList = new List<string>() { message.SenderUserName, message.RecipientUserName };
			actorNameList.Sort();
			var actorName = string.Join('-', actorNameList);

			var directMessageActor = Context.ActorOf(DirectMessageActor.Props(_serviceProvider), $"directMessage-{actorName}");

			// send message with the actor
			directMessageActor.Tell(message);
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new DirectMessageManagerActor(serviceProvider));
	}

	public class DirectMessageActor : ReceiveActor
	{
		private readonly IHubContext<ChatHub> _hubContext;

		public DirectMessageActor(IServiceProvider serviceProvider)
		{
			_hubContext = serviceProvider.GetRequiredService<IHubContext<ChatHub>>();

			Receive<SendDirectMessage>(message => HandleSendDirectMessage(message));
		}

		private void HandleSendDirectMessage(SendDirectMessage message)
		{
			Console.WriteLine($"Direct Message from {message.SenderUserName} to {message.RecipientUserName}: {message.Content}");

			// get reference of user actor
			var userActor = Context.ActorSelection($"/user/user-management/user-{message.RecipientUserName}");

			var connectionIds = userActor.Ask<GetUserConnectionIdsResponseMessage>(new GetUserConnectionIdsMessage()).Result;

			foreach(var connectionId in connectionIds.ConnectionIds)
			{
				_hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message.SenderUserName, message.Content);
			}

			// Store the message
			//_messageStoreActor.Tell(new StoreMessage(msg.SenderId, msg.ReceiverId, msg.Content, false));

			// terminate the actor
			Context.Stop(Self);
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new DirectMessageActor(serviceProvider));
	}

	public class GroupMessageManagerActor : ReceiveActor
	{
		private readonly IServiceProvider _serviceProvider;

		public GroupMessageManagerActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			Receive<SendGroupMessage>(message => HandleSendGroupMessage(message));
		}

		private void HandleSendGroupMessage(SendGroupMessage message)
		{
			// create a GroupMessageActor
			var groupMessageActor = Context.ActorOf(GroupMessageActor.Props(_serviceProvider), $"groupMessage-{message.GroupUserName}");

			// send message with the actor
			groupMessageActor.Tell(message);
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new GroupMessageManagerActor(serviceProvider));
	}

	public class GroupMessageActor : ReceiveActor
	{
		private readonly IHubContext<ChatHub> _hubContext;

		public GroupMessageActor(IServiceProvider serviceProvider)
		{
			_hubContext = serviceProvider.GetRequiredService<IHubContext<ChatHub>>();

			Receive<SendGroupMessage>(message => HandleSendGroupMessage(message));
		}

		private void HandleSendGroupMessage(SendGroupMessage message)
		{
			Console.WriteLine($"Group Message from {message.SenderUserName} to {message.GroupUserName}: {message.Contenet}");

			// get reference of group actor
			var groupActor = Context.ActorSelection($"/user/group-management/{message.GroupUserName}");

			var connectionIds = groupActor.Ask<GetGroupConnectionIdsResponseMessage>(new GetGroupConnectionIdsMessage()).Result;

			foreach(var connectionId in connectionIds.ConnectionIds)
			{
				_hubContext.Clients.Client(connectionId).SendAsync("ReceiveGroupMessage", message.SenderUserName, message.Contenet);
			}

			// Store the message
			//_messageStoreActor.Tell(new StoreMessage(msg.SenderId, msg.ReceiverId, msg.Content, true));

			// terminate the actor
			Context.Stop(Self);
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new GroupMessageActor(serviceProvider));
	}
}
