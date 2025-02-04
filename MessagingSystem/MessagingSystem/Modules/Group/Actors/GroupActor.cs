using Akka.Actor;
using MessagingSystem.Modules.Messaging.Actors;

namespace MessagingSystem.Modules.Group.Actors
{
    public class GroupActor : ReceiveActor
    {
        protected string _name;
        protected string? _bio;
        protected string? _avatarUrl;

        private readonly Dictionary<string, IActorRef> _users;

        public GroupActor(string name, string? bio, string? avatarUrl)
        {
            _users = new();

            _name = name;
            _bio = bio;
            _avatarUrl = avatarUrl;

            Receive<AddUserToGroupMessage>(message => HandleAddUserToGroup(message));
            Receive<GetGroupConnectionIdsMessage>(message => GetGroupConnectionIds(message));
            Receive<ChangeGroupProfileMessage>(message => HandleChangeGroupProfile(message));
            Receive<GetGroupUsersMessage>(message => HandleGetGroupUsers(message));
            Receive<RemoveUserFromGroupMessage>(message => HandleRemoveUserFromGroup(message));
        }

        private void HandleRemoveUserFromGroup(RemoveUserFromGroupMessage message)
        {
            if (_users.ContainsKey(message.UserName))
            {
                _users.Remove(message.UserName);

                Sender.Tell(new RemoveUserFromGroupResponseMessage(true, "User removed from group"));
            }

            Sender.Tell(new RemoveUserFromGroupResponseMessage(false, "User not in group"));
        }

        private void HandleGetGroupUsers(GetGroupUsersMessage message)
        {
            Sender.Tell(new GetGroupUsersResponseMessage(_users.Keys.ToArray()));
        }

        private void HandleChangeGroupProfile(ChangeGroupProfileMessage message)
        {
            _bio = message.Bio ?? _bio;
            _avatarUrl = message.Avatar ?? _avatarUrl;

            Sender.Tell(new ChangeGroupProfileResponseMessage(true, "Group profile updated"));
        }

        private void HandleAddUserToGroup(AddUserToGroupMessage message)
        {
            if (!_users.ContainsKey(message.UserName))
            {
                //TODO: check user exists
                var user = Context.ActorSelection($"/user/user-management/user-{message.UserName}");
                _users[message.UserName] = user.ResolveOne(TimeSpan.FromSeconds(5)).Result;

                Sender.Tell(new AddUserToGroupResponseMessage(true, "User added to group"));
            }

            Sender.Tell(new AddUserToGroupResponseMessage(false, "User already in group"));
        }

        private void GetGroupConnectionIds(GetGroupConnectionIdsMessage message)
        {
            var connectionIds = new List<string>();

            foreach (var user in _users)
            {
                var userConnectionIds = user.Value.Ask<GetUserConnectionIdsResponseMessage>(new GetUserConnectionIdsMessage()).Result;

                connectionIds.AddRange(userConnectionIds.ConnectionIds);
            }

            Sender.Tell(new GetGroupConnectionIdsResponseMessage(connectionIds.ToArray()));
        }

        public static Props Props(string name, string? bio, string? avatarUrl) =>
                Akka.Actor.Props.Create(() => new GroupActor(name, bio, avatarUrl));
    }
}
