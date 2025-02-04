using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Carter;
using MessagingSystem.Configs;
using MessagingSystem.Hubs;
using MessagingSystem.Modules.Group.Actors;
using MessagingSystem.Modules.Messaging.Actors;
using MessagingSystem.Modules.Notification.Actors;
using MessagingSystem.Modules.User.Actors;
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

			// notification actor
			var notificationActor = system.ActorOf(Props.Create(() => new NotificationManagerActor(serviceProvider)), "notification-management");
			registry.Register<NotificationManagerActor>(notificationActor);
		});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCarter();
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

app.MapCarter();

// add swagger for API documentation
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging_System v1"));

await app.RunAsync();
