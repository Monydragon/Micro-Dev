namespace MicroDev.Core.Simulation;

public sealed class ProjectBlueprint
{
    public ProjectProductType ProductType { get; set; } = ProjectProductType.App;

    public string Theme { get; set; } = string.Empty;

    public string Tone { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string BusinessModel { get; set; } = string.Empty;

    public int VariantSeedOffset { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Pitch { get; set; } = string.Empty;

    public double PublishIncomeMultiplier { get; set; } = 1d;

    public double SaleIncomeMultiplier { get; set; } = 1d;

    public string Signature =>
        $"{(int)ProductType}|{Theme}|{Tone}|{Platform}|{BusinessModel}|{VariantSeedOffset}|{Title}";

    public ProjectBlueprint Clone()
    {
        return new ProjectBlueprint
        {
            ProductType = ProductType,
            Theme = Theme,
            Tone = Tone,
            Platform = Platform,
            BusinessModel = BusinessModel,
            VariantSeedOffset = VariantSeedOffset,
            Title = Title,
            Pitch = Pitch,
            PublishIncomeMultiplier = PublishIncomeMultiplier,
            SaleIncomeMultiplier = SaleIncomeMultiplier,
        };
    }
}
