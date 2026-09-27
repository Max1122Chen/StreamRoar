namespace StreamRoar.Infrastructure
{
    public readonly struct ConsoleOutputEntry
    {
        public ConsoleOutputKind Kind { get; }
        public string Text { get; }

        public ConsoleOutputEntry(ConsoleOutputKind kind, string text)
        {
            Kind = kind;
            Text = text ?? string.Empty;
        }
    }
}
