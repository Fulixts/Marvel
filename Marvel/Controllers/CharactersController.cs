#nullable disable

using Marvel.Data;
using Marvel.Models;
using Marvel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tools.Models;

namespace Marvel.Controllers;

[Route("/[controller]")]
[Controller]
public class CharactersController : Controller
{
    private readonly DataContext _context;
    private readonly IService _service;

    public CharactersController(DataContext context, IService service)
    {
        _context = context;
        _service = service;
    }

    // GET: Characters
    [HttpGet]
    public async Task<IActionResult> Index(string sortExpression = "", int page = 1, int pageSize = 5)
    {
        SortModel sort = new SortModel();
        sort.AddColumn("id");
        sort.AddColumn("name");
        sort.AddColumn("favorite");
        sort.ApplySort(sortExpression);
        ViewData["sort"] = sort;

        PaginatedList<Character> characters = await _service.GetCharacters(sort.SortProperty, sort.OrderBy, page, pageSize);

        var pager = new PagerModel(characters.TotalRecords, page, pageSize);
        pager.SortExpression = sortExpression;
        this.ViewBag.Pager = pager;

        return View(characters);
    }

    [HttpGet("/[controller]/{name}")]
    public async Task<IActionResult> GetCharacter(string GetCharacter)
    {
        if (GetCharacter == null)
        {
            return NotFound();
        }

        if (_service.CharacterExists(GetCharacter))
        {
            var character = _context.characterContext.FirstOrDefault(c => c.Name == GetCharacter);

            List<Character> result = new List<Character>() { character };

            return View(result);
        }
        else
        {
            var result = await _service.GetCharacters(GetCharacter);

            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }
    }

    // GET: Characters/Details/5
    [Route("/[controller]/details")]
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var character = await _context.characterContext
            .FirstOrDefaultAsync(m => m.Id == id);
        if (character == null)
        {
            return NotFound();
        }

        return View(character);
    }

    // GET: Characters/Edit/5
    [Route("/[controller]/edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var character = await _context.characterContext.FindAsync(id);
        if (character == null)
        {
            return NotFound();
        }
        return View(character);
    }

    // POST: Characters/Edit/5
    [Route("/[controller]/edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Modified,ResourceURI,Image,Favorite")] Character character)
    {
        if (id != character.Id)
        {
            return NotFound();
        }

        if (character.Description == null)
            character.Description = String.Empty;

        if (character.Favorite == true && !_service.ValidateFavorite())
        {
            character.Favorite = true;
        }
        else
        {
            character.Favorite = false;
        }

        try
        {
            _context.Update(character);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_service.CharacterExists(character.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }
}