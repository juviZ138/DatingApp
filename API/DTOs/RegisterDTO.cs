using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDTO
{
    [Required]
    public  string Username { get; set; } = String.Empty;
    [Required]
    [StringLength(8,  MinimumLength = 4)]
    public  string Password { get; set; } = String.Empty;
}
