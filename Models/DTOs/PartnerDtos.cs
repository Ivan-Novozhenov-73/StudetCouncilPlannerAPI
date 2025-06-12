using StudetCouncilPlannerAPI.Models.Dtos;

namespace StudetCouncilPlannerAPI.Models.DTOs
{
    // Для создания партнера
    public class PartnerCreateDto
    {
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string Description { get; set; } = string.Empty;
        public long Phone { get; set; }
        public string Contacts { get; set; } = string.Empty;
    }

    // Для обновления партнера
    public class PartnerUpdateDto
    {
        public string? Surname { get; set; }
        public string? Name { get; set; }
        public string? Patronymic { get; set; }
        public string? Description { get; set; }
        public long? Phone { get; set; }
        public string? Contacts { get; set; }
    }

    // Для краткого вывода (например, в списке)
    public class PartnerShortDto
    {
        public Guid PartnerId { get; set; }
        public string Fio { get; set; } = string.Empty; // ФИО в одну строку
        public string? Description { get; set; }
    }

    // Для детального вывода, включая связи
    public class PartnerDetailDto
    {
        public Guid PartnerId { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string Description { get; set; } = string.Empty;
        public long Phone { get; set; }
        public string Contacts { get; set; } = string.Empty;
        public bool Archive { get; set; }

        public List<EventShortDto>? Events { get; set; }
        public List<TaskShortDto>? Tasks { get; set; }
    }

    // Для поиска/фильтрации
    public class PartnerFilterDto
    {
        public string? FioSearch { get; set; }
        public bool? Archive { get; set; }
        public Guid? EventId { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}