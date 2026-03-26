namespace MicroDev.Core.Simulation;

public sealed class SocialContact
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public SocialContactRole Role { get; set; } = SocialContactRole.Friend;

    public int BondProgress { get; set; }

    public int MessageCount { get; set; }

    public int CallCount { get; set; }

    public SocialContact Clone()
    {
        return new SocialContact
        {
            Id = Id,
            Name = Name,
            Role = Role,
            BondProgress = BondProgress,
            MessageCount = MessageCount,
            CallCount = CallCount,
        };
    }
}
