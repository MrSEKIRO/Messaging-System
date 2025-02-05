using Akka.Actor;
using Akka.Hosting;
using Carter;
using MessagingSystem.Modules.Group.Actors;

namespace MessagingSystem.Modules.Group.Endpoints
{
    public class GroupModuleEndpoints : CarterModule
	{
        public GroupModuleEndpoints() : base("api/group")
        {
            
        }
        public override void AddRoutes(IEndpointRouteBuilder app)
		{
			// api for create group
			app.MapPost("/createGroup", async (HttpContext context, CreateGroupMessage createGroupMessage, ActorRegistry registry) =>
			{
				var groupManagementActor = registry.Get<GroupManagementActor>();
				var resp = await groupManagementActor.Ask<CreateGroupResponseMessage>(createGroupMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for change group profile
			app.MapPost("/changeGroupProfile", async (HttpContext context, ChangeGroupProfileMessage changeGroupProfileMessage, ActorRegistry registry) =>
			{
				var groupManagementActor = registry.Get<GroupManagementActor>();
				var resp = await groupManagementActor.Ask<ChangeGroupProfileResponseMessage>(changeGroupProfileMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for add user to group
			app.MapPost("/addUserToGroup", async (HttpContext context, AddUserToGroupMessage addUserToGroupMessage, ActorRegistry registry) =>
			{
				var groupManagementActor = registry.Get<GroupManagementActor>();
				var resp = await groupManagementActor.Ask<AddUserToGroupResponseMessage>(addUserToGroupMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for remove user from group
			app.MapPost("/removeUserFromGroup", async (HttpContext context, RemoveUserFromGroupMessage removeUserFromGroupMessage, ActorRegistry registry) =>
			{
				var groupManagementActor = registry.Get<GroupManagementActor>();
				var resp = await groupManagementActor.Ask<RemoveUserFromGroupResponseMessage>(removeUserFromGroupMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for get group users
			app.MapPost("/getGroupUsers", async (HttpContext context, GetGroupUsersMessage getGroupUsersMessage, ActorRegistry registry) =>
			{
				var groupManagementActor = registry.Get<GroupManagementActor>();
				var resp = await groupManagementActor.Ask<GetGroupUsersResponseMessage>(getGroupUsersMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});
		}
	}
}
