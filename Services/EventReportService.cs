using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Data;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace StudetCouncilPlannerAPI.Services
{
    public class EventReportService
    {
        private readonly ApplicationDbContext _context;

        public EventReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string DateToString(DateOnly? date) => date?.ToString("dd.MM.yyyy") ?? "—";
        private static string TimeToString(TimeSpan? time) => time.HasValue ? time.Value.ToString(@"hh\:mm") : "—";

        /// <summary>
        /// Генерирует PDF с планом мероприятий на следующий месяц
        /// </summary>
        public async Task<byte[]> GeneratePlanNextMonthAsync()
        {
            var today = DateTime.Today;
            var nextMonthDate = today.AddMonths(1);
            int nextMonth = nextMonthDate.Month;
            int year = nextMonthDate.Year;

            var firstDay = new DateOnly(year, nextMonth, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var events = await _context.Events
                .Include(e => e.EventUsers)
                    .ThenInclude(eu => eu.User)
                .Where(e => e.EndDate >= firstDay && e.EndDate <= lastDay)
                .OrderBy(e => e.EndDate)
                .ToListAsync();

            var reportRows = events.Select(e =>
            {
                var mainOrg = e.EventUsers.FirstOrDefault(u => u.Role == 2);
                return new EventPlanReportRowDto
                {
                    Title = e.Title,
                    EventDate = e.EndDate,
                    EventTime = e.EventTime,
                    Location = string.IsNullOrWhiteSpace(e.Location) ? "По согласованию" : e.Location,
                    ShortDescription = e.Description,
                    ResponsibleFullName = mainOrg != null ? $"{mainOrg.User.Surname} {GetInitials(mainOrg.User.Name, mainOrg.User.Patronymic)}" : "—",
                    ResponsiblePhone = mainOrg != null ? mainOrg.User.Phone.ToString() : "—"
                };
            }).ToList();

            string monthRu = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetMonthName(nextMonth).ToLower();

            return GeneratePlanPdf(reportRows, monthRu, year);
        }

        /// <summary>
        /// Генерирует PDF с отчетом о проведенных мероприятиях за указанный месяц и год
        /// </summary>
        public async Task<byte[]> GenerateReportForMonthAsync(int year, int month)
        {
            var firstDay = new DateOnly(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            const short CompletedStatus = 4;

            var events = await _context.Events
                .Include(e => e.EventUsers)
                    .ThenInclude(eu => eu.User)
                .Where(e => e.EndDate >= firstDay && e.EndDate <= lastDay && e.Status == CompletedStatus)
                .OrderBy(e => e.EndDate)
                .ToListAsync();

            var reportRows = events.Select(e =>
            {
                var mainOrg = e.EventUsers.FirstOrDefault(u => u.Role == 2);
                var organizerTeam = e.EventUsers
                    .Where(u => u.Role == 1)
                    .Select(u => $"{u.User.Surname} {GetInitials(u.User.Name, u.User.Patronymic)}")
                    .ToList();
                var participantsCount = e.EventUsers.Count(u => u.Role == 0);
                var organizersCount = e.EventUsers.Count(u => u.Role == 1 || u.Role == 2);

                return new EventCompletedReportRowDto
                {
                    Title = e.Title,
                    EventDate = e.EndDate,
                    EventTime = e.EventTime,
                    Location = string.IsNullOrWhiteSpace(e.Location) ? "По согласованию" : e.Location,
                    OrganizersCount = organizersCount,
                    ActiveParticipantsCount = participantsCount,
                    NumberOfSpectators = e.NumberOfParticipants,
                    ResponsibleFullName = mainOrg != null ? $"{mainOrg.User.Surname} {GetInitials(mainOrg.User.Name, mainOrg.User.Patronymic)}" : "—",
                    ResponsiblePhone = mainOrg != null ? mainOrg.User.Phone.ToString() : "—",
                    OrganizerTeamFullNames = organizerTeam
                };
            }).ToList();

            string monthRu = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetMonthName(month).ToLower();

            return GenerateCompletedReportPdf(reportRows, monthRu, year);
        }

        /// <summary>
        /// Генерирует PDF с отчетом о завершенных мероприятиях пользователя
        /// </summary>
        public async Task<byte[]> GenerateUserEventsReportAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            const short CompletedStatus = 4;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var userEventRoles = await _context.EventUsers
                .Include(eu => eu.Event)
                .Where(eu => eu.UserId == userId && eu.Event.Status == CompletedStatus && eu.Event.EndDate <= today)
                .ToListAsync();

            var reportRows = userEventRoles.Select(eu => new UserEventsReportRowDto
            {
                Title = eu.Event.Title,
                UserRole = eu.Role,
                EventDate = eu.Event.EndDate,
                EventTime = eu.Event.EventTime,
                Location = string.IsNullOrWhiteSpace(eu.Event.Location) ? "По согласованию" : eu.Event.Location
            })
            .OrderBy(r => r.EventDate)
            .ToList();

            var userInfo = new UserInfoForReportDto
            {
                Group = user.Group,
                Surname = user.Surname,
                Name = user.Name,
                Patronymic = user.Patronymic
            };

            return GenerateUserEventsReportPdf(reportRows, userInfo);
        }

        // Формирует строку "И.О."
        private static string GetInitials(string name, string patronymic)
        {
            var initials = "";
            if (!string.IsNullOrWhiteSpace(name))
                initials += name[0] + ".";
            if (!string.IsNullOrWhiteSpace(patronymic))
                initials += patronymic[0] + ".";
            return initials;
        }

        // Единый стиль для заголовка таблицы
        private static IContainer CellHeaderStyle(IContainer container) =>
            container
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(0)
                .PaddingHorizontal(0)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten1)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(x => x.SemiBold());

        // Единый стиль для ячеек таблицы
        private static IContainer CellStyle(IContainer container) =>
            container
                .PaddingVertical(0)
                .PaddingHorizontal(0)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .AlignCenter()
                .AlignMiddle();

        /// <summary>
        /// Генерация PDF плана мероприятий (единый стиль)
        /// </summary>
        private byte[] GeneratePlanPdf(List<EventPlanReportRowDto> rows, string monthText, int year)
        {
            var file = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Element(header =>
                    {
                        header
                            .AlignCenter()
                            .Column(column =>
                            {
                                column.Spacing(3);
                                column.Item().AlignCenter().Text("ПЛАН")
                                    .FontSize(22).Bold();
                                column.Item().AlignCenter().Text("мероприятий совета студенческого самоуправления")
                                    .FontSize(14).SemiBold();
                                column.Item().AlignCenter().Text($"на {monthText} {year} г.")
                                    .FontSize(13).SemiBold();
                                column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            });
                    });

                    page.Content().Element(content =>
                    {
                        content
                        .DefaultTextStyle(s => s.FontSize(9))
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); // Название
                                columns.RelativeColumn(1f);   // Дата
                                columns.RelativeColumn(1f);   // Время
                                columns.RelativeColumn(1.2f); // Место
                                columns.RelativeColumn(2f);   // Краткое описание
                                columns.RelativeColumn(1.5f); // Ответственный
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Название мероприятия");
                                header.Cell().Element(CellHeaderStyle).Text("Дата проведения");
                                header.Cell().Element(CellHeaderStyle).Text("Время проведения");
                                header.Cell().Element(CellHeaderStyle).Text("Место проведения");
                                header.Cell().Element(CellHeaderStyle).Text("Краткое описание");
                                header.Cell().Element(CellHeaderStyle).Text("Ответственный (Ф.И.О., телефон)");
                            });

                            foreach (var row in rows)
                            {
                                table.Cell().Element(CellStyle).Text(row.Title).WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(DateToString(row.EventDate));
                                table.Cell().Element(CellStyle).Text(TimeToString(row.EventTime));
                                table.Cell().Element(CellStyle).Text(row.Location ?? "По согласованию").WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(row.ShortDescription).WrapAnywhere();
                                table.Cell().Element(CellStyle).Text($"{row.ResponsibleFullName}\n{row.ResponsiblePhone}").WrapAnywhere();
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return file;
        }

        /// <summary>
        /// Генерация PDF отчета о проведенных мероприятиях (единый стиль)
        /// </summary>
        private byte[] GenerateCompletedReportPdf(List<EventCompletedReportRowDto> rows, string monthText, int year)
        {
            var file = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Element(header =>
                    {
                        header.AlignCenter().Column(column =>
                        {
                            column.Spacing(3);
                            column.Item().AlignCenter().Text("ОТЧЕТ")
                                .FontSize(22).Bold();
                            column.Item().AlignCenter().Text("о проведенных мероприятиях совета студенческого самоуправления")
                                .FontSize(14).SemiBold();
                            column.Item().AlignCenter().Text($"за {monthText} {year} г.")
                                .FontSize(13).SemiBold();
                            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        });
                    });

                    page.Content().Element(content =>
                    {
                        content
                        .DefaultTextStyle(s => s.FontSize(9))
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); // Название
                                columns.RelativeColumn(1f);   // Дата
                                columns.RelativeColumn(1f);   // Время
                                columns.RelativeColumn(1.2f); // Место
                                columns.RelativeColumn(0.7f); // орг.
                                columns.RelativeColumn(0.9f); // участников
                                columns.RelativeColumn(0.9f); // зрителей
                                columns.RelativeColumn(1.5f); // ответственный
                                columns.RelativeColumn(2f);   // команда
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Название nмероприятия");
                                header.Cell().Element(CellHeaderStyle).Text("Дата");
                                header.Cell().Element(CellHeaderStyle).Text("Время");
                                header.Cell().Element(CellHeaderStyle).Text("Место");
                                header.Cell().Element(CellHeaderStyle).Text("Орг.");
                                header.Cell().Element(CellHeaderStyle).Text("Участн.");
                                header.Cell().Element(CellHeaderStyle).Text("Зрители");
                                header.Cell().Element(CellHeaderStyle).Text("Ответственный\n");
                                header.Cell().Element(CellHeaderStyle).Text("Команда организаторов");
                            });

                            foreach (var row in rows)
                            {
                                table.Cell().Element(CellStyle).Text(row.Title).WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(DateToString(row.EventDate));
                                table.Cell().Element(CellStyle).Text(TimeToString(row.EventTime));
                                table.Cell().Element(CellStyle).Text(row.Location ?? "—").WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(row.OrganizersCount.ToString());
                                table.Cell().Element(CellStyle).Text(row.ActiveParticipantsCount.ToString());
                                table.Cell().Element(CellStyle).Text(row.NumberOfSpectators?.ToString() ?? "—");
                                table.Cell().Element(CellStyle).Text($"{row.ResponsibleFullName}\n{row.ResponsiblePhone}").WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(string.Join(",\n", row.OrganizerTeamFullNames)).WrapAnywhere();
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return file;
        }

        /// <summary>
        /// Генерация PDF отчета о завершенных мероприятиях пользователя (единый стиль)
        /// </summary>
        private byte[] GenerateUserEventsReportPdf(List<UserEventsReportRowDto> rows, UserInfoForReportDto user)
        {
            var file = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Element(header =>
                    {
                        header
                            .AlignCenter()
                            .Column(column =>
                            {
                                column.Spacing(3);
                                column.Item().AlignCenter().Text("ОТЧЕТ")
                                    .FontSize(22).Bold();
                                column.Item().AlignCenter().Text($"о мероприятиях студента гр. {user.Group}")
                                    .FontSize(14).SemiBold();
                                column.Item().AlignCenter().Text($"{user.Surname} {user.Name} {user.Patronymic}")
                                    .FontSize(13).SemiBold();
                                column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            });
                    });

                    page.Content().Element(content =>
                    {
                        content
                        .DefaultTextStyle(s => s.FontSize(9))
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); // Название
                                columns.RelativeColumn(1.2f);   // Роль
                                columns.RelativeColumn(1f);   // Дата
                                columns.RelativeColumn(1f);   // Время
                                columns.RelativeColumn(1.2f); // Место
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Название мероприятия");
                                header.Cell().Element(CellHeaderStyle).Text("Роль");
                                header.Cell().Element(CellHeaderStyle).Text("Дата проведения");
                                header.Cell().Element(CellHeaderStyle).Text("Время проведения");
                                header.Cell().Element(CellHeaderStyle).Text("Место проведения");
                            });

                            foreach (var row in rows)
                            {
                                table.Cell().Element(CellStyle).Text(row.Title).WrapAnywhere();
                                table.Cell().Element(CellStyle).Text(GetRoleName(row.UserRole));
                                table.Cell().Element(CellStyle).Text(DateToString(row.EventDate));
                                table.Cell().Element(CellStyle).Text(TimeToString(row.EventTime));
                                table.Cell().Element(CellStyle).Text(row.Location ?? "По согласованию").WrapAnywhere();
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return file;

            string GetRoleName(short role) =>
                role switch
                {
                    0 => "участник",
                    1 => "организатор",
                    2 => "главный организатор",
                    _ => $"роль {role}"
                };
        }
    }
}