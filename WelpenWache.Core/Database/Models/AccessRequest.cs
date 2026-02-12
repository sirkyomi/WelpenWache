using System.ComponentModel.DataAnnotations;

namespace WelpenWache.Core.Database.Models;

public class AccessRequest {
    [Key]
    public int Id { get; set; }
    
    [MaxLength(256)]
    public required string Sid { get; set; }
    
    [MaxLength(256)]
    public required string Username { get; set; }
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;
    
    public DateTime? ProcessedAt { get; set; }
    
    [MaxLength(256)]
    public string? ProcessedBy { get; set; }
}

public enum AccessRequestStatus {
    Pending,
    Approved,
    Rejected
}

