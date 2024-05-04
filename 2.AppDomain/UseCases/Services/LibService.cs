using AppDomain.UseCases._Contracts;
using ProblemDomain.Entities.LibraryEntities;

namespace AppDomain.UseCases.Services;

// TODO UseCases - временно
public class LibService(IRepository repository)
{
    private readonly IRepository _repository = repository;

    public async Task<IEnumerable<Discipline>> GetDisciplines()
    {
        var disciplines = await _repository.GetAllAsync<Discipline>();

        return disciplines;
    }
}