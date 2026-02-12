﻿using System.ComponentModel.DataAnnotations;

namespace WelpenWache.Core.Database.Models;

public class UserPermission {
    
    [MaxLength(256)]
    public required string Sid { get; set; }
    public Permissions Permission { get; set; }
}