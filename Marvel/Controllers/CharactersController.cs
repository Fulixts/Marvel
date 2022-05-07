#nullable disable

using Marvel.Data;
using Marvel.Models;
using Marvel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Index(string? SortOrder, int pg = 1)
    {
        const int pgSize = 10;
        int recsCount;
        int recSkip;

        ViewBag.Id = String.IsNullOrEmpty(SortOrder) ? "Id_desc" : "";
        ViewBag.Name = SortOrder == "Name" ? "Name_desc" : "Name";
        ViewBag.Favorite = SortOrder == "Favorite" ? "Favorite_desc" : "Favorite";

        if (pg < 1)
        {
            pg = 1;
        }

        recSkip = (pg - 1) * pgSize;

        if (_service.validateDb())
        {
            var result = await _context.characterContext.ToListAsync();

            switch (SortOrder)
            {
                case "Id_desc":
                    result = result.OrderByDescending(c => c.Id).ToList();
                    break;
                case "Name":
                    result = result.OrderBy(c => c.Name).ToList();
                    break;
                case "Name_desc":
                    result = result.OrderByDescending(c => c.Name).ToList();
                    break;
                case "Favorite":
                    result = result.OrderByDescending(c => c.Favorite).ToList();
                    break;
                case "Favorite_desc":
                    result = result.OrderBy(c => c.Favorite).ToList();
                    break;
                default:
                    result = result.OrderBy(c => c.Id).ToList();
                    break;
            }

            recsCount = result.Count();

            var pager = new Pager(recsCount, pg, pgSize);

            var data = result.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
        }
        else
        {
            var result = await _service.GetCharacters();

            result = result.OrderBy(c => c.Name).ToList();

            foreach (var character in result)
            {
                _service.SaveToDb(character);
            }

            recsCount = result.Count();

            var pager = new Pager(recsCount, pg, pgSize);

            var data = result.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
        }
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> SingleCharacter(string nameCharacter)
    {
        if (nameCharacter == null)
        {
            return NotFound();
        }

        if (_service.CharacterExists(nameCharacter))
        {
            var character = _context.characterContext.FirstOrDefault(c => c.Name == nameCharacter);

            List<Character> result = new List<Character>() { character };

            return View(result);
        }
        else
        {
            var result = await _service.GetCharacters(nameCharacter);

            foreach (var character in result)
            {
                _service.SaveToDb(character);
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Modified,ResourceURI,Favorite")] Character character)
    {
        if (id != character.Id)
        {
            return NotFound();
        }

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