namespace Hash.Data.Models;

public class PostHashesInputModel
{
    public int NumOfMessages { get; set; } = 40000;
    public int BatchSize { get; set; } = 1000;
}