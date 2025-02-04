using Akka.Actor;

namespace MessagingSystem.Modules.Messaging.Actors
{
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
}
