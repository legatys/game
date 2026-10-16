public class JumpState
{
    public int maxJumps;
    private int _jumpsAvailable;

    public void ResetJumps()
    {
        _jumpsAvailable = maxJumps;
    }

    public bool TryConsumeJump()
    {
        if (_jumpsAvailable <= 0) return false;
        _jumpsAvailable--;
        return true;
    }
}