using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using MessagingSystem.Actors;
using MessagingSystem.Configs;
using MessagingSystem.Hubs;
using MessagingSystem.Messages;
using Microsoft.OpenApi.Models;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Cluster.Sharding;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;
using System.Diagnostics;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
	.AddJsonFile("appsettings.json")
	.AddJsonFile($"appsettings.{environment}.json", optional: true)
	.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddConsole();

var akkaConfig = builder.Configuration.GetRequiredSection(nameof(AkkaClusterConfig))
	.Get<AkkaClusterConfig>();

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddAkka(akkaConfig.ActorSystemName, (builder, provider) =>
{
	Debug.Assert(akkaConfig.Port != null, "akkaConfig.Port != null");

	builder.AddHoconFile("app.conf", HoconAddMode.Append)
		.WithRemoting(akkaConfig.Hostname, akkaConfig.Port.Value)
		.WithClustering(new ClusterOptions()
		{
			Roles = akkaConfig.Roles,
			SeedNodes = akkaConfig.SeedNodes,
		})
		.AddPetabridgeCmd(cmd =>
		{
			cmd.RegisterCommandPalette(new RemoteCommands());
			cmd.RegisterCommandPalette(ClusterCommands.Instance);

			// sharding commands, although the app isn't configured to host any by default
			cmd.RegisterCommandPalette(ClusterShardingCommands.Instance);
		})
		.WithActors((system, registry) =>
		{
			var serviceProvider = provider.GetRequiredService<IServiceProvider>();

			// user management actor
			var userManagementActor = system.ActorOf(Props.Create(() => new UserManagementActor()), "user-management");
			registry.Register<UserManagementActor>(userManagementActor);

			// group management actor
			var groupManagementActor = system.ActorOf(Props.Create(() => new GroupManagementActor()), "group-management");
			registry.Register<GroupManagementActor>(groupManagementActor);

			// message router actor
			var messageRouterActor = system.ActorOf(Props.Create(() => new MessageRouterActor(serviceProvider)), "message-router");
			registry.Register<MessageRouterActor>(messageRouterActor);
		});
});

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Messaging_System", Version = "v1" });
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.MapHub<ChatHub>("/chat");

app.MapGet("/", async (HttpContext context, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();

	var signUpMessage = new SignUpMessage("jason", "jason@gmail.com", "pass");
	var resp = await userManagementActor.Ask<SignUpResponseMessage>(signUpMessage, context.RequestAborted);

	signUpMessage = new SignUpMessage("arshi", "arshi@gmail.com", "pass");
	resp = await userManagementActor.Ask<SignUpResponseMessage>(signUpMessage, context.RequestAborted);

	var groupManagementActor = registry.Get<GroupManagementActor>();

	var createGroupMessage = new CreateGroupMessage("g1", "group1 bio", "group1 avatar");
	var resp2 = await groupManagementActor.Ask<CreateGroupResponseMessage>(createGroupMessage, context.RequestAborted);

	var addUserToGroupMessage = new AddUserToGroupMessage("g1", "jason");
	var resp3 = await groupManagementActor.Ask<AddUserToGroupResponseMessage>(addUserToGroupMessage, context.RequestAborted);

	addUserToGroupMessage = new AddUserToGroupMessage("g1", "arshi");
	resp3 = await groupManagementActor.Ask<AddUserToGroupResponseMessage>(addUserToGroupMessage, context.RequestAborted);

	await context.Response.WriteAsJsonAsync(resp);
});

// api for sign-up
app.MapPost("/api/signup", async (HttpContext context, SignUpMessage signUpMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<SignUpResponseMessage>(signUpMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for login
app.MapPost("/api/login", async (HttpContext context, LoginMessage loginMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<LoginResponseMessage>(loginMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for checking if user is online
app.MapPost("/api/checkIsOnline", async (HttpContext context, CheckIsOnlineMessage checkIsOnlineMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<IsOnlineResponseMessage>(checkIsOnlineMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for changing profile
app.MapPost("/api/changeProfile", async (HttpContext context, ChangeProfileMessage changeProfileMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<ChangeProfileResponseMessage>(changeProfileMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for get user profile
app.MapPost("/api/getUserProfile", async (HttpContext context, GetUserProfileMessage getUserProfileMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<GetUserProfileResponseMessage>(getUserProfileMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for logout
app.MapPost("/api/logout", async (HttpContext context, LogoutMessage logoutMessage, ActorRegistry registry) =>
{
	var userManagementActor = registry.Get<UserManagementActor>();
	var resp = await userManagementActor.Ask<LogoutResponseMessage>(logoutMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for create group
app.MapPost("/api/createGroup", async (HttpContext context, CreateGroupMessage createGroupMessage, ActorRegistry registry) =>
{
	var groupManagementActor = registry.Get<GroupManagementActor>();
	var resp = await groupManagementActor.Ask<CreateGroupResponseMessage>(createGroupMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for change group profile
app.MapPost("/api/changeGroupProfile", async (HttpContext context, ChangeGroupProfileMessage changeGroupProfileMessage, ActorRegistry registry) =>
{
	var groupManagementActor = registry.Get<GroupManagementActor>();
	var resp = await groupManagementActor.Ask<ChangeGroupProfileResponseMessage>(changeGroupProfileMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for add user to group
app.MapPost("/api/addUserToGroup", async (HttpContext context, AddUserToGroupMessage addUserToGroupMessage, ActorRegistry registry) =>
{
	var groupManagementActor = registry.Get<GroupManagementActor>();
	var resp = await groupManagementActor.Ask<AddUserToGroupResponseMessage>(addUserToGroupMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for remove user from group
app.MapPost("/api/removeUserFromGroup", async (HttpContext context, RemoveUserFromGroupMessage removeUserFromGroupMessage, ActorRegistry registry) =>
{
	var groupManagementActor = registry.Get<GroupManagementActor>();
	var resp = await groupManagementActor.Ask<RemoveUserFromGroupResponseMessage>(removeUserFromGroupMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for get group users
app.MapPost("/api/getGroupUsers", async (HttpContext context, GetGroupUsersMessage getGroupUsersMessage, ActorRegistry registry) =>
{
	var groupManagementActor = registry.Get<GroupManagementActor>();
	var resp = await groupManagementActor.Ask<GetGroupUsersResponseMessage>(getGroupUsersMessage, context.RequestAborted);
	await context.Response.WriteAsJsonAsync(resp);
});

// api for send direct message
app.MapPost("/api/sendDirectMessage", async (HttpContext context, SendDirectMessage sendDirectMessage, ActorRegistry registry) =>
{
	var messageRouterActor = registry.Get<MessageRouterActor>();
	messageRouterActor.Tell(sendDirectMessage);
	await context.Response.WriteAsJsonAsync(new { Success = true });
});

// api for send group message
app.MapPost("/api/sendGroupMessage", async (HttpContext context, SendGroupMessage sendGroupMessage, ActorRegistry registry) =>
{
	var messageRouterActor = registry.Get<MessageRouterActor>();
	messageRouterActor.Tell(sendGroupMessage);
	await context.Response.WriteAsJsonAsync(new { Success = true });
});

// add swagger for API documentation
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging_System v1"));


await app.RunAsync();
