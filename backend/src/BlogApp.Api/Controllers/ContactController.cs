using BlogApp.Application.Features.Administration.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Api.Controllers;

[ApiController]
public class ContactController : ControllerBase
{
    private readonly ContactInfoService _contactService;

    public ContactController(ContactInfoService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>Lấy thông tin liên hệ và các liên kết mạng xã hội (công khai cho người đọc).</summary>
    [HttpGet("api/contact")]
    [AllowAnonymous]
    public async Task<ActionResult<ContactInfoDto>> GetContact(CancellationToken ct)
    {
        var result = await _contactService.GetContactInfoAsync(ct);
        return Ok(result);
    }

    /// <summary>Quản trị viên cập nhật thông tin liên hệ và mạng xã hội.</summary>
    [HttpPut("api/admin/contact")]
    [Authorize]
    public async Task<ActionResult<ContactInfoDto>> UpdateContact([FromBody] UpdateContactInfoDto dto, CancellationToken ct)
    {
        var result = await _contactService.UpdateContactInfoAsync(dto, ct);
        return Ok(result);
    }
}
