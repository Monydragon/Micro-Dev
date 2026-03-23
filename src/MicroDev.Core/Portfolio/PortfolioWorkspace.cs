namespace MicroDev.Core.Portfolio;

using MicroDev.Core.Simulation;

public static class PortfolioWorkspace
{
    private static readonly IReadOnlyList<PortfolioProgramDefinition> StarterPrograms =
    [
        new(
            "Task Queue",
            "TaskQueue.cs",
            "A tiny queue for estimating and starting tasks.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Timeboxing;",
                "",
                "public sealed class TaskQueue",
                "{",
                "    private readonly Queue<WorkItem> _items = new();",
                "",
                "    public int Count => _items.Count;",
                "",
                "    public void Enqueue(string title, int estimateMinutes)",
                "    {",
                "        _items.Enqueue(new WorkItem(title, estimateMinutes));",
                "    }",
                "",
                "    public WorkItem? TryStartNext()",
                "    {",
                "        if (_items.Count == 0)",
                "        {",
                "            return null;",
                "        }",
                "",
                "        return _items.Dequeue();",
                "    }",
                "",
                "    public TimeSpan GetRemainingEstimate()",
                "    {",
                "        var totalMinutes = 0;",
                "        foreach (var item in _items)",
                "        {",
                "            totalMinutes += item.EstimateMinutes;",
                "        }",
                "",
                "        return TimeSpan.FromMinutes(totalMinutes);",
                "    }",
                "}",
                "",
                "public sealed record WorkItem(string Title, int EstimateMinutes);",
            ]),
        new(
            "Dialogue Formatter",
            "DialogueFormatter.cs",
            "Formats interactive dialogue options for a narrative tool.",
            [
                "using System.Collections.Generic;",
                "using System.Text;",
                "",
                "namespace Portfolio.Dialogue;",
                "",
                "public static class DialogueFormatter",
                "{",
                "    public static string FormatChoices(",
                "        string speaker,",
                "        IReadOnlyList<string> choices)",
                "    {",
                "        var builder = new StringBuilder();",
                "        builder.AppendLine($\"{speaker}:\");",
                "",
                "        for (var index = 0; index < choices.Count; index++)",
                "        {",
                "            builder.Append(\"- \");",
                "            builder.Append(index + 1);",
                "            builder.Append(\". \");",
                "            builder.AppendLine(choices[index].Trim());",
                "        }",
                "",
                "        return builder.ToString().TrimEnd();",
                "    }",
                "",
                "    public static string CleanLine(string text)",
                "    {",
                "        return text.Replace(\"\\r\", string.Empty).Trim();",
                "    }",
                "}",
            ]),
        new(
            "Save Slot Index",
            "SaveSlotIndex.cs",
            "Tracks the newest save slot in a local index.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Persistence;",
                "",
                "public sealed class SaveSlotIndex",
                "{",
                "    private readonly Dictionary<string, DateTime> _slots = new();",
                "",
                "    public IEnumerable<string> Names => _slots.Keys;",
                "",
                "    public void Touch(string slotName, DateTime utcNow)",
                "    {",
                "        _slots[slotName] = utcNow;",
                "    }",
                "",
                "    public bool Contains(string slotName)",
                "    {",
                "        return _slots.ContainsKey(slotName);",
                "    }",
                "",
                "    public string? GetLatest()",
                "    {",
                "        string? latestName = null;",
                "        var latestTime = DateTime.MinValue;",
                "",
                "        foreach (var pair in _slots)",
                "        {",
                "            if (pair.Value <= latestTime)",
                "            {",
                "                continue;",
                "            }",
                "",
                "            latestName = pair.Key;",
                "            latestTime = pair.Value;",
                "        }",
                "",
                "        return latestName;",
                "    }",
                "}",
            ]),
        new(
            "Inventory Ledger",
            "InventoryLedger.cs",
            "Balances item counts for a systems-heavy game prototype.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Systems;",
                "",
                "public sealed class InventoryLedger",
                "{",
                "    private readonly Dictionary<string, int> _counts = new();",
                "",
                "    public int this[string itemId]",
                "    {",
                "        get => _counts.GetValueOrDefault(itemId);",
                "        set => _counts[itemId] = value;",
                "    }",
                "",
                "    public void Add(string itemId, int amount)",
                "    {",
                "        _counts[itemId] = this[itemId] + amount;",
                "    }",
                "",
                "    public bool Consume(string itemId, int amount)",
                "    {",
                "        var current = this[itemId];",
                "        if (current < amount)",
                "        {",
                "            return false;",
                "        }",
                "",
                "        _counts[itemId] = current - amount;",
                "        return true;",
                "    }",
                "}",
            ]),
        new(
            "Palette Cycler",
            "PaletteCycler.cs",
            "Interpolates a palette over time for animated effects.",
            [
                "using System;",
                "using Microsoft.Xna.Framework;",
                "",
                "namespace Portfolio.Rendering;",
                "",
                "public sealed class PaletteCycler",
                "{",
                "    private readonly Color[] _colors;",
                "",
                "    public PaletteCycler(params Color[] colors)",
                "    {",
                "        _colors = colors;",
                "    }",
                "",
                "    public Color Sample(float t)",
                "    {",
                "        if (_colors.Length == 0)",
                "        {",
                "            return Color.White;",
                "        }",
                "",
                "        var index = (int)MathF.Floor(t) % _colors.Length;",
                "        var next = (index + 1) % _colors.Length;",
                "        var blend = t - MathF.Floor(t);",
                "",
                "        return Color.Lerp(_colors[index], _colors[next], blend);",
                "    }",
                "}",
            ]),
        new(
            "Frame Stepper",
            "FrameStepper.cs",
            "Advances a simple frame animation by elapsed time.",
            [
                "using System;",
                "",
                "namespace Portfolio.Animation;",
                "",
                "public sealed class FrameStepper",
                "{",
                "    private readonly TimeSpan _frameLength;",
                "    private TimeSpan _accumulator;",
                "",
                "    public FrameStepper(double framesPerSecond)",
                "    {",
                "        _frameLength = TimeSpan.FromSeconds(1d / framesPerSecond);",
                "    }",
                "",
                "    public int Step(TimeSpan elapsed)",
                "    {",
                "        _accumulator += elapsed;",
                "        var frames = 0;",
                "",
                "        while (_accumulator >= _frameLength)",
                "        {",
                "            _accumulator -= _frameLength;",
                "            frames++;",
                "        }",
                "",
                "        return frames;",
                "    }",
                "}",
            ]),
        new(
            "Tag Matcher",
            "TagMatcher.cs",
            "Scores search tags for a small content browser.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Search;",
                "",
                "public static class TagMatcher",
                "{",
                "    public static int Score(",
                "        string query,",
                "        IReadOnlyCollection<string> tags)",
                "    {",
                "        var score = 0;",
                "        foreach (var tag in tags)",
                "        {",
                "            if (tag.Contains(query, StringComparison.OrdinalIgnoreCase))",
                "            {",
                "                score += 10;",
                "            }",
                "        }",
                "",
                "        return score;",
                "    }",
                "}",
            ]),
        new(
            "Daily Budget",
            "DailyBudget.cs",
            "Calculates whether a spending plan stays under a cap.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Tools;",
                "",
                "public sealed class DailyBudget",
                "{",
                "    public bool IsAffordable(",
                "        decimal cap,",
                "        IReadOnlyDictionary<string, decimal> expenses)",
                "    {",
                "        decimal total = 0;",
                "",
                "        foreach (var expense in expenses.Values)",
                "        {",
                "            total += expense;",
                "        }",
                "",
                "        return total <= cap;",
                "    }",
                "}",
            ]),
    ];

    private static readonly IReadOnlyList<PortfolioProgramDefinition> BonusPrograms =
    [
        new(
            "Input Buffer",
            "InputBuffer.cs",
            "Captures recent button presses to make actions more forgiving.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Input;",
                "",
                "public sealed class InputBuffer<T>",
                "{",
                "    private readonly Queue<(T Value, TimeSpan Age)> _entries = new();",
                "",
                "    public void Push(T value)",
                "    {",
                "        _entries.Enqueue((value, TimeSpan.Zero));",
                "    }",
                "",
                "    public void Update(TimeSpan elapsed, TimeSpan maxAge)",
                "    {",
                "        var count = _entries.Count;",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            var entry = _entries.Dequeue();",
                "            entry.Age += elapsed;",
                "            if (entry.Age <= maxAge)",
                "            {",
                "                _entries.Enqueue(entry);",
                "            }",
                "        }",
                "    }",
                "",
                "    public bool TryConsume(out T value)",
                "    {",
                "        if (_entries.Count == 0)",
                "        {",
                "            value = default!;",
                "            return false;",
                "        }",
                "",
                "        value = _entries.Dequeue().Value;",
                "        return true;",
                "    }",
                "}",
            ]),
        new(
            "Tween Sequence",
            "TweenSequence.cs",
            "Chains timed value transitions for UI animation beats.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Animation;",
                "",
                "public sealed class TweenSequence",
                "{",
                "    private readonly Queue<Func<float, float>> _steps = new();",
                "",
                "    public void Add(Func<float, float> easing)",
                "    {",
                "        _steps.Enqueue(easing);",
                "    }",
                "",
                "    public float Evaluate(float progress)",
                "    {",
                "        if (_steps.Count == 0)",
                "        {",
                "            return progress;",
                "        }",
                "",
                "        var clamped = Math.Clamp(progress, 0f, 1f);",
                "        return _steps.Peek()(clamped);",
                "    }",
                "",
                "    public void Advance()",
                "    {",
                "        if (_steps.Count > 0)",
                "        {",
                "            _steps.Dequeue();",
                "        }",
                "    }",
                "}",
            ]),
        new(
            "Sprite Batch Scope",
            "SpriteBatchScope.cs",
            "Wraps sprite-batch begin/end into a disposable render helper.",
            [
                "using System;",
                "using Microsoft.Xna.Framework.Graphics;",
                "",
                "namespace Portfolio.Rendering;",
                "",
                "public readonly struct SpriteBatchScope : IDisposable",
                "{",
                "    private readonly SpriteBatch _spriteBatch;",
                "",
                "    public SpriteBatchScope(SpriteBatch spriteBatch)",
                "    {",
                "        _spriteBatch = spriteBatch;",
                "        _spriteBatch.Begin();",
                "    }",
                "",
                "    public void Dispose()",
                "    {",
                "        _spriteBatch.End();",
                "    }",
                "}",
            ]),
        new(
            "Audio Bus",
            "AudioBus.cs",
            "Separates music and effects gain into named channels.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Audio;",
                "",
                "public sealed class AudioBus",
                "{",
                "    private readonly Dictionary<string, float> _gains = new();",
                "",
                "    public void SetGain(string busName, float gain)",
                "    {",
                "        _gains[busName] = gain;",
                "    }",
                "",
                "    public float GetGain(string busName)",
                "    {",
                "        return _gains.TryGetValue(busName, out var gain)",
                "            ? gain",
                "            : 1f;",
                "    }",
                "",
                "    public float Mix(string busName, float sample)",
                "    {",
                "        return sample * GetGain(busName);",
                "    }",
                "}",
            ]),
        new(
            "Achievement Tracker",
            "AchievementTracker.cs",
            "Unlocks lightweight goals and prevents duplicate popups.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Progression;",
                "",
                "public sealed class AchievementTracker",
                "{",
                "    private readonly HashSet<string> _unlocked = new();",
                "",
                "    public bool Unlock(string achievementId)",
                "    {",
                "        return _unlocked.Add(achievementId);",
                "    }",
                "",
                "    public bool IsUnlocked(string achievementId)",
                "    {",
                "        return _unlocked.Contains(achievementId);",
                "    }",
                "",
                "    public IReadOnlyCollection<string> Snapshot()",
                "    {",
                "        return _unlocked;",
                "    }",
                "}",
            ]),
        new(
            "Status Timeline",
            "StatusTimeline.cs",
            "Tracks temporary gameplay effects over a shared timeline.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Gameplay;",
                "",
                "public sealed class StatusTimeline",
                "{",
                "    private readonly List<(string Id, TimeSpan Remaining)> _entries = [];",
                "",
                "    public void Add(string statusId, TimeSpan duration)",
                "    {",
                "        _entries.Add((statusId, duration));",
                "    }",
                "",
                "    public void Update(TimeSpan elapsed)",
                "    {",
                "        for (var index = _entries.Count - 1; index >= 0; index--)",
                "        {",
                "            var entry = _entries[index];",
                "            entry.Remaining -= elapsed;",
                "            if (entry.Remaining <= TimeSpan.Zero)",
                "            {",
                "                _entries.RemoveAt(index);",
                "                continue;",
                "            }",
                "",
                "            _entries[index] = entry;",
                "        }",
                "    }",
                "}",
            ]),
        new(
            "Scene Router",
            "SceneRouter.cs",
            "Moves between named scenes without leaking navigation rules everywhere.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Navigation;",
                "",
                "public sealed class SceneRouter",
                "{",
                "    private readonly Dictionary<string, Action> _routes = new();",
                "",
                "    public void Map(string sceneId, Action enter)",
                "    {",
                "        _routes[sceneId] = enter;",
                "    }",
                "",
                "    public bool TryGo(string sceneId)",
                "    {",
                "        if (!_routes.TryGetValue(sceneId, out var enter))",
                "        {",
                "            return false;",
                "        }",
                "",
                "        enter();",
                "        return true;",
                "    }",
                "}",
            ]),
        new(
            "Projectile Pool",
            "ProjectilePool.cs",
            "Reuses projectile instances instead of allocating them every frame.",
            [
                "using System.Collections.Generic;",
                "",
                "namespace Portfolio.Gameplay;",
                "",
                "public sealed class ProjectilePool",
                "{",
                "    private readonly Stack<Projectile> _free = new();",
                "",
                "    public Projectile Rent()",
                "    {",
                "        return _free.Count > 0",
                "            ? _free.Pop()",
                "            : new Projectile();",
                "    }",
                "",
                "    public void Return(Projectile projectile)",
                "    {",
                "        projectile.Active = false;",
                "        _free.Push(projectile);",
                "    }",
                "}",
                "",
                "public sealed class Projectile",
                "{",
                "    public bool Active { get; set; }",
                "}",
            ]),
    ];

    private static readonly IReadOnlyList<GeneratedProgramTheme> GeneratedThemes =
    [
        new("Quest", "Quests", "quest state and progression"),
        new("Dialogue", "Dialogue", "dialogue branches and conversation data"),
        new("Save", "SaveData", "save payloads and restore points"),
        new("Inventory", "Inventory", "inventory entries and item counts"),
        new("Audio", "Audio", "audio cues and routing metadata"),
        new("Achievement", "Achievements", "achievement unlock data"),
        new("Level", "Levels", "level metadata and staging state"),
        new("Input", "Input", "input actions and recent command flow"),
        new("Build", "Builds", "build steps and artifact metadata"),
        new("Camera", "Cameras", "camera tuning and framing state"),
        new("Tooltip", "Tooltips", "tooltip content and placement hints"),
        new("Matchmaking", "Matchmaking", "matchmaking tickets and lobby state"),
    ];

    private static readonly IReadOnlyList<PortfolioProgramDefinition> CuratedProgramTemplates =
        StarterPrograms.Concat(BonusPrograms).ToArray();
    private static readonly IReadOnlyList<PortfolioProgramDefinition> GeneratedProgramTemplates = BuildGeneratedPrograms();
    private static readonly IReadOnlyList<PortfolioProgramDefinition> ProgramPool =
        CuratedProgramTemplates.Concat(GeneratedProgramTemplates).ToArray();
    private static readonly Dictionary<int, IReadOnlyList<PortfolioProgramDefinition>> ProgramPoolBySeed = [];

    public static int ProgramCount => ProgramPool.Count;

    public static int GetProgramCount(RunState state)
    {
        if (state.Difficulty == GameDifficulty.Endless)
        {
            return GetRunProgramPool(state).Count;
        }

        return Math.Clamp(state.Difficulty switch
        {
            GameDifficulty.Easy => 8,
            GameDifficulty.Hard => 16,
            _ => 12,
        }, 1, GetRunProgramPool(state).Count);
    }

    public static bool HasFiniteProgramCount(RunState state)
    {
        return state.Difficulty != GameDifficulty.Endless;
    }

    public static PortfolioProgramDefinition GetCurrentProgram(RunState state)
    {
        return GetProgramAt(state, state.CurrentProgramIndex);
    }

    public static int GetCompletedProgramCount(RunState state)
    {
        var currentProgram = GetCurrentProgram(state);
        var completed = state.CurrentProgramIndex;
        if (state.CurrentProgramVisibleLineCount >= currentProgram.CodeLines.Count)
        {
            completed++;
        }

        return HasFiniteProgramCount(state)
            ? Math.Clamp(completed, 0, GetProgramCount(state))
            : Math.Max(0, completed);
    }

    public static IReadOnlyList<string> GetVisibleLines(RunState state)
    {
        var program = GetCurrentProgram(state);
        return program.CodeLines.Take(state.CurrentProgramVisibleLineCount).ToArray();
    }

    public static PortfolioWriteResult RevealLines(RunState state, int requestedLinesOfCode)
    {
        var linesToReveal = Math.Max(0, requestedLinesOfCode);
        var linesAdded = 0;
        string? completedFileName = null;
        string? startedFileName = null;

        while (linesToReveal > 0)
        {
            var program = GetCurrentProgram(state);

            if (state.CurrentProgramVisibleLineCount >= program.CodeLines.Count)
            {
                if (HasFiniteProgramCount(state) &&
                    state.CurrentProgramIndex >= GetProgramCount(state) - 1)
                {
                    break;
                }

                state.CurrentProgramIndex++;
                state.CurrentProgramVisibleLineCount = 0;
                startedFileName ??= GetCurrentProgram(state).FileName;
                continue;
            }

            var nextLine = program.CodeLines[state.CurrentProgramVisibleLineCount];
            state.CurrentProgramVisibleLineCount++;

            if (!string.IsNullOrWhiteSpace(nextLine))
            {
                linesToReveal--;
                linesAdded++;
            }

            if (state.CurrentProgramVisibleLineCount < program.CodeLines.Count)
            {
                continue;
            }

            completedFileName ??= program.FileName;
            if (HasFiniteProgramCount(state) &&
                state.CurrentProgramIndex >= GetProgramCount(state) - 1)
            {
                continue;
            }

            state.CurrentProgramIndex++;
            state.CurrentProgramVisibleLineCount = 0;
            startedFileName ??= GetCurrentProgram(state).FileName;
        }

        return new PortfolioWriteResult(linesAdded, completedFileName, startedFileName);
    }

    public static void SynchronizeToLinesOfCode(RunState state)
    {
        var remainingLines = Math.Max(0, state.LinesOfCode);
        state.CurrentProgramIndex = 0;
        state.CurrentProgramVisibleLineCount = 0;

        var programIndex = 0;
        while (true)
        {
            var program = GetProgramAt(state, programIndex);
            var visibleLineCount = 0;

            for (var lineIndex = 0; lineIndex < program.CodeLines.Count; lineIndex++)
            {
                var line = program.CodeLines[lineIndex];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (remainingLines <= 0)
                    {
                        state.CurrentProgramIndex = programIndex;
                        state.CurrentProgramVisibleLineCount = visibleLineCount;
                        return;
                    }

                    remainingLines--;
                }

                visibleLineCount++;
                if (remainingLines <= 0)
                {
                    state.CurrentProgramIndex = programIndex;
                    state.CurrentProgramVisibleLineCount = visibleLineCount;
                    return;
                }
            }

            if (HasFiniteProgramCount(state) &&
                programIndex >= GetProgramCount(state) - 1)
            {
                state.CurrentProgramIndex = programIndex;
                state.CurrentProgramVisibleLineCount = program.CodeLines.Count;
                return;
            }

            programIndex++;
        }
    }

    private static PortfolioProgramDefinition GetProgramAt(RunState state, int programIndex)
    {
        var programPool = GetRunProgramPool(state);
        var templateCount = GetProgramCount(state);
        var clampedIndex = Math.Max(0, programIndex);

        if (HasFiniteProgramCount(state))
        {
            return programPool[Math.Clamp(clampedIndex, 0, templateCount - 1)];
        }

        var template = programPool[clampedIndex % templateCount];
        var cycle = clampedIndex / templateCount;
        if (cycle <= 0)
        {
            return template;
        }

        var cycleNumber = cycle + 1;
        var suffix = $".Pass{cycleNumber}";
        var fileName = template.FileName.EndsWith(".cs", StringComparison.Ordinal)
            ? template.FileName[..^3] + suffix + ".cs"
            : template.FileName + suffix;

        return new PortfolioProgramDefinition(
            $"{template.ProjectName} Iteration {cycleNumber}",
            fileName,
            $"{template.Description} Endless mode keeps the portfolio feed moving with another real pass.",
            template.CodeLines);
    }

    private static IReadOnlyList<PortfolioProgramDefinition> GetRunProgramPool(RunState state)
    {
        var seed = state.RunSeed == 0 ? 17 : state.RunSeed;
        if (ProgramPoolBySeed.TryGetValue(seed, out var programPool))
        {
            return programPool;
        }

        var shuffled = ProgramPool.ToList();
        Shuffle(shuffled, seed);
        programPool = shuffled;
        ProgramPoolBySeed[seed] = programPool;
        return programPool;
    }

    private static IReadOnlyList<PortfolioProgramDefinition> BuildGeneratedPrograms()
    {
        var programs = new List<PortfolioProgramDefinition>();

        foreach (var theme in GeneratedThemes)
        {
            programs.Add(CreateRegistryProgram(theme));
            programs.Add(CreatePlannerProgram(theme));
            programs.Add(CreateFormatterProgram(theme));
            programs.Add(CreateTimelineProgram(theme));
            programs.Add(CreateSelectorProgram(theme));
            programs.Add(CreateBufferProgram(theme));
            programs.Add(CreateLayoutProgram(theme));
            programs.Add(CreateStatsProgram(theme));
            programs.Add(CreateIndexProgram(theme));
            programs.Add(CreateRouterProgram(theme));
        }

        return programs;
    }

    private static PortfolioProgramDefinition CreateRegistryProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Registry";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Registry",
            $"{className}.cs",
            $"Caches {theme.DomainDescription} by id so the runtime can answer lookups without reopening source data every frame.",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Dictionary<string, string> _entries = new();",
                "",
                "    public void Remember(string id, string value)",
                "    {",
                "        _entries[id] = value;",
                "    }",
                "",
                "    public bool TryGet(string id, out string value)",
                "    {",
                "        return _entries.TryGetValue(id, out value);",
                "    }",
                "",
                "    public IEnumerable<string> Keys => _entries.Keys;",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreatePlannerProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Planner";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Planner",
            $"{className}.cs",
            $"Queues small work items for {theme.DomainDescription} so the runtime can consume them in a predictable order.",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Queue<(string Id, int Cost)> _items = new();",
                "",
                "    public void Enqueue(string id, int cost)",
                "    {",
                "        _items.Enqueue((id, cost));",
                "    }",
                "",
                "    public bool TryDequeue(out (string Id, int Cost) item)",
                "    {",
                "        if (_items.Count == 0)",
                "        {",
                "            item = default;",
                "            return false;",
                "        }",
                "",
                "        item = _items.Dequeue();",
                "        return true;",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateFormatterProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Formatter";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Formatter",
            $"{className}.cs",
            $"Formats {theme.DomainDescription} into a readable summary for logs, tools, and quick debugging.",
            [
                "using System.Collections.Generic;",
                "using System.Text;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public static class {className}",
                "{",
                "    public static string Format(IReadOnlyList<string> lines)",
                "    {",
                "        var builder = new StringBuilder();",
                "",
                "        for (var index = 0; index < lines.Count; index++)",
                "        {",
                "            builder.Append(index + 1);",
                "            builder.Append(\": \");",
                "            builder.AppendLine(lines[index].Trim());",
                "        }",
                "",
                "        return builder.ToString().TrimEnd();",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateTimelineProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Timeline";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Timeline",
            $"{className}.cs",
            $"Tracks temporary {theme.DomainDescription} over time and removes entries that have already expired.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly List<(string Id, TimeSpan Remaining)> _entries = [];",
                "",
                "    public void Add(string id, TimeSpan duration)",
                "    {",
                "        _entries.Add((id, duration));",
                "    }",
                "",
                "    public void Update(TimeSpan elapsed)",
                "    {",
                "        for (var index = _entries.Count - 1; index >= 0; index--)",
                "        {",
                "            var entry = _entries[index];",
                "            entry.Remaining -= elapsed;",
                "            if (entry.Remaining <= TimeSpan.Zero)",
                "            {",
                "                _entries.RemoveAt(index);",
                "                continue;",
                "            }",
                "",
                "            _entries[index] = entry;",
                "        }",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateSelectorProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Selector";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Selector",
            $"{className}.cs",
            $"Cycles through candidate {theme.DomainDescription} while keeping the next pick stable and easy to reason about.",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private int _nextIndex;",
                "",
                "    public string? PickNext(IReadOnlyList<string> values)",
                "    {",
                "        if (values.Count == 0)",
                "        {",
                "            return null;",
                "        }",
                "",
                "        var value = values[_nextIndex % values.Count];",
                "        _nextIndex++;",
                "        return value;",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateBufferProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}Buffer";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Buffer",
            $"{className}.cs",
            $"Keeps the most recent {theme.DomainDescription} events under a fixed cap so the newest information stays hot.",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Queue<string> _entries = new();",
                "",
                "    public void Push(string value, int capacity)",
                "    {",
                "        _entries.Enqueue(value);",
                "        while (_entries.Count > capacity)",
                "        {",
                "            _entries.Dequeue();",
                "        }",
                "    }",
                "",
                "    public string[] Snapshot()",
                "    {",
                "        return _entries.ToArray();",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateLayoutProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}LayoutStrip";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Layout Strip",
            $"{className}.cs",
            $"Spaces small panels for {theme.DomainDescription} across a row without scattering the layout math everywhere.",
            [
                "using Microsoft.Xna.Framework;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public static class {className}",
                "{",
                "    public static Rectangle[] Build(Rectangle area, int count, int gap)",
                "    {",
                "        var slots = new Rectangle[count];",
                "        var width = (area.Width - ((count - 1) * gap)) / count;",
                "",
                "        for (var index = 0; index < count; index++)",
                "        {",
                "            var x = area.X + (index * (width + gap));",
                "            slots[index] = new Rectangle(x, area.Y, width, area.Height);",
                "        }",
                "",
                "        return slots;",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateStatsProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}StatsLedger";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Stats Ledger",
            $"{className}.cs",
            $"Accumulates counters for {theme.DomainDescription} and exposes a simple read-only total by key.",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Dictionary<string, int> _counts = new();",
                "",
                "    public void Add(string id, int amount)",
                "    {",
                "        _counts[id] = _counts.GetValueOrDefault(id) + amount;",
                "    }",
                "",
                "    public int Get(string id)",
                "    {",
                "        return _counts.GetValueOrDefault(id);",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateIndexProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}RecentIndex";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Recent Index",
            $"{className}.cs",
            $"Tracks the most recent timestamp written for {theme.DomainDescription} so the latest entry stays easy to query.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Dictionary<string, DateTime> _entries = new();",
                "",
                "    public void Touch(string id, DateTime utcNow)",
                "    {",
                "        _entries[id] = utcNow;",
                "    }",
                "",
                "    public DateTime? TryGet(string id)",
                "    {",
                "        return _entries.TryGetValue(id, out var value) ? value : null;",
                "    }",
                "}",
            ]);
    }

    private static PortfolioProgramDefinition CreateRouterProgram(GeneratedProgramTheme theme)
    {
        var className = $"{theme.Name}ActionRouter";
        return new PortfolioProgramDefinition(
            $"{theme.Name} Action Router",
            $"{className}.cs",
            $"Maps named actions for {theme.DomainDescription} so screen code can trigger behavior without hardcoded switch blocks everywhere.",
            [
                "using System;",
                "using System.Collections.Generic;",
                "",
                $"namespace Portfolio.Generated.{theme.NamespaceSegment};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly Dictionary<string, Action> _routes = new();",
                "",
                "    public void Map(string id, Action action)",
                "    {",
                "        _routes[id] = action;",
                "    }",
                "",
                "    public bool TryRun(string id)",
                "    {",
                "        if (!_routes.TryGetValue(id, out var action))",
                "        {",
                "            return false;",
                "        }",
                "",
                "        action();",
                "        return true;",
                "    }",
                "}",
            ]);
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

    private sealed record GeneratedProgramTheme(
        string Name,
        string NamespaceSegment,
        string DomainDescription);
}
