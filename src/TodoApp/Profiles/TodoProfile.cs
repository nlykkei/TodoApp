using AutoMapper;
using TodoApp.Models;

namespace CityInfo.API.Profiles
{
  public class TodoProfile : Profile
  {
    public TodoProfile()
    {
      CreateMap<Todo, TodoDto>();
      CreateMap<Todo, CreateTodoDto>();
      CreateMap<Todo, UpdateTodoDto>();
    }
  }
}
