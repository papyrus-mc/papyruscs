namespace PapyrusCs.Strategies.Dataflow
{
    public interface ITplBlock
    {
        int InputCount { get; }
        int OutputCount { get; }
        int ProcessedCount { get; }
    }
}