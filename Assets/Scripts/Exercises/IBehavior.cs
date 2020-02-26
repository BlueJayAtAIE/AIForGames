public enum BehaviorResult { SUCCESS, FAILURE }

public interface IBehavior
{
    BehaviorResult DoBehavior();
}

/// <summary>
/// Executes each child in the order that they're specified in.
/// Runs through until one child returns a success, then returns success itself immediately.
/// If no child returns a success, it returns a failure itself.
/// </summary>
public class Selector : IBehavior
{
    private IBehavior[] children;

    public Selector() { }
    
    public Selector(params IBehavior[] children)
    {
        this.children = children;
    }

    public BehaviorResult DoBehavior()
    {
        foreach (var child in children)
        {
            if (child.DoBehavior() == BehaviorResult.SUCCESS)
            {
                return BehaviorResult.SUCCESS;
            }
        }

        return BehaviorResult.FAILURE;
    }
}

/// <summary>
/// Executes each child in the order that they're specified in.
/// If any of the children fail in their execution, the entire sequence fails.
/// </summary>
public class Sequence : IBehavior
{
    private IBehavior[] children;

    public Sequence() { }

    public Sequence(params IBehavior[] children)
    {
        this.children = children;
    }

    public BehaviorResult DoBehavior()
    {
        foreach (var child in children)
        {
            if (child.DoBehavior() == BehaviorResult.FAILURE)
            {
                return BehaviorResult.FAILURE;
            }
        }

        return BehaviorResult.SUCCESS;
    }
}