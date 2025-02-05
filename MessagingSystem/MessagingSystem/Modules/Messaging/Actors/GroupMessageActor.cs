using Akka.Actor;
using MessagingSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MessagingSystem.Modules.Messaging.Actors
{
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

            foreach (var connectionId in connectionIds.ConnectionIds)
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
