using GoldbApi.Data;
using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldbApi.Services;

public interface INoticeService
{
    Task<IEnumerable<NoticeDto>> GetAllNoticesAsync(bool? isLoginRequired = null);
    Task<NoticeDto?> GetNoticeByIdAsync(int id);
    Task<NoticeDto> CreateNoticeAsync(CreateNoticeDto createDto);
    Task<bool> UpdateNoticeAsync(UpdateNoticeDto updateDto);
    Task<bool> DeleteNoticeAsync(int id);
    Task IncrementViewCountAsync(int id);
}

public class NoticeService : INoticeService
{
    private readonly IRepository<Notice> _noticeRepository;

    public NoticeService(IRepository<Notice> noticeRepository)
    {
        _noticeRepository = noticeRepository;
    }

    public async Task<IEnumerable<NoticeDto>> GetAllNoticesAsync(bool? isLoginRequired = null)
    {
        var query = _noticeRepository.GetQueryable();

        if (isLoginRequired.HasValue)
        {
            query = query.Where(n => n.IsLoginRequired == isLoginRequired.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ProjectToType<NoticeDto>()
            .ToListAsync();
    }

    public async Task<NoticeDto?> GetNoticeByIdAsync(int id)
    {
        var notice = await _noticeRepository.GetByIdAsync(id);
        if (notice == null) return null;

        return notice.Adapt<NoticeDto>();
    }

    public async Task<NoticeDto> CreateNoticeAsync(CreateNoticeDto createDto)
    {
        var notice = createDto.Adapt<Notice>();

        await _noticeRepository.AddAsync(notice);
        await _noticeRepository.SaveChangesAsync();

        return notice.Adapt<NoticeDto>();
    }

    public async Task<bool> UpdateNoticeAsync(UpdateNoticeDto updateDto)
    {
        var notice = await _noticeRepository.GetByIdAsync(updateDto.Id);
        if (notice == null) return false;

        updateDto.Adapt(notice);

        await _noticeRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNoticeAsync(int id)
    {
        var notice = await _noticeRepository.GetByIdAsync(id);
        if (notice == null) return false;

        _noticeRepository.Delete(notice);
        await _noticeRepository.SaveChangesAsync();
        return true;
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var notice = await _noticeRepository.GetByIdAsync(id);
        if (notice != null)
        {
            notice.ViewCount++;
            await _noticeRepository.SaveChangesAsync();
        }
    }
}
