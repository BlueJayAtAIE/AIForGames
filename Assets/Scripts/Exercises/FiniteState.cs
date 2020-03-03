public interface IFiniteState
{
    // Called when transitioning into this state.
    void EnterState();

    // Called when executing this state.
    void RunState();

    // Called when trasitioning way from this state.
    void ExitState();


    // Adds a trasition away from this state.
    void AddCondition(IStateTransition transition);

    // Removes a transition away from this state.
    void RemoveCondition(IStateTransition transition);


    // Evaluates its conditions and returns the next state to switch to.
    //  If no change is necessary, it may return the current state.
    IFiniteState ChangeState();
}

public interface IStateTransition
{
    bool ShouldTrasition();

    IFiniteState NextState { get; }
}
