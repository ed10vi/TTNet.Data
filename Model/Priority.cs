namespace TTNet.Data;

/// <summary>
/// Message schedule
/// </summary>
public enum Priority
{
    /// <summary>
    /// The lowest priority (default).
    /// </summary>
    Lowest,
    /// <summary>
    /// Low priority.
    /// </summary>
    Low,
    /// <summary>
    /// Priority below normal.
    /// </summary>
    BelowNormal,
    /// <summary>
    /// Normal priority.
    /// </summary>
    Normal,
    /// <summary>
    /// Priority above normal.
    /// </summary>
    AboveNormal,
    /// <summary>
    /// High priority.
    /// </summary>
    High,
    /// <summary>
    /// The highest priority.
    /// </summary>
    Highest
}
