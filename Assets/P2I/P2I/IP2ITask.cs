using UnityEngine;

public interface IP2ITask
{
    void EnterTask();
    void UpdateTask();
    void ExitTask();
    void RequestNextTrial();

    int TrialIndex { get; }
    int NumberOfTrials { get; }
    bool ForceNextTrial { get; }
    string TaskStep { get; }
}
