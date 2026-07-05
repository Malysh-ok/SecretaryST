using System.Collections.ObjectModel;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;

namespace AppDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Судьями.
/// </summary>
/// <param name="repository">Репозиторий.</param>
public class RefereeService(IRepository repository)
{
    /// <summary>
    /// Перенумерация коллекции судей.
    /// </summary>
    public void RenumberRefereesCollection(ObservableCollection<Referee> refereesCollection)
    {
        var referees = refereesCollection.ToList();
        refereesCollection.Clear();
        for (var i = 0; i < referees.Count; i++)
        {
            referees[i].Number = i + 1;
            refereesCollection.Add(referees[i]);
        }
    }
    
    /// <summary>
    /// Получение списка судейских категорий.
    /// </summary>
    public async Task<Result<IList<RefereeLevel>>> GetRefereeLevelsAsync()
    {
        var refereeLevelsResult = await repository.GetAllAsync<RefereeLevel>();

        return refereeLevelsResult
            ? Result<IList<RefereeLevel>>.Done(refereeLevelsResult.Value!)
            : Result<IList<RefereeLevel>>.Fail(new AppException(AppPhrases.RefereeLevelsLoadError, refereeLevelsResult.Excptn));
    }
    
    /// <summary>
    /// Получение списка судейских должностей.
    /// </summary>
    public async Task<Result<IList<RefereeJobTitle>>> GetRefereeJobTitlesAsync()
    {
        var refereeJobTitlesResult =
            (await repository.GetAllAsync<RefereeJobTitle>());

        return refereeJobTitlesResult 
            ? Result<IList<RefereeJobTitle>>.Done(refereeJobTitlesResult.Value!) 
            : Result<IList<RefereeJobTitle>>.Fail(new AppException(AppPhrases.RefereeJobTitlesLoadError, refereeJobTitlesResult.Excptn));
    }

    /// <summary>
    /// Получение судей, связанных с соревнованием, и обновление ими коллекции.
    /// </summary>
    /// <param name="refereesCollection">Обновляемая коллекция судей.</param>
    /// <param name="competition">Текущее соревнование.</param>
    /// <returns>Индекс нового текущего судьи.</returns>
    public async Task<Result<bool>> GetRefereesAsync(
        ObservableCollection<Referee> refereesCollection, CompetitionData? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<bool>.Fail(
                new AppException(AppPhrases.RefereesLoadError, new AppException(AppPhrases.CompetitionDataIsNull))
            );
        }

        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<Referee>();
        if (! result)
            return Result<bool>.Fail(new AppException(AppPhrases.RefereesLoadError, result.Excptn));
        
        // Загружаем данные из репозитория
        var refereesResult = await repository.GetNumberedAllAsync<Referee>(
            true, 
            r => r.CompetitionDataId == competition.Id,
            nameof(RefereeLevel));
        if (! refereesResult)
            return Result<bool>.Fail(new AppException(AppPhrases.RefereesLoadError, refereesResult.Excptn));
        
        // Перезаписываем коллекцию судей новыми данными
        refereesCollection.Clear();
        refereesResult.Value.ForEach(refereesCollection.Add);

        return Result<bool>.Done(true);
    }

    /// <summary>
    /// Создание нового судьи и добавление в коллекцию после текущего.
    /// </summary>
    /// <param name="refereesCollection">Обновляемая коллекция судей.</param>
    /// <param name="index">Индекс текущего судьи.</param>
    /// <param name="competition">Текущее соревнование.</param>
    /// <returns>Индекс нового текущего судьи.</returns>
    public async Task<Result<int>> CreateRefereeAsync(
        ObservableCollection<Referee> refereesCollection, int index, CompetitionData? competition)
    {
        AppException innerException;
        
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeCreateError, new AppException(AppPhrases.CompetitionDataIsNull))
            );
        }

        // Получаем судейскую категорию
        var refereeLevelResult = await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3);
        if (! refereeLevelResult)
        {
            innerException = new AppException(AppPhrases.RefereeLevelFindError, refereeLevelResult.Excptn);
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeCreateError, innerException)
            );
        }
        
        // Получаем судейскую должность
        var refereeJobTitleResult = await repository.FindAsync<RefereeJobTitle>(RefereeJobTitleEnm.StageReferee);
        if (! refereeJobTitleResult)
        {
            innerException = new AppException(AppPhrases.RefereeJobTitleFindError, refereeJobTitleResult.Excptn);
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeCreateError, innerException)
            );
        }
        
        // Новый индекс
        var newIndex = (index >= refereesCollection.Count || index < 0)
            ? refereesCollection.Count
            : index + 1;

        var newReferee = new Referee(newIndex + 1,
            "ФАМИЛИЯ",
            "ИМЯ",
            "ТЕРРИТОРИЯ",
            refereeLevelResult.Value!,
            refereeJobTitleResult.Value!,
            competition);
        
        // Добавляем в коллекцию судью и перенумеровываем коллекцию
        refereesCollection.Insert(newIndex, newReferee);
        RenumberRefereesCollection(refereesCollection);
        
        // Обновляем судей в репозитории
        var intResult = repository.UpdateRange(refereesCollection);
        if (! intResult)
        {
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeCreateError, intResult.Excptn)
            );
        }

        return Result<int>.Done(newIndex);
    }
    
    /// <summary>
    /// Удаление судьи из коллекции.
    /// </summary>
    /// <param name="refereesCollection">Текущая коллекция судей.</param>
    /// <param name="index">Индекс удаляемого судьи в коллекции.</param>
    /// <returns>Индекс нового текущего судьи.</returns>
    public Result<int> RemoveReferee(ObservableCollection<Referee> refereesCollection, int index)
    {
        if (index >= refereesCollection.Count || index < 0)
            return Result<int>.Done(index);

        // Судья, которого удаляем
        var refereeToRemove = refereesCollection[index];
        
        // Удаляем из коллекции судью и перенумеровываем коллекцию
        refereesCollection.RemoveAt(index);
        RenumberRefereesCollection(refereesCollection);
        
        // Удаляем из репозитория
        var intResult = repository.Remove(refereeToRemove);
        if (! intResult)
        {
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(index == 0 ? 0 : index - 1);
    }
}