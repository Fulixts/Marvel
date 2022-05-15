using Marvel.Models;
using Tools.Models;

namespace Marvel.Services;

public interface IService
{
    Task<PaginatedList<Character>> GetCharacters(string SortProperty, OrderBy orderBy, int pageIndex = 1, int pageSize = 10);

    Task<List<Character>> GetCharacters(string Name);

    void SaveToDb(Character character);

    bool validateDb();

    bool CharacterExists(int id);

    bool CharacterExists(string name);

    bool ValidateFavorite();
}