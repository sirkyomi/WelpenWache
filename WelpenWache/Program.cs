using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.HttpSys;
using MudBlazor.Services;
using WelpenWache;
using WelpenWache.Components;
using WelpenWache.Core;
using WelpenWache.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Use HTTP.sys for Windows Authentication in Development
if (builder.Environment.IsDevelopment())
{
#pragma warning disable CA1416
    builder.WebHost.UseHttpSys(options =>
    {
        options.Authentication.Schemes = AuthenticationSchemes.Negotiate;
        options.Authentication.AllowAnonymous = true;
        options.UrlPrefixes.Add("http://localhost:5278");
    });
#pragma warning restore CA1416
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddWelpenWacheCoreServices(builder.Configuration.GetConnectionString("Default")!);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Intern.CanCreate, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Intern_Create))))
    .AddPolicy(Policies.Intern.CanRead, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Intern_Read))))
    .AddPolicy(Policies.Intern.CanUpdate, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Intern_Update))))
    .AddPolicy(Policies.Intern.CanDelete, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Intern_Delete))))
    .AddPolicy(Policies.Team.CanCreate, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Team_Create))))
    .AddPolicy(Policies.Team.CanRead, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Team_Read))))
    .AddPolicy(Policies.Team.CanUpdate, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Team_Update))))
    .AddPolicy(Policies.Team.CanDelete, policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Admin)) ||
            ctx.User.HasClaim(nameof(Permissions), nameof(Permissions.Team_Delete))))
    .AddPolicy(Policies.Admin.CanManageUsers, policy =>
        policy.RequireClaim(
            nameof(Permissions),
            nameof(Permissions.Admin)));
var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting();

app.UseAuthentication(); 
app.UseSetupRedirect();
app.UseAccessRequestRedirect();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
