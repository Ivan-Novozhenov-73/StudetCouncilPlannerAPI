using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudetCouncilPlannerAPI.Models.Dtos;
using StudetCouncilPlannerAPI.Models.DTOs;
using StudetCouncilPlannerAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudetCouncilPlannerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerController : ControllerBase
    {
        private readonly PartnerService _partnerService;

        public PartnerController(PartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        // 1. Создать партнера (роль 1 или 2)
        [HttpPost]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<Guid>> Create([FromBody] PartnerCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _partnerService.CreatePartnerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { partnerId = id }, id);
        }

        // 2. Изменить партнера (роль 1 или 2)
        [HttpPut("{partnerId}")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> Update(Guid partnerId, [FromBody] PartnerUpdateDto dto)
        {
            var result = await _partnerService.UpdatePartnerAsync(partnerId, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        // 3. Архивировать партнера (роль 1 или 2)
        [HttpPatch("{partnerId}/archive")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> Archive(Guid partnerId)
        {
            var result = await _partnerService.ArchivePartnerAsync(partnerId);
            if (!result) return NotFound();
            return NoContent();
        }

        // 4. Восстановить партнера из архива (роль 1 или 2)
        [HttpPatch("{partnerId}/restore")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> Restore(Guid partnerId)
        {
            var result = await _partnerService.RestorePartnerAsync(partnerId);
            if (!result) return NotFound();
            return NoContent();
        }

        // 5. Поиск партнеров по ФИО + расширенный поиск (GET с query-параметрами)
        [HttpGet]
        public async Task<ActionResult<List<PartnerShortDto>>> Search([FromQuery] string? fio, [FromQuery] bool? archive, [FromQuery] Guid? eventId, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var filter = new PartnerFilterDto
            {
                FioSearch = fio,
                Archive = archive,
                EventId = eventId,
                Page = page,
                PageSize = pageSize
            };
            var result = await _partnerService.SearchPartnersAsync(filter);
            return Ok(result);
        }

        // 6. Получить партнера по id
        [HttpGet("{partnerId}")]
        public async Task<ActionResult<PartnerDetailDto>> GetById(Guid partnerId)
        {
            var result = await _partnerService.GetPartnerByIdAsync(partnerId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // 7. Получить все задачи партнера
        [HttpGet("{partnerId}/tasks")]
        public async Task<ActionResult<List<TaskShortDto>>> GetPartnerTasks(Guid partnerId)
        {
            var result = await _partnerService.GetTasksByPartnerAsync(partnerId);
            return Ok(result);
        }

        // 8. Получить все мероприятия партнера
        [HttpGet("{partnerId}/events")]
        public async Task<ActionResult<List<EventShortDto>>> GetPartnerEvents(Guid partnerId)
        {
            var result = await _partnerService.GetEventsByPartnerAsync(partnerId);
            return Ok(result);
        }
    }
}