using Akka.Actor;
using Akka.Hosting;
using Carter;
using MessagingSystem.Modules.User.Actors;

namespace MessagingSystem.Modules.User.Endpoints
{
    public class UserModuleEndpoints : CarterModule
	{
        public UserModuleEndpoints(): base("api/user")
        {
				
        }
        public override void AddRoutes(IEndpointRouteBuilder app)
        {
			// api for sign-up
			app.MapPost("/signup", async (HttpContext context, SignUpMessage signUpMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<SignUpResponseMessage>(signUpMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for login
			app.MapPost("/login", async (HttpContext context, LoginMessage loginMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<LoginResponseMessage>(loginMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for checking if user is online
			app.MapPost("/checkIsOnline", async (HttpContext context, CheckIsOnlineMessage checkIsOnlineMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<IsOnlineResponseMessage>(checkIsOnlineMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for changing profile
			app.MapPost("/changeProfile", async (HttpContext context, ChangeProfileMessage changeProfileMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<ChangeProfileResponseMessage>(changeProfileMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for get user profile
			app.MapPost("/getUserProfile", async (HttpContext context, GetUserProfileMessage getUserProfileMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<GetUserProfileResponseMessage>(getUserProfileMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});

			// api for logout
			app.MapPost("/logout", async (HttpContext context, LogoutMessage logoutMessage, ActorRegistry registry) =>
			{
				var userManagementActor = registry.Get<UserManagementActor>();
				var resp = await userManagementActor.Ask<LogoutResponseMessage>(logoutMessage, context.RequestAborted);
				await context.Response.WriteAsJsonAsync(resp);
			});
		}
    }
}
