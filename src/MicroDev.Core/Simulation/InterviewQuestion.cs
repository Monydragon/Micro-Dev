namespace MicroDev.Core.Simulation;

public sealed class InterviewQuestion
{
    public InterviewQuestion(string prompt, IReadOnlyList<string> options, int correctOptionIndex)
    {
        Prompt = prompt;
        Options = options.ToArray();
        CorrectOptionIndex = correctOptionIndex;
    }

    public string Prompt { get; }

    public IReadOnlyList<string> Options { get; }

    public int CorrectOptionIndex { get; }

    public InterviewQuestion Clone()
    {
        return new InterviewQuestion(Prompt, Options, CorrectOptionIndex);
    }
}
