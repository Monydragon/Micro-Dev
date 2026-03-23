namespace MicroDev.Core.Portfolio;

using MicroDev.Core.Simulation;

public static class PortfolioWorkspace
{
    private static readonly IReadOnlyList<PortfolioProgramDefinition> Programs =
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

    public static int ProgramCount => Programs.Count;

    public static int TotalLinesOfCode => Programs.Sum(program => program.TotalLinesOfCode);

    public static PortfolioProgramDefinition GetCurrentProgram(RunState state)
    {
        var index = Math.Clamp(state.CurrentProgramIndex, 0, Programs.Count - 1);
        return Programs[index];
    }

    public static int GetCompletedProgramCount(RunState state)
    {
        if (state.CurrentProgramIndex >= Programs.Count - 1 &&
            state.CurrentProgramVisibleLineCount >= Programs[^1].CodeLines.Count)
        {
            return Programs.Count;
        }

        return Math.Clamp(state.CurrentProgramIndex, 0, Programs.Count);
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
                if (state.CurrentProgramIndex >= Programs.Count - 1)
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
            if (state.CurrentProgramIndex < Programs.Count - 1)
            {
                state.CurrentProgramIndex++;
                state.CurrentProgramVisibleLineCount = 0;
                startedFileName ??= GetCurrentProgram(state).FileName;
            }
        }

        return new PortfolioWriteResult(linesAdded, completedFileName, startedFileName);
    }

    public static void SynchronizeToLinesOfCode(RunState state)
    {
        var remainingLines = Math.Max(0, state.LinesOfCode);
        state.CurrentProgramIndex = 0;
        state.CurrentProgramVisibleLineCount = 0;

        for (var programIndex = 0; programIndex < Programs.Count; programIndex++)
        {
            var program = Programs[programIndex];
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
        }

        state.CurrentProgramIndex = Programs.Count - 1;
        state.CurrentProgramVisibleLineCount = Programs[^1].CodeLines.Count;
    }
}
