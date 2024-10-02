using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using TodoApp.Models;

namespace TodoApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
// [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class TodoController : ControllerBase
{
    private static readonly List<Todo> TodosDatabase = new List<Todo>
    {
        new Todo { Id = 1, Title = "Learn ASP.NET Core", IsComplete = false, UserId = "1" },
        new Todo { Id = 2, Title = "Build awesome apps", IsComplete = false, UserId = "1" },
        new Todo { Id = 3, Title = "Deploy to Azure", IsComplete = false, UserId = "1" }
    };

    private readonly ILogger<TodoController> _logger;

    private readonly GraphServiceClient _graphServiceClient;

    private readonly IMapper _mapper;

    public TodoController(ILogger<TodoController> logger, GraphServiceClient graphServiceClient, IMapper mapper)
    {
        _logger = logger;
        _graphServiceClient = graphServiceClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Get todos
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetTodos")]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos()
    {
        _logger.LogInformation("Getting todos");

        return Ok(_mapper.Map<IEnumerable<TodoDto>>(TodosDatabase));
    }

    /// <summary>
    /// Get todo by id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}", Name = "GetTodo")]
    public async Task<ActionResult<TodoDto>> GetTodo(int id)
    {
        _logger.LogInformation("Getting todo with id {id}", id);

        return TodosDatabase.FirstOrDefault(todo => todo.Id == id) switch
        {
            Todo todo => Ok(_mapper.Map<TodoDto>(todo)),
            _ => NotFound()
        };
    }

    /// <summary>
    /// Add todo
    /// </summary>
    /// <returns></returns>
    [HttpPost(Name = "CreateTodo")]
    public async Task<ActionResult<TodoDto>> CreateTodo(CreateTodoDto todoDTO)
    {
        _logger.LogInformation("Creating todo");

        var todo = new Todo
        {
            Id = TodosDatabase.Max(todo => todo.Id) + 1,
            Title = todoDTO.Title,
            IsComplete = todoDTO.IsComplete,
            UserId = "1" // Get user id from token
        };

        TodosDatabase.Add(todo);

        return CreatedAtRoute("GetTodo", new { id = todo.Id }, _mapper.Map<TodoDto>(todo));
    }

    /// <summary>
    /// Update todo by id
    /// </summary>
    /// <returns></returns>
    [HttpPut("{id}", Name = "UpdateTodo")]
    public async Task<ActionResult> UpdateTodo(int id, UpdateTodoDto todoDto)
    {
        _logger.LogInformation("Updating todo with id {id}", id);

        var todo = TodosDatabase.FirstOrDefault(todo => todo.Id == id);

        if (todo is null)
        {
            return NotFound();
        }

        _mapper.Map(todoDto, todo);

        return NoContent();
    }

    /// <summary>
    /// Delete todo by id
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}", Name = "DeleteTodo")]
    public async Task<ActionResult> DeleteTodo(int id)
    {
        _logger.LogInformation("Deleting todo with id {id}", id);

        var todo = TodosDatabase.FirstOrDefault(todo => todo.Id == id);

        if (todo is null)
        {
            return NotFound();
        }

        var index = TodosDatabase.FindIndex(todo => todo.Id == id);

        if (index >= 0)
        {
            TodosDatabase.RemoveAt(index);
        }

        return NoContent();
    }
}
