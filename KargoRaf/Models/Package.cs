namespace KargoRaf.Models;

public class Package
{
    public int Id { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public int SectionId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsDelivered { get; set; }

    public string SectionName { get; set; } = string.Empty;
}
