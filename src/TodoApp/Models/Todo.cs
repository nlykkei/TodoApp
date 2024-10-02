using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
  /// <summary>
  /// A todo item.
  /// </summary>
  public class Todo
  {
    /// <summary>
    /// Gets or sets the todo's identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the todo's title.
    /// </summary>
    [Required]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the todo is complete.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets the user identifier associated with the todo.
    /// </summary>
    [Required]
    public string UserId { get; set; } = default!;
  }

  public static class TodoMappingExtensions
  {
    public static TodoDto ToDto(this Todo todo)
    {
      return new TodoDto
      {
        Id = todo.Id,
        Title = todo.Title,
        IsComplete = todo.IsComplete
      };
    }
  }
}