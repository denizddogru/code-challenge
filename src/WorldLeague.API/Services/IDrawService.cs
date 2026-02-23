using WorldLeague.API.DTOs;

namespace WorldLeague.API.Services;

public interface IDrawService
{
    Task<DrawResponse> PerformDrawAsync(DrawRequest request);
}
