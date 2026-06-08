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
    /// Перенумеровываем коллекцию судей.
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
    /// Получение судейских категорий.
    /// </summary>
    public async Task<Result<IList<RefereeLevel>>> GetRefereeLevelsAsync()
    {
        var refereeLevelsResult = await repository.GetAllAsync<RefereeLevel>();

        return refereeLevelsResult
            ? Result<IList<RefereeLevel>>.Done(refereeLevelsResult.Value!)
            : Result<IList<RefereeLevel>>.Fail(new AppException(AppPhrases.RefereeLevelsLoadError, refereeLevelsResult.Excptn));
    }
    
    /// <summary>
    /// Получение судейских должностей.
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
    /// Обновление коллекции судей.
    /// </summary>
    /// <param name="refereesCollection">Текущая коллекция судей.</param>
    /// <param name="index">Индекс выбранного судьи.</param>
    public async Task<Result<int>> GetRefereesAsync(
        ObservableCollection<Referee> refereesCollection, int index)
    {
        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<Referee>();
        if (! result)
            return Result<int>.Fail(new AppException(AppPhrases.RefereesLoadError, result.Excptn));
        
        // Загружаем данные из репозитория
        var refereesResult = await repository.GetNumberedAllAsync<Referee>(
            true, nameof(RefereeLevel));
        if (! refereesResult)
            return Result<int>.Fail(new AppException(AppPhrases.RefereesLoadError, result.Excptn));
        
        // Перезаписываем коллекцию судей новыми данными
        refereesCollection.Clear();
        refereesResult.Value.ForEach(refereesCollection.Add);

        return Result<int>.Done(index);
    }
    
    /// <summary>
    /// Добавление нового судьи на указанную позицию.
    /// </summary>
    public async Task<Result<int>> AddRefereeAsync(
        ObservableCollection<Referee> refereesCollection, int index)
    {
        AppException innerException;
        var newIndex = (index >= refereesCollection.Count || index < 0)
            ? refereesCollection.Count
            : index + 1;

        // Получаем судейские категории
        var refereeLevelResult = await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3);
        if (! refereeLevelResult)
        {
            innerException = new AppException(AppPhrases.RefereeLevelFindError, refereeLevelResult.Excptn);
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeAddError, innerException)
            );
        }
        
        // Получаем судейские должности
        var refereeJobTitleResult = await repository.FindAsync<RefereeJobTitle>(RefereeJobTitleEnm.StageReferee);
        if (! refereeJobTitleResult)
        {
            innerException = new AppException(AppPhrases.RefereeJobTitleFindError, refereeJobTitleResult.Excptn);
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeAddError, innerException)
            );
        }

        var newReferee = new Referee(newIndex + 1,
            "ФАМИЛИЯ",
            "ИМЯ",
            "ТЕРРИТОРИЯ",
            refereeLevelResult.Value!,
            refereeJobTitleResult.Value!);
        
        // Добавляем в коллекцию судью и перенумеровываем коллекцию
        refereesCollection.Insert(newIndex, newReferee);
        RenumberRefereesCollection(refereesCollection);
        
        // Обновляем судей в репозитории
        var intResult = repository.UpdateRange(refereesCollection);
        if (! intResult)
        {
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeAddError, intResult.Excptn)
            );
        }

        return Result<int>.Done(newIndex);
    }
    
    /// <summary>
    /// Удаление судьи.
    /// </summary>
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
        
        // Обновляем судей в репозитории
        intResult = repository.UpdateRange(refereesCollection);
        if (! intResult)
        {
            return Result<int>.Fail(
                new AppException(AppPhrases.RefereeRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(index == 0 ? 0 : index - 1);
    }
    
    /// <summary>
    /// Сохранение списка судей.
    /// </summary>
    /// <param name="referees">Список судей.</param>
    public async Task<Result<int>> SaveRefereesAsync(IEnumerable<Referee> referees)
    {
        var intResult = await repository.SaveChangesAsync();
        
        return intResult
            ? intResult
            : Result<int>.Fail(new AppException(AppPhrases.RefereesSaveError, intResult.Excptn));
    }
}