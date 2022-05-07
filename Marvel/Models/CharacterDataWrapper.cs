namespace Marvel.Models;

public class CharacterDataWrapper
{
    public int Code { get; set; }

    public string Status { get; set; }

    public string Copyright { get; set; }

    public string AttributionText { get; set; }

    public string AttributionHTML { get; set; }

    public CharacterDataContainer Data { get; set; }

    public string Etag { get; set; }
}