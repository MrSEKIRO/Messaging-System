using Akka.Actor;
using Akka.Hosting;
using Carter;
using MessagingSystem.Modules.Messaging.Actors;

namespace MessagingSystem.Modules.User.Endpoints
{
    public class MessagingModuleEndpoints : CarterModule
	{
		public MessagingModuleEndpoints() : base("api/messaging")
		{ }

		public override void AddRoutes(IEndpointRouteBuilder app)
		{
			// api for send direct message
			app.MapPost("/sendDirectMessage", async (HttpContext context, SendDirectMessage sendDirectMessage, ActorRegistry registry) =>
			{
				var messageRouterActor = registry.Get<MessageRouterActor>();
				messageRouterActor.Tell(sendDirectMessage);
				await context.Response.WriteAsJsonAsync(new { Success = true });
			});

			// api for send group message
			app.MapPost("/sendGroupMessage", async (HttpContext context, SendGroupMessage sendGroupMessage, ActorRegistry registry) =>
			{
				var messageRouterActor = registry.Get<MessageRouterActor>();
				messageRouterActor.Tell(sendGroupMessage);
				await context.Response.WriteAsJsonAsync(new { Success = true });
			});
		}
	}
}
