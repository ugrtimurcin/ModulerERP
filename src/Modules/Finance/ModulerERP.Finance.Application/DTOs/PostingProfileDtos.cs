using System;

namespace ModulerERP.Finance.Application.DTOs;

public class PostingProfileDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public string? Category { get; set; }
    public bool IsDefault { get; set; }
    
    // Simplification for the basic Frontend UI
    public Guid? AccountId { get; set; }
}

public class CreatePostingProfileDto
{
    public int TransactionType { get; set; }
    public string? Category { get; set; }
    public Guid AccountId { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdatePostingProfileDto
{
    public int TransactionType { get; set; }
    public string? Category { get; set; }
    public Guid AccountId { get; set; }
    public bool IsDefault { get; set; }
}
