using Microsoft.EntityFrameworkCore;

namespace titledbConverter.Models;

[PrimaryKey("Id")]
public sealed class ScreenShot
{
    public int Id { get; init; }
    public string Url { get; init; }
    public int TitleId { get; init; }
    public Title? Title { get; init; }
}