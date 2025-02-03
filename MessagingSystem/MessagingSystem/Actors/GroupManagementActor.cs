﻿using Akka.Actor;

namespace MessagingSystem.Actors
{
	public record GetGroupActor(string GroupName);

	public record CreateGroupMessage(string GroupName, string? Bio, string? Avatar);
	public record CreateGroupResponseMessage(bool Success, string Message);

	public record ChangeGroupProfileMessage(string GroupName, string? Bio, string? Avatar);
	public record ChangeGroupProfileResponseMessage(bool Success, string Message);

	public record AddUserToGroupMessage(string GroupName, string UserName);
	public record AddUserToGroupResponseMessage(bool Success, string Message);

	public record GetGroupUsersMessage(string GroupName);
	public record GetGroupUsersResponseMessage(string[] UserNames);

	public record RemoveUserFromGroupMessage(string GroupName, string UserName);
	public record RemoveUserFromGroupResponseMessage(bool Success, string Message);

	// store list of groups
	public class GroupManagementActor : ReceiveActor
	{
		private readonly Dictionary<string, IActorRef> _groups;

		public GroupManagementActor()
		{
			_groups = new Dictionary<string, IActorRef>();

			Receive<CreateGroupMessage>(message => HandleCreateGroup(message));
			Receive<GetGroupActor>(message => HandleGetGroupActor(message));
			Receive<GetGroupUsersMessage>(message => HandleGetGroupUsers(message));
			Receive<ChangeGroupProfileMessage>(message => HandleChangeGroupProfile(message));
			Receive<AddUserToGroupMessage>(message => HandleAddUserToGroup(message));
			Receive<RemoveUserFromGroupMessage>(message => HandleDeleteFromGroup(message));
		}

		private void HandleGetGroupUsers(GetGroupUsersMessage message)
		{
			if(_groups.ContainsKey(message.GroupName))
			{
				_groups[message.GroupName].Forward(message);
			}
			else
			{
				Sender.Tell(new GetGroupUsersResponseMessage(new string[0]));
			}
		}

		private void HandleDeleteFromGroup(RemoveUserFromGroupMessage message)
		{
			if(_groups.ContainsKey(message.GroupName))
			{
				_groups[message.GroupName].Forward(message);
			}
			else
			{
				Sender.Tell(new RemoveUserFromGroupResponseMessage(false, "Group not found"));
			}
		}

		private void HandleAddUserToGroup(AddUserToGroupMessage message)
		{
			if(_groups.ContainsKey(message.GroupName))
			{
				_groups[message.GroupName].Forward(message);
			}
			else
			{
				Sender.Tell(new AddUserToGroupResponseMessage(false, "Group not found"));
			}
		}

		private void HandleChangeGroupProfile(ChangeGroupProfileMessage message)
		{
			if(_groups.ContainsKey(message.GroupName))
			{
				var group = _groups[message.GroupName];
				group.Tell(new ChangeGroupProfileMessage(message.GroupName, message.Bio, message.Avatar));

				Sender.Tell(new ChangeGroupProfileResponseMessage(true, "Group profile updated"));
			}

			Sender.Tell(new ChangeGroupProfileResponseMessage(false, "Group not found"));
		}

		private void HandleCreateGroup(CreateGroupMessage message)
		{
			if(!_groups.ContainsKey(message.GroupName))
			{
				_groups[message.GroupName] = Context.ActorOf(Props.Create(() => new GroupActor(message.GroupName, message.Bio, message.Avatar)), message.GroupName);

				Sender.Tell(new CreateGroupResponseMessage(true, "Group created"));
			}

			Sender.Tell(new CreateGroupResponseMessage(false, "Group already exists"));
		}

		private void HandleGetGroupActor(GetGroupActor message)
		{
			if(_groups.ContainsKey(message.GroupName))
			{
				Sender.Tell(_groups[message.GroupName]);
			}

			//Sender.Tell(null);
		}

	}

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
			if(_users.ContainsKey(message.UserName))
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
			if(!_users.ContainsKey(message.UserName))
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

			foreach(var user in _users)
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
