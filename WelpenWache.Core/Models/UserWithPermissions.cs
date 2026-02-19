using WelpenWache.Core;

namespace WelpenWache.Core.Models;

public class UserWithPermissions {
    public required string Sid { get; set; }
    public required string Username { get; set; }
    public List<Permissions> Permissions { get; set; } = new();
}
