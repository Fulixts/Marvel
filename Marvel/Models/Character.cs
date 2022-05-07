using System.ComponentModel.DataAnnotations.Schema;
using Marvel.Api.Model.DomainObjects;
using Marvel.Api.Model.Lists;

namespace Marvel.Models;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Modified { get; set; }
    public string ResourceURI { get; set; }
    [NotMapped]
    public List<MarvelUrl> Urls { get; set; }
    [NotMapped]
    public MarvelImage Thumbnail { get; set; }
    [NotMapped]
    public ComicList Comics { get; set; }
    [NotMapped]
    public StoryList Stories { get; set; }
    [NotMapped]
    public EventList Events { get; set; }
    [NotMapped]
    public SeriesList Series { get; set; }
    public bool Favorite { get; set; } = false;
}