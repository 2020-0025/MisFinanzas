namespace MisFinanzas.Domain.DTOs
{
    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }

        // Información adicional
        public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName)
            ? $"{FirstName} {LastName}"
            : UserName;

        public string StatusDisplay => IsActive ? "Activo" : "Inactivo";

        public string LastLoginDisplay => LastLogin.HasValue
            ? LastLogin.Value.ToString("dd/MM/yyyy HH:mm")
            : "Nunca";
    }
}