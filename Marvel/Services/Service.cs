using Marvel.Data;
using Marvel.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Marvel.Services;

public class Service : IService
{
    private const string BASE_URL = "http://gateway.marvel.com/v1/public";
    private readonly string _publicKey = "e111d19556dfac80b319b6e74ab4df6f";
    private readonly string _privateKey = "0751e452b72ce12ceb5a9f8129405a521e17339b";
    private static HttpClient _client = new HttpClient();
    private readonly DataContext _context;
    private readonly IConfiguration _config;

    public Service()
    {
    }

    public Service(DataContext context, IConfiguration configuration)
    {
        _context = context;
        _config = configuration;
    }

    public async Task<List<Character>> GetCharacters()
    {
        string requestURL = RequestUrl();

        var url = new Uri(requestURL);

        var response = await _client.GetAsync(url);

        string json;

        using (var content = response.Content)
        {
            json = await content.ReadAsStringAsync();
        }

        CharacterDataWrapper cdw = JsonConvert.DeserializeObject<CharacterDataWrapper>(json);

        return cdw.Data.Results;
    }

    public async Task<List<Character>> GetCharacters(string nameCharacter)
    {
        string requestURL = RequestUrl(nameCharacter);

        var url = new Uri(requestURL);

        var response = await _client.GetAsync(url);

        string json;

        using (var content = response.Content)
        {
            json = await content.ReadAsStringAsync();
        }

        CharacterDataWrapper cdw = JsonConvert.DeserializeObject<CharacterDataWrapper>(json);

        var result = cdw.Data.Results;

        result = new List<Character> { result.FirstOrDefault() };

        return result;
    }

    private string RequestUrl()
    {
        string timestamp = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds.ToString();

        string s = $"{timestamp}{_privateKey}{_publicKey}";

        string hash = CreateHash(s);

        string requestURL;

        requestURL = $"{BASE_URL}/characters?&ts={timestamp}&apikey={_publicKey}&hash={hash}";

        return requestURL;
    }

    private string RequestUrl(string nameCharacter)
    {
        string timestamp = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds.ToString();

        string s = $"{timestamp}{_privateKey}{_publicKey}";

        string hash = CreateHash(s);

        string requestURL;

        requestURL = $"{BASE_URL}/characters?name={nameCharacter}&ts={timestamp}&apikey={_publicKey}&hash={hash}";

        return requestURL;
    }

    private string CreateHash(string input)
    {
        var hash = String.Empty;
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            hash = sBuilder.ToString();
        }
        return hash;
    }

    public void SaveToDb(Character character)
    {
        if (_context.characterContext.Any(c => c.Id == character.Id))
        {
            _context.characterContext.Update(character);
        }
        else
        {
            _context.characterContext.Add(character);
        }
        _context.SaveChanges();
    }

    public bool validateDb()
    {
        var sqlCommand = _context.characterContext.CreateDbCommand();

        sqlCommand.CommandText = "SELECT COUNT(*) from [Characters].[dbo].[characterContext]";

        sqlCommand.Connection.ConnectionString = _config.GetConnectionString("defaultConnection");

        sqlCommand.Connection.Open();

        int validate = int.Parse(sqlCommand.ExecuteScalar().ToString());

        sqlCommand.Connection.Close();

        var response = validate > 0 ? true : false;

        return response;
    }

    public bool ValidateFavorite()
    {
        var sqlCommand = _context.characterContext.CreateDbCommand();

        sqlCommand.CommandText = "SELECT COUNT(*) from [Characters].[dbo].[characterContext] WHERE Favorite=1";

        sqlCommand.Connection.ConnectionString = _config.GetConnectionString("defaultConnection");

        sqlCommand.Connection.Open();

        int validate = int.Parse(sqlCommand.ExecuteScalar().ToString());

        sqlCommand.Connection.Close();

        var response = validate >= 5 ? true : false;

        return response;
    }

    public bool CharacterExists(int id)
    {
        return _context.characterContext.Any(e => e.Id == id);
    }

    public bool CharacterExists(string name)
    {
        return _context.characterContext.Any(e => e.Name == name);
    }
}