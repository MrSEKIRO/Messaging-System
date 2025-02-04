using Akka.Actor;

namespace MessagingSystem.Modules.Notification.Actors
{
	public class GroupNotificationActor : ReceiveActor
	{
		private readonly IServiceProvider _serviceProvider;

		public GroupNotificationActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			Receive<SendNotificationToGroupMessage>(message => HandleSendNotificationToGroup(message));
		}

		private void HandleSendNotificationToGroup(SendNotificationToGroupMessage message)
		{
			throw new NotImplementedException();
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new GroupNotificationActor(serviceProvider));
	}
}
