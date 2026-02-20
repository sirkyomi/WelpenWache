# WelpenWache

A modern Blazor Server application for managing internal data with Windows Authentication and fine-grained permission management.

## Overview

WelpenWache is a role-based application built with .NET 10 and Blazor Server that provides secure access control using Windows Authentication. The application features a first-run setup wizard, access request workflow, and comprehensive user management.

## Purpose

WelpenWache is designed for internal teams that need a clear overview of interns and a secure, auditable way to:

- Manage intern records and related internal data
- Control access with fine-grained permissions
- Track access requests and approvals in a clear workflow

## Features

- **Windows Authentication**: Seamless integration with Windows Active Directory
- **First-Run Setup**: Automatic admin creation on initial deployment
- **Access Request Workflow**: Users can request access, admins approve with specific permissions
- **Fine-Grained Permissions**: Granular control over user capabilities
- **Modern UI**: Built with MudBlazor component library
- **Responsive Design**: Works on desktop and mobile devices

## Authentication & Authorization

### Windows Authentication

The application uses Windows Authentication (Negotiate/Kerberos) to automatically authenticate users based on their Windows login credentials. No separate login is required.

### Permission System

The application supports the following permissions:

- **Admin**: Full administrative access, including user management
- **Intern_Create**: Create new intern records
- **Intern_Read**: View intern records
- **Intern_Update**: Edit existing intern records
- **Intern_Delete**: Delete intern records

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server (or SQL Server Express)
- Windows environment (for Windows Authentication)
- IIS or HTTP.sys capable server

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/sirkyomi/WelpenWache.git
   cd WelpenWache/src
   ```

2. **Configure the database connection**
   
   Update the connection string in `WelpenWache/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=.;Database=WelpenWache;Integrated Security=true;TrustServerCertificate=true;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project WelpenWache.Core --startup-project WelpenWache
   ```

4. **Run the application**
   ```bash
   cd WelpenWache
   dotnet run
   ```

5. **Access the application**
   
   Navigate to `http://localhost:5278` in your browser.

### Docker Deployment

A Dockerfile is included for containerized deployment:

```bash
docker build -t welpenwache .
docker run -p 8080:8080 welpenwache
```

> **Note**: When running in Docker, you'll need to configure Windows Authentication appropriately for your container environment.

## First-Run Setup

When you first deploy the application:

1. The application automatically detects that no users exist
2. You are redirected to the **Setup** page (`/setup`)
3. The currently authenticated Windows user is displayed
4. Click **"Setup abschließen"** (Complete Setup)
5. Your Windows account is granted all permissions and becomes the first administrator

### Access Workflow for Subsequent Users

After the initial setup, new users follow this workflow:

1. **User accesses the application**
   - Authenticated via Windows Authentication
   - Has no permissions in the database

2. **Automatic redirect to Request Access page**
   - User sees they need permissions
   - Can submit an access request

3. **Admin approval**
   - Administrators see pending requests in `/admin/users`
   - Can approve or reject requests
   - When approving, can select specific permissions to grant

4. **User gains access**
   - After approval, user can access permitted features
   - May need to refresh the page or re-login

## User Management

Administrators with the **Admin** permission can manage users at `/admin/users`:

### Pending Requests

View all pending access requests with:
- Username
- Windows SID
- Request timestamp
- Actions: Approve (with permission selection) or Reject

### Request History

View the complete history of all access requests:
- Status (Pending, Approved, Rejected)
- Processing details (who approved/rejected and when)

### Permission Assignment

When approving a user, administrators can select which permissions to grant:
- Select individual permissions based on the user's role
- At least one permission must be selected
- Can include Admin permission to create additional administrators

## Project Structure

```
WelpenWache/
├── WelpenWache/                    # Main Blazor Server application
│   ├── Components/
│   │   ├── Pages/                  # Razor pages
│   │   │   ├── Setup.razor         # First-run setup
│   │   │   ├── RequestAccess.razor # Access request page
│   │   │   ├── ManageUsers.razor   # User management (admin)
│   │   │   └── ...
│   │   └── Layout/                 # Layout components
│   ├── Middleware/
│   │   ├── SetupRedirectMiddleware.cs       # Redirects to setup if needed
│   │   └── AccessRequestRedirectMiddleware.cs # Redirects users without permissions
│   ├── Program.cs                  # Application entry point
│   └── appsettings.json            # Configuration
│
└── WelpenWache.Core/               # Core business logic
    ├── Database/
    │   ├── WelpenWacheContext.cs   # EF Core DbContext
    │   ├── Models/                 # Database models
    │   │   ├── UserPermission.cs
    │   │   ├── AccessRequest.cs
    │   │   └── Intern.cs
    │   └── Migrations/             # EF Core migrations
    ├── Services/
    │   ├── SetupService.cs         # First-run setup logic
    │   ├── AccessRequestService.cs # Access request management
    │   └── PermissionService.cs    # Permission management
    ├── Authorization/              # Authorization policies
    └── Permissions.cs              # Permission enum
```

## Configuration

### Development Environment

In development, the application uses HTTP.sys for Windows Authentication:

```csharp
builder.WebHost.UseHttpSys(options =>
{
    options.Authentication.Schemes = AuthenticationSchemes.Negotiate;
    options.Authentication.AllowAnonymous = true;
    options.UrlPrefixes.Add("http://localhost:5278");
});
```

### Production Environment

For production deployment:

1. **IIS**: Configure Windows Authentication in IIS
   - Enable Windows Authentication
   - Disable Anonymous Authentication
   - Configure application pool with appropriate identity

2. **HTTP.sys**: Configure appropriate URL reservations and SSL certificates

3. **Environment Variables**: Set `ASPNETCORE_ENVIRONMENT` to `Production`

### IIS Hosting Notes

- The app derives its base path at runtime, so there is no hardcoded `<base href>`.
- This supports hosting under a virtual directory (e.g., `https://server/werbung/welpenwache/`) without extra configuration.
- Ensure the IIS site/application path matches the URL you want to serve.

### Database Configuration

The application uses Entity Framework Core with SQL Server. Configure your connection string in:
- `appsettings.json` for default configuration
- `appsettings.Development.json` for development overrides
- `appsettings.Production.json` for production settings

## Security Considerations

- **Windows Authentication**: Users are authenticated based on their Windows credentials
- **SID-based Authorization**: Permissions are stored using Windows Security Identifiers (SID)
- **Claims Transformation**: User permissions are loaded into claims on each request
- **Policy-based Authorization**: All pages use declarative authorization policies
- **HTTPS**: Always use HTTPS in production
- **CSRF Protection**: Built-in antiforgery validation

## Technologies Used

- **.NET 10.0**: Latest .NET framework
- **Blazor Server**: Interactive server-side rendering
- **Entity Framework Core 10**: ORM for database access
- **MudBlazor**: Material Design component library
- **SQL Server**: Database backend
- **Windows Authentication**: Negotiate/Kerberos authentication

## Troubleshooting

### "Access Denied" or "401 Unauthorized"

- Ensure Windows Authentication is enabled in IIS or HTTP.sys
- Verify the user is logged into a domain or has a local Windows account
- Check that Anonymous Authentication is disabled

### Setup Page Not Appearing

- Clear browser cache and cookies
- Verify database connectivity
- Check that the UserPermissions table is empty

### Permissions Not Working After Approval

- User may need to log out and log back in
- Refresh the browser page (F5)
- Check that permissions were saved correctly in the database

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure the application has access to the database
- Run migrations: `dotnet ef database update`

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Merge Strategy

- Pull requests into `master` must use **Squash and merge** only.
- Do not use merge commits or rebase merges for `master`.
- This keeps Nerdbank commit height aligned with one commit per merged PR on `master`.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For questions or issues, please [create an issue](link-to-issues) in the repository.

