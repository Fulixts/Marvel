using Marvel.Data;
using Marvel.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Tools.Models;

namespace Marvel.Services;

public class Service : IService
{
    private const string BASE_URL = "http://gateway.marvel.com/v1/public";
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

    public async Task<PaginatedList<Character>> GetCharacters(string SortProperty, OrderBy orderBy, int pageIndex = 1, int pageSize = 10)
    {
        PaginatedList<Character> retCharacters;

        if (validateDb())
        {
            List<Character> characters = await _context.characterContext.ToListAsync();

            characters = DoSort(characters, SortProperty, orderBy);

             retCharacters = new PaginatedList<Character>(characters, pageIndex, pageSize);

            return retCharacters;
        }
        string requestURL = RequestUrl();

        var url = new Uri(requestURL);

        var response = await _client.GetAsync(url);

        string json;

        using (var content = response.Content)
        {
            json = await content.ReadAsStringAsync();
        }

        CharacterDataWrapper cdw = JsonConvert.DeserializeObject<CharacterDataWrapper>(json);

        foreach (var character in cdw.Data.Results)
        {
            character.Image = $"{character.Thumbnail.Path}.{character.Thumbnail.Extension}";
            SaveToDb(character);
        }
        retCharacters = new PaginatedList<Character>(cdw.Data.Results, pageIndex, pageSize);

        return retCharacters;
    }

    public async Task<List<Character>> GetCharacters(string name)
    {
        string requestURL = RequestUrl(name);

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

        if (result[0] == null)
        {
            return null;
        }

        foreach (var character in result)
        {
            character.Image = $"{character.Thumbnail.Path}.{character.Thumbnail.Extension}";
            SaveToDb(character);
        }

        return result;
    }

    private List<Character> DoSort(List<Character> characters, string SortProperty, OrderBy orderBy)
    {
        if (SortProperty.ToLower() == "id")
        {
            if (orderBy == OrderBy.Ascending)
            {
                characters = characters.OrderBy(c => c.Id).ToList();
            }
            else
            {
                characters = characters.OrderByDescending(c => c.Id).ToList();
            }
        }
        else if (SortProperty.ToLower() == "name")
        {
            if (orderBy == OrderBy.Ascending)
            {
                characters = characters.OrderBy(c => c.Name).ToList();
            }
            else
            {
                characters = characters.OrderByDescending(c => c.Name).ToList();
            }
        }
        else
        {
            if (orderBy == OrderBy.Ascending)
            {
                characters = characters.OrderByDescending(c => c.Favorite).ThenBy(c => c.Name).ToList();
            }
            else
            {
                characters = characters.OrderBy(c => c.Favorite).ThenByDescending(c => c.Name).ToList();
            }
        }
        return characters;
    }

    private string RequestUrl()
    {
        string _privateKey = _config.GetSection("privateKey").Value;

        string _publicKey = _config.GetSection("publicKey").Value;

        string timestamp = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds.ToString();

        string s = $"{timestamp}{_privateKey}{_publicKey}";

        string hash = CreateHash(s);

        string requestURL;

        requestURL = $"{BASE_URL}/characters?&ts={timestamp}&apikey={_publicKey}&hash={hash}";

        return requestURL;
    }

    private string RequestUrl(string name)
    {
        string requestURL = RequestUrl();

        var response = $"{requestURL}&name={name}";

        return response;
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