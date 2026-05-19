using System.ComponentModel.DataAnnotations;

namespace NutriAI.Application.DTOs;

public record AdminCreateUserDto(
    [Required, MinLength(2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public record AdminUpdateUserDto(
    [Required, MinLength(2)] string FullName,
    [Required, EmailAddress] string Email,
    bool IsBanned);
