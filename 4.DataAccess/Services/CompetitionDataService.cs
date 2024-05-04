using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Exceptions;
using ProblemDomain.Entities;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;

// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Services;

/// <summary>
/// Сервис для работы с данными о соревнованиях.
/// </summary>
public class CompetitionDataService
{
    /// <summary>
    /// Создание экземпляра <see cref="CompetitionData"/> (фабричный метод).
    /// </summary>
    /// <inheritdoc cref="CompetitionData"/>
    /// <param name="repository">Репозиторий.</param>
    /// <param name="competitionsStatus">Статус соревнования.</param>
    /// <param name="detailedCompetitionStatus">Статус и обобщенное наименование соревнования.</param>
    public async Task<Result<CompetitionData>> CreateAsync(IRepository repository, string name, 
        string conductingOrganizations, DateTime date, int dayCount, string venue,
        CompetitionsStatus competitionsStatus, DetailedCompetitionStatus detailedCompetitionStatus,
        string? description = null)
    {
        // Удаляем данные о соревновании из репозитория
        var result = await repository.RemoveRangeAsync(await repository.GetAllAsync<CompetitionData>());
        if (! result)
            return Result<CompetitionData>.Fail(
                new BaseException(
                    $"Failed to delete the competition data from the repository.", result.Excptn,
                    "ru", $"Не удалось удалить данные о соревновании из репозитория.")
            );
        
        // Создаем данные о соревновании и добавляем в репозиторий
        var competition =
            new CompetitionData(name, conductingOrganizations, date, dayCount, venue,
                competitionsStatus, detailedCompetitionStatus,
                description);
        result = await repository.AddAsync(competition);
        
        return result 
            ? Result<CompetitionData>.Done(competition)
            : Result<CompetitionData>.Fail(
                new BaseException(
                    $"Failed to save the competition data to the repository.", result.Excptn,
                    "ru", $"Ошибка при сохранении данных о соревновании в репозитории.")
            );
    }
}