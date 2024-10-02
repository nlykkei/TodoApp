namespace TodoApp.Models
{
  /// <summary>
  /// Data transfer object (DTO) for a todo item.
  /// </summary>
  public class TodoDto
  {
    /// <summary>
    /// Gets or sets the todo's identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the todo's title.
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the todo is complete.
    /// </summary>
    public bool IsComplete { get; set; }
  }
}