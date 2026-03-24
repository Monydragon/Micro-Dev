namespace MicroDev.Core.Simulation;

internal static class JobApplicationContentCatalog
{
    private static readonly IReadOnlyList<ChallengeBlueprint> MonoGameChallenges =
    [
        new(
            "CameraFollowSystem.cs",
            "Finish a lightweight follow camera that eases toward a target while respecting a dead zone.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.Gameplay;",
                "",
                "public sealed class CameraFollowSystem",
                "{",
                "    public Vector2 Position { get; private set; }",
                "",
                "    public void Update(Vector2 target, Rectangle deadZone, float smoothing)",
                "    {",
                "        var desired = Position;",
                "",
                "        if (target.X < deadZone.Left)",
                "        {",
                "            desired.X += target.X - deadZone.Left;",
                "        }",
                "        else if (target.X > deadZone.Right)",
                "        {",
                "            desired.X += target.X - deadZone.Right;",
                "        }",
                "",
                "        if (target.Y < deadZone.Top)",
                "        {",
                "            desired.Y += target.Y - deadZone.Top;",
                "        }",
                "        else if (target.Y > deadZone.Bottom)",
                "        {",
                "            desired.Y += target.Y - deadZone.Bottom;",
                "        }",
                "",
                "        Position = Vector2.Lerp(Position, desired, smoothing);",
                "    }",
                "}",
            ]),
        new(
            "DashCooldownTracker.cs",
            "Ship a tiny tracker that cools down dash charges over time without exceeding the cap.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.Gameplay;",
                "",
                "public sealed class DashCooldownTracker",
                "{",
                "    public int Charges { get; private set; } = 3;",
                "    public float CooldownRemaining { get; private set; }",
                "",
                "    public void Update(float elapsedSeconds, float rechargeSeconds)",
                "    {",
                "        if (Charges >= 3)",
                "        {",
                "            CooldownRemaining = 0f;",
                "            return;",
                "        }",
                "",
                "        CooldownRemaining = MathF.Max(0f, CooldownRemaining - elapsedSeconds);",
                "        if (CooldownRemaining > 0f)",
                "        {",
                "            return;",
                "        }",
                "",
                "        Charges = Math.Min(3, Charges + 1);",
                "        CooldownRemaining = Charges >= 3 ? 0f : rechargeSeconds;",
                "    }",
                "}",
            ]),
        new(
            "SpawnBurstPlanner.cs",
            "Complete a helper that spaces enemy spawns evenly across a short burst window.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.Gameplay;",
                "",
                "public static class SpawnBurstPlanner",
                "{",
                "    public static Vector2[] BuildLine(Vector2 start, Vector2 step, int count)",
                "    {",
                "        var positions = new Vector2[count];",
                "",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            positions[index] = start + (step * index);",
                "        }",
                "",
                "        return positions;",
                "    }",
                "}",
            ]),
        new(
            "ParallaxStrip.cs",
            "Implement a strip that wraps background positions cleanly as the camera moves.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.Rendering;",
                "",
                "public static class ParallaxStrip",
                "{",
                "    public static float WrapOffset(float cameraX, float factor, float textureWidth)",
                "    {",
                "        var offset = cameraX * factor;",
                "        offset %= textureWidth;",
                "        if (offset < 0f)",
                "        {",
                "            offset += textureWidth;",
                "        }",
                "",
                "        return offset;",
                "    }",
                "}",
            ]),
        new(
            "ComboDecayMeter.cs",
            "Finish the decay step for a combo meter that drops safely to zero after enough idle time.",
            [
                "namespace Interview.Gameplay;",
                "",
                "public sealed class ComboDecayMeter",
                "{",
                "    public int Value { get; private set; }",
                "    public float IdleSeconds { get; private set; }",
                "",
                "    public void RegisterHit(int amount)",
                "    {",
                "        Value += amount;",
                "        IdleSeconds = 0f;",
                "    }",
                "",
                "    public void Update(float elapsedSeconds, float decayDelay, int decayAmount)",
                "    {",
                "        IdleSeconds += elapsedSeconds;",
                "        if (IdleSeconds < decayDelay)",
                "        {",
                "            return;",
                "        }",
                "",
                "        Value = Value < decayAmount ? 0 : Value - decayAmount;",
                "    }",
                "}",
            ]),
        new(
            "CollisionPulse.cs",
            "Build a pulse helper that flashes quickly after a collision and then fades back down.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.Feedback;",
                "",
                "public sealed class CollisionPulse",
                "{",
                "    public float Intensity { get; private set; }",
                "",
                "    public void Trigger(float amount)",
                "    {",
                "        Intensity = MathHelper.Clamp(Intensity + amount, 0f, 1f);",
                "    }",
                "",
                "    public void Update(float elapsedSeconds, float fadePerSecond)",
                "    {",
                "        Intensity = MathHelper.Clamp(Intensity - (elapsedSeconds * fadePerSecond), 0f, 1f);",
                "    }",
                "}",
            ]),
    ];

    private static readonly IReadOnlyList<ChallengeBlueprint> ToolingChallenges =
    [
        new(
            "BuildSummaryFormatter.cs",
            "Ship a formatter that turns raw build steps into a readable console summary for the tools team.",
            [
                "using System.Collections.Generic;",
                "using System.Text;",
                "",
                "namespace Interview.Tools;",
                "",
                "public static class BuildSummaryFormatter",
                "{",
                "    public static string Format(IReadOnlyList<string> steps)",
                "    {",
                "        var builder = new StringBuilder();",
                "        builder.AppendLine(\"Build Summary\");",
                "        builder.AppendLine(\"-------------\");",
                "",
                "        for (var index = 0; index < steps.Count; index++)",
                "        {",
                "            builder.Append(index + 1);",
                "            builder.Append(\". \");",
                "            builder.AppendLine(steps[index]);",
                "        }",
                "",
                "        return builder.ToString().TrimEnd();",
                "    }",
                "}",
            ]),
        new(
            "ArtifactCleanupPlanner.cs",
            "Complete a planner that keeps only the newest artifacts under a file-count limit.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Interview.Tools;",
                "",
                "public static class ArtifactCleanupPlanner",
                "{",
                "    public static List<string> PickForDeletion(IReadOnlyList<string> artifacts, int keepNewest)",
                "    {",
                "        var deletions = new List<string>();",
                "        var deleteCount = artifacts.Count - keepNewest;",
                "",
                "        for (var index = 0; index < deleteCount; index++)",
                "        {",
                "            deletions.Add(artifacts[index]);",
                "        }",
                "",
                "        return deletions;",
                "    }",
                "}",
            ]),
        new(
            "ReleaseNoteFormatter.cs",
            "Finish a formatter that writes a compact release note section from shipped ticket ids.",
            [
                "using System.Collections.Generic;",
                "using System.Text;",
                "",
                "namespace Interview.Tools;",
                "",
                "public static class ReleaseNoteFormatter",
                "{",
                "    public static string Format(string version, IReadOnlyList<string> tickets)",
                "    {",
                "        var builder = new StringBuilder();",
                "        builder.Append(\"Version \");",
                "        builder.AppendLine(version);",
                "",
                "        foreach (var ticket in tickets)",
                "        {",
                "            builder.Append(\"- \");",
                "            builder.AppendLine(ticket);",
                "        }",
                "",
                "        return builder.ToString().TrimEnd();",
                "    }",
                "}",
            ]),
        new(
            "BuildCacheIndex.cs",
            "Implement a tiny cache index that updates the last-seen tick for a build key.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Interview.Tools;",
                "",
                "public sealed class BuildCacheIndex",
                "{",
                "    private readonly Dictionary<string, long> _entries = new();",
                "",
                "    public void Touch(string key, long tick)",
                "    {",
                "        _entries[key] = tick;",
                "    }",
                "",
                "    public bool Contains(string key)",
                "    {",
                "        return _entries.ContainsKey(key);",
                "    }",
                "",
                "    public int Count => _entries.Count;",
                "}",
            ]),
        new(
            "TaskBatchReducer.cs",
            "Build a reducer that totals the duration of a task batch while ignoring null steps.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Interview.Tools;",
                "",
                "public static class TaskBatchReducer",
                "{",
                "    public static int SumMinutes(IReadOnlyList<int?> stepMinutes)",
                "    {",
                "        var total = 0;",
                "",
                "        foreach (var step in stepMinutes)",
                "        {",
                "            if (step is null)",
                "            {",
                "                continue;",
                "            }",
                "",
                "            total += step.Value;",
                "        }",
                "",
                "        return total;",
                "    }",
                "}",
            ]),
        new(
            "WarningRollup.cs",
            "Complete a helper that counts warnings per category for the build summary panel.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Interview.Tools;",
                "",
                "public static class WarningRollup",
                "{",
                "    public static Dictionary<string, int> Count(IReadOnlyList<string> categories)",
                "    {",
                "        var counts = new Dictionary<string, int>();",
                "",
                "        foreach (var category in categories)",
                "        {",
                "            counts[category] = counts.GetValueOrDefault(category) + 1;",
                "        }",
                "",
                "        return counts;",
                "    }",
                "}",
            ]),
    ];

    private static readonly IReadOnlyList<ChallengeBlueprint> UiChallenges =
    [
        new(
            "HudLayoutBuilder.cs",
            "Mock up a small HUD layout helper that spaces widgets evenly inside a panel.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class HudLayoutBuilder",
                "{",
                "    public static Rectangle[] BuildRow(Rectangle area, int count, int gap)",
                "    {",
                "        var slots = new Rectangle[count];",
                "        var slotWidth = (area.Width - ((count - 1) * gap)) / count;",
                "",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            var x = area.X + (index * (slotWidth + gap));",
                "            slots[index] = new Rectangle(x, area.Y, slotWidth, area.Height);",
                "        }",
                "",
                "        return slots;",
                "    }",
                "}",
            ]),
        new(
            "TooltipAnchorResolver.cs",
            "Complete an anchor helper that nudges tooltips back on screen when they would clip.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class TooltipAnchorResolver",
                "{",
                "    public static Point Resolve(Point mouse, Point tooltipSize, Rectangle viewport)",
                "    {",
                "        var x = mouse.X + 18;",
                "        var y = mouse.Y + 18;",
                "",
                "        if (x + tooltipSize.X > viewport.Right)",
                "        {",
                "            x = viewport.Right - tooltipSize.X;",
                "        }",
                "",
                "        if (y + tooltipSize.Y > viewport.Bottom)",
                "        {",
                "            y = viewport.Bottom - tooltipSize.Y;",
                "        }",
                "",
                "        return new Point(x, y);",
                "    }",
                "}",
            ]),
        new(
            "NotificationLane.cs",
            "Build a lane helper that stacks toast notifications with a fixed gap.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class NotificationLane",
                "{",
                "    public static Rectangle[] Build(Rectangle area, int count, int itemHeight, int gap)",
                "    {",
                "        var items = new Rectangle[count];",
                "",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            var y = area.Y + (index * (itemHeight + gap));",
                "            items[index] = new Rectangle(area.X, y, area.Width, itemHeight);",
                "        }",
                "",
                "        return items;",
                "    }",
                "}",
            ]),
        new(
            "SidebarSectionBuilder.cs",
            "Finish a small helper that slices a sidebar into header and body regions.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class SidebarSectionBuilder",
                "{",
                "    public static (Rectangle Header, Rectangle Body) Build(Rectangle area, int headerHeight)",
                "    {",
                "        var header = new Rectangle(area.X, area.Y, area.Width, headerHeight);",
                "        var body = new Rectangle(area.X, area.Y + headerHeight, area.Width, area.Height - headerHeight);",
                "        return (header, body);",
                "    }",
                "}",
            ]),
        new(
            "DialogChoiceLayout.cs",
            "Implement a simple layout that places dialog choice buttons from bottom to top.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class DialogChoiceLayout",
                "{",
                "    public static Rectangle[] Build(Rectangle area, int count, int height, int gap)",
                "    {",
                "        var choices = new Rectangle[count];",
                "        var y = area.Bottom - height;",
                "",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            choices[index] = new Rectangle(area.X, y - (index * (height + gap)), area.Width, height);",
                "        }",
                "",
                "        return choices;",
                "    }",
                "}",
            ]),
        new(
            "WidgetGridBuilder.cs",
            "Complete a builder that divides a panel into a clean widget grid.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Interview.UI;",
                "",
                "public static class WidgetGridBuilder",
                "{",
                "    public static Rectangle[] Build(Rectangle area, int columns, int rows, int gap)",
                "    {",
                "        var widgets = new Rectangle[columns * rows];",
                "        var cellWidth = (area.Width - ((columns - 1) * gap)) / columns;",
                "        var cellHeight = (area.Height - ((rows - 1) * gap)) / rows;",
                "",
                "        for (var row = 0; row < rows; row++)",
                "        {",
                "            for (var column = 0; column < columns; column++)",
                "            {",
                "                var index = (row * columns) + column;",
                "                var x = area.X + (column * (cellWidth + gap));",
                "                var y = area.Y + (row * (cellHeight + gap));",
                "                widgets[index] = new Rectangle(x, y, cellWidth, cellHeight);",
                "            }",
                "        }",
                "",
                "        return widgets;",
                "    }",
                "}",
            ]),
    ];

    private static readonly IReadOnlyList<InterviewQuestion> CoreProgrammingQuestions =
    [
        new(
            "Which C# type is typically used for a true-or-false value?",
            ["bool", "string", "int"],
            0),
        new(
            "Which type is the usual choice for text such as a player name?",
            ["string", "bool", "int"],
            0),
        new(
            "Which type is typically used for a whole number such as 42?",
            ["int", "bool", "string"],
            0),
        new(
            "What does an if statement usually do?",
            ["Runs code only when a condition is true", "Repeats a block a fixed number of times", "Stores a value for later use"],
            0),
        new(
            "What is a loop usually used for?",
            ["Repeating a set of steps", "Declaring a namespace", "Renaming a variable automatically"],
            0),
        new(
            "What is a variable mainly used for?",
            ["Storing a value you want to use later", "Ending the program immediately", "Rendering every frame by itself"],
            0),
        new(
            "What does a function's return value represent?",
            ["The result it gives back to the caller", "The line where the function was declared", "The button that triggered the function"],
            0),
        new(
            "What does == usually check?",
            ["Whether two values are equal", "Whether a method is static", "Whether a list has more than one item"],
            0),
        new(
            "What is a list or array useful for?",
            ["Storing multiple values together", "Making every method public", "Guaranteeing thread safety automatically"],
            0),
        new(
            "What does null usually mean?",
            ["No value is set there yet", "The value is always true", "The number is negative"],
            0),
        new(
            "What is a parameter?",
            ["A value passed into a function", "A file generated during publish", "A warning produced by the compiler"],
            0),
    ];

    public static (string Title, string Description, IReadOnlyList<string> CodeLines) CreateChallenge(ActiveJobListing listing, int runSeed)
    {
        var family = GetFamily(listing.TechStack);
        var challenges = family switch
        {
            JobTechFamily.MonoGame => MonoGameChallenges,
            JobTechFamily.Tooling => ToolingChallenges,
            _ => UiChallenges,
        };

        var seed = CreateSeed(runSeed, $"{listing.ListingId}:{listing.Title}:challenge");
        var challenge = challenges[seed % challenges.Count];
        return (challenge.Title, challenge.Description, challenge.CodeLines);
    }

    public static IReadOnlyList<InterviewQuestion> CreateInterviewQuestions(ActiveJobListing listing, int runSeed)
    {
        var seed = CreateSeed(runSeed, $"{listing.ListingId}:{listing.Title}:questions");
        var indexes = Enumerable.Range(0, CoreProgrammingQuestions.Count).ToList();
        Shuffle(indexes, seed);

        return indexes
            .Take(3)
            .Select((index, position) => ShuffleQuestionOptions(
                CoreProgrammingQuestions[index],
                CreateSeed(seed, $"question:{position}:{CoreProgrammingQuestions[index].Prompt}")))
            .ToArray();
    }

    private static JobTechFamily GetFamily(string techStack)
    {
        if (techStack.Contains("MonoGame", StringComparison.Ordinal))
        {
            return JobTechFamily.MonoGame;
        }

        if (techStack.Contains("Tool", StringComparison.Ordinal))
        {
            return JobTechFamily.Tooling;
        }

        return JobTechFamily.Ui;
    }

    private static int CreateSeed(int baseSeed, string key)
    {
        unchecked
        {
            var hash = baseSeed == 0 ? 17 : baseSeed;
            foreach (var character in key)
            {
                hash = (hash * 31) + character;
            }

            return hash & int.MaxValue;
        }
    }

    private static void Shuffle<T>(IList<T> values, int seed)
    {
        var random = new Random(seed);
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
        }
    }

    private static InterviewQuestion ShuffleQuestionOptions(InterviewQuestion question, int seed)
    {
        var options = question.Options.ToList();
        var correctAnswer = options[question.CorrectOptionIndex];
        Shuffle(options, seed);
        var correctOptionIndex = options.FindIndex(option => string.Equals(option, correctAnswer, StringComparison.Ordinal));
        return new InterviewQuestion(question.Prompt, options, correctOptionIndex);
    }

    private enum JobTechFamily
    {
        MonoGame,
        Tooling,
        Ui,
    }

    private sealed record ChallengeBlueprint(
        string Title,
        string Description,
        IReadOnlyList<string> CodeLines);
}
