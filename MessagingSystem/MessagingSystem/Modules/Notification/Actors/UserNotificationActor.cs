using Akka.Actor;

namespace MessagingSystem.Modules.Notification.Actors
{
	public class UserNotificationActor : ReceiveActor
	{
		private readonly IServiceProvider _serviceProvider;

		public UserNotificationActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			Receive<SendNotificationToUserMessage>(message => HandleSendNotificationToUser(message));
		}

		private void HandleSendNotificationToUser(SendNotificationToUserMessage message)
		{
			throw new NotImplementedException();
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new UserNotificationActor(serviceProvider));
	}
}
