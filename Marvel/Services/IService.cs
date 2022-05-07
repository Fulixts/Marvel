using Marvel.Models;

namespace Marvel.Services;

public interface IService
{
    Task<List<Character>> GetCharacters();
    Task<List<Character>> GetCharacters(string Name);
    void SaveToDb(Character character);
    bool validateDb();
    bool CharacterExists(int id);
    bool CharacterExists(string name);
    bool ValidateFavorite();
}