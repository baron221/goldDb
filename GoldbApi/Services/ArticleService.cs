using GoldbApi.DTOs;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IArticleService
{
    Task<ApiResponse<ArticleListResponse>> GetArticlesAsync(ArticleListRequest request);
    Task<ApiResponse<Article>> GetArticleDetailAsync(int id);
    Task<ApiResponse<string>> CreateArticleAsync(Article article);
    Task<ApiResponse<string>> UpdateArticleAsync(Article article);
}

public class ArticleService : IArticleService
{
    private readonly IRepository<Article> _articleRepository;

    public ArticleService(IRepository<Article> articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public async Task<ApiResponse<ArticleListResponse>> GetArticlesAsync(ArticleListRequest request)
    {
        var query = _articleRepository.GetQueryable();

        if (request.Importance.HasValue)
            query = query.Where(a => a.Importance == request.Importance.Value);

        if (!string.IsNullOrEmpty(request.Type))
            query = query.Where(a => a.Type == request.Type);

        if (!string.IsNullOrEmpty(request.Title))
            query = query.Where(a => a.Title.Contains(request.Title));

        if (request.Sort == "-id")
            query = query.OrderByDescending(a => a.Id);
        else
            query = query.OrderBy(a => a.Id);

        var total = await query.CountAsync();
        var items = await query.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToListAsync();

        return ApiResponse<ArticleListResponse>.Success(new ArticleListResponse
        {
            Total = total,
            Items = items
        });
    }

    public async Task<ApiResponse<Article>> GetArticleDetailAsync(int id)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null) return ApiResponse<Article>.Failure("Article not found", 404);
        return ApiResponse<Article>.Success(article);
    }

    public async Task<ApiResponse<string>> CreateArticleAsync(Article article)
    {
        await _articleRepository.AddAsync(article);
        await _articleRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }

    public async Task<ApiResponse<string>> UpdateArticleAsync(Article article)
    {
        _articleRepository.Update(article);
        await _articleRepository.SaveChangesAsync();
        return ApiResponse<string>.Success("success");
    }
}
