using Akka.Actor;

namespace MessagingSystem.Modules.Messaging.Actors
{
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
}
