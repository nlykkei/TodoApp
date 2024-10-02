using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
  /// <summary>
  /// Data transfer object for updating a todo item.
  /// </summary>
  public class UpdateTodoDto
  {
    /// <summary>
    /// Gets or sets the todo's title.
    /// </summary>
    [Required]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the todo is complete.
    /// </summary>
    public bool IsComplete { get; set; } = false;
  }
}