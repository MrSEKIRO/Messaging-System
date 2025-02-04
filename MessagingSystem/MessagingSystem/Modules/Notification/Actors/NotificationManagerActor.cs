using Akka.Actor;

namespace MessagingSystem.Modules.Notification.Actors
{
	public record SendNotificationToUserMessage(string SenderUserName, string RecipientUserName, string Content);

	public record SendNotificationToGroupMessage(string SenderUserName, string GroupName, string Content);

	public record SendOTPMessage(string UserName);
	public record CheckOTPCodeMessage(string UserName, string OTPCode);

	public class NotificationManagerActor : ReceiveActor
	{
		private readonly IServiceProvider _serviceProvider;

		public NotificationManagerActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			Receive<SendNotificationToUserMessage>(message => HandleSendNotificationToUser(message));
			Receive<SendNotificationToGroupMessage>(message => HandleSendNotificationToGroup(message));
		}

		private void HandleSendNotificationToGroup(SendNotificationToGroupMessage message)
		{
			throw new NotImplementedException();
		}

		private void HandleSendNotificationToUser(SendNotificationToUserMessage message)
		{
			throw new NotImplementedException();
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new NotificationManagerActor(serviceProvider));
	}
}
