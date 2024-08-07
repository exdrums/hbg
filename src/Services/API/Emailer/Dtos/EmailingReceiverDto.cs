namespace API.Emailer.Dtos;

/// <summary>
/// Dtos indicates the Receiver is used in the Distribution
/// => used to Create and Remove Email objects
/// </summary>
public class EmailingReceiverDto : ReceiverDto
{
    /// <summary>
    /// true => Email exists, false => not
    /// </summary>
    public bool Assigned { get; set; }
}

public class EmailingReceiverUpdateDto
{
    /// <summary>
    /// true => Email exists, false => not
    /// </summary>
    public bool Assigned { get; set; }
}
