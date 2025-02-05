using Akka.Actor;

namespace MessagingSystem.Modules.Notification.Actors
{
	public class OTPActor : ReceiveActor
	{
		private string _otpCode;
		private readonly IServiceProvider _serviceProvider;

		public OTPActor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

			Receive<SendOTPMessage>(message => HandleSendOTP(message));
			Receive<CheckOTPCodeMessage>(message => HandleCheckOTPCode(message));
		}

		private void HandleCheckOTPCode(CheckOTPCodeMessage message)
		{
			throw new NotImplementedException();
		}

		private void HandleSendOTP(SendOTPMessage message)
		{
			throw new NotImplementedException();
		}

		public static Props Props(IServiceProvider serviceProvider) =>
				Akka.Actor.Props.Create(() => new OTPActor(serviceProvider));
	}
}
