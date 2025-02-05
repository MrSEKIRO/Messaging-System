using Akka.Actor;

namespace MessagingSystem.Modules.Messaging.Actors
{
    public record SendDirectMessage(string SenderUserName, string RecipientUserName, string Content);
    public record SendGroupMessage(string SenderUserName, string GroupUserName, string Contenet);


    public record GetUserConnectionIdsMessage();
    public record GetUserConnectionIdsResponseMessage(string[] ConnectionIds);

    public record GetGroupConnectionIdsMessage();
    public record GetGroupConnectionIdsResponseMessage(string[] ConnectionIds);

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
}
