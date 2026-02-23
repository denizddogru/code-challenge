using AdessoWorldLeague.API.DTOs;

namespace AdessoWorldLeague.API.Services;

public interface IDrawService
{
    Task<DrawResponse> PerformDrawAsync(DrawRequest request);
}
