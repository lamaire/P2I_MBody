
public interface IP2ITask
{
    int TrialIndex { get; }
    int NumberOfTrials { get; }
    bool ForceNextTrial { get; }
    string TaskStep { get; }

    void EnterTask();
    void UpdateTask();
    void ExitTask();
    void RequestNextTrial();

}
