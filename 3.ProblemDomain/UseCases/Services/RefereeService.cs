using Common.BaseComponents.Components;
using Common.BaseExtensions.Collections;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;

namespace ProblemDomain.UseCases.Services;

/// <summary>
/// Сервис для работы с Судьями.
/// </summary>
/// <param name="repository">Репозиторий.</param>
/// <param name="problemErrorMsgProvider">Провайдер сообщений об ошибках слоя предметной области.</param>
public class RefereeService(IRepository repository, IProblemErrorMsgProvider problemErrorMsgProvider)
{
    /// <summary>
    /// Перенумерация коллекции судей.
    /// </summary>
    public void RenumberReferees(IList<Referee> refereesCollection)
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
            : Result<IList<RefereeLevel>>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeLevelsLoadError, refereeLevelsResult.Excptn)
            );
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
            : Result<IList<RefereeJobTitle>>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeJobTitlesLoadError, refereeJobTitlesResult.Excptn)
            );
    }

    /// <summary>
    /// Получение судей, связанных с соревнованием, и обновление ими коллекции.
    /// </summary>
    /// <param name="refereesCollection">Обновляемая коллекция судей.</param>
    /// <param name="competition">Текущее соревнование.</param>
    /// <returns>Индекс нового текущего судьи.</returns>
    public async Task<Result<bool>> GetRefereesAsync(
        IList<Referee> refereesCollection, CompetitionData? competition)
    {
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<bool>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.RefereesLoadError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }

        // Сбрасываем отслеживание сущностей
        var result = repository.DetachAll<Referee>();
        if (! result)
            return Result<bool>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereesLoadError, result.Excptn)
            );
        
        // Загружаем данные из репозитория
        var refereesResult = await repository.GetNumberedAllAsync<Referee>(
            true, 
            r => r.CompetitionDataId == competition.Id,
            nameof(RefereeLevel));
        if (! refereesResult)
            return Result<bool>.Fail(
                problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereesLoadError, refereesResult.Excptn)
            );
        
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
        IList<Referee> refereesCollection, int index, CompetitionData? competition)
    {
        ProblemException innerException;
        
        // Проверяем наличие соревнования
        if (competition == null)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(ProblemErrorCodes.RefereeCreateError,
                    problemErrorMsgProvider.CreateException(ProblemErrorCodes.CompetitionDataIsNull))
            );
        }

        // Получаем судейскую категорию
        var refereeLevelResult = await repository.FindAsync<RefereeLevel>(RefereeLevelEnm.Category3);
        if (! refereeLevelResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereeLevelFindError, refereeLevelResult.Excptn);
            
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, innerException)
            );
        }
        
        // Получаем судейскую должность
        var refereeJobTitleResult = await repository.FindAsync<RefereeJobTitle>(RefereeJobTitleEnm.StageReferee);
        if (! refereeJobTitleResult)
        {
            innerException = problemErrorMsgProvider.CreateException(
                ProblemErrorCodes.RefereeJobTitleFindError, refereeJobTitleResult.Excptn);
            
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, innerException)
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
        RenumberReferees(refereesCollection);
        
        // Добавляем судью в репозиторий
        var intResult = repository.Add(newReferee);
        if (! intResult)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeCreateError, intResult.Excptn)
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
    public Result<int> RemoveReferee(IList<Referee> refereesCollection, int index)
    {
        if (index >= refereesCollection.Count || index < 0)
            return Result<int>.Done(index);

        // Судья, которого удаляем
        var refereeToRemove = refereesCollection[index];
        
        // Удаляем из коллекции судью и перенумеровываем коллекцию
        refereesCollection.RemoveAt(index);
        RenumberReferees(refereesCollection);
        
        // Удаляем из репозитория
        var intResult = repository.Remove(refereeToRemove);
        if (! intResult)
        {
            return Result<int>.Fail(
                problemErrorMsgProvider.CreateException(
                    ProblemErrorCodes.RefereeRemoveError, intResult.Excptn)
            );
        }

        return Result<int>.Done(index == 0 ? 0 : index - 1);
    }
}