using Akka.Actor;
using MessagingSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MessagingSystem.Modules.Messaging.Actors
{
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

            foreach (var connectionId in connectionIds.ConnectionIds)
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
}
