using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using ProblemDomain.Entities;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Services;

public class CompetitionService
{
    /// <summary>
    /// Получение/создание списка судей из репозитория.
    /// </summary>
    private async Task<Result<List<Referee>>> GetOrCreateRefereesAsync(IRepository repository,
        Referee? chiefReferee, 
        Referee? chiefSecretary,
        List<Referee>? secretaries,
        Referee? mandateChairman)
    {
        List<Referee> foundAllReferees = [];
        ExceptionList<BaseException> exceptionsList = [];

        var allReferees = new List<Referee>(secretaries ?? []);
        if (chiefReferee is not null)
            allReferees.Add(chiefReferee);
        if (chiefSecretary is not null)
            allReferees.Add(chiefSecretary);
        if (mandateChairman is not null)
            allReferees.Add(mandateChairman);

        foreach (var referee in allReferees)
        {
            var foundReferee = // получаем из репозитория
                (await repository.GetFromIdAsync<Referee>((int)referee.Id));

            Result<int> result;
            if (foundReferee is null)
                result = await repository.AddAsync(referee);
            else
            {
                referee.Copy(foundReferee);
                result = await repository.UpdateAsync(foundReferee);
            }
            
            if (! result)
                exceptionsList.Add(new BaseException(
                    $"Failed to create or get the referee '{referee.Id}' from the repository.", result.Excptn, 
                    "ru", $"Не удалось создать или получить судью '{referee.Id}' из репозитория."));
            
            // Добавляем судью в конечный список
            foundAllReferees.Add(foundReferee ?? referee);
        }
        
        // Добавляем полученный список ошибок в список репозитория
        repository.ExceptionsList.AddRange(exceptionsList);

        return exceptionsList.IsNoExceptions
            ? Result<List<Referee>>.Done(foundAllReferees)
            : Result<List<Referee>>.Fail(exceptionsList.GetFirst());
    }
    
    /// <summary>
    /// Создание экземпляра <see cref="Competition"/> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="Competition(string, string, string, DateTime, int, string)"/>
    /// <param name="repository">Репозиторий.</param>
    /// <param name="competitionsStatus">Статус соревнования.</param>
    /// <param name="detailedCompetitionStatus">Статус и обобщенное наименование соревнования.</param>
    /// <param name="chiefReferee">Главный судья.</param>
    /// <param name="chiefSecretary">Главный секретарь.</param>
    /// <param name="mandateChairman">Председатель комиссии по допуску.</param>
    /// <param name="secretaries">Список секретарей.</param>
    public async Task<Result<Competition>> CreateAsync(IRepository repository, string name, 
        string conductingOrganizations, string venue, DateTime date, int dayCount,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        Referee chiefReferee, Referee chiefSecretary, Referee mandateChairman,
        List<Referee> secretaries, string? description = null)
    {
        // Все судьи
        var resultReferees =
            await GetOrCreateRefereesAsync(repository, chiefReferee, chiefSecretary, secretaries, mandateChairman);
        
        if (! resultReferees)
        {
            return Result<Competition>.Fail(resultReferees.Excptn);
        }

        // Секретари
        secretaries = resultReferees.Value.Where(r => r.JobTitleId >= JobTitleEnm.Secretary1).ToList();
        
        // Создаем соревнование и добавляем в репозиторий (или получаем из него)
        var competition =
            new Competition(name, conductingOrganizations, date, dayCount, venue,
                competitionsStatus, detailedCompetitionStatus,
                resultReferees.Value.FirstOrDefault(r => r.JobTitleId == JobTitleEnm.ChiefReferee)!,
                resultReferees.Value.FirstOrDefault(r => r.JobTitleId == JobTitleEnm.ChiefSecretary)!,
                resultReferees.Value.FirstOrDefault(r => r.JobTitleId == JobTitleEnm.MandateChairman)!,
                secretaries, description);
        
        var foundCompetition = // получаем из репозитория
            (await repository.GetAllFromNameAsync<Competition>(competition.Name)).FirstOrDefault();
        
        Result<int> result;
        if (foundCompetition is null)
            result = await repository.AddAsync(competition);
        else
        {
            competition.Copy(foundCompetition);
            result = await repository.UpdateAsync(foundCompetition);
        }
        
        return result 
            ? Result<Competition>.Done(competition) 
            : Result<Competition>.Fail(result.Excptn);
    }
}