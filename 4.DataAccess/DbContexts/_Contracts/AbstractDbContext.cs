using Common.BaseExtensions;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DbContexts._Contracts
{
    /// <summary>
    /// Абстрактный контекст БД.
    /// </summary>
    public abstract class AbstractDbContext : DbContext
    {
        /// <summary>
        /// Строка подключения к БД.
        /// </summary>
        protected string? ConnectionString { get; init; } = string.Empty;

        /// <summary>
        /// Признак того, что строка подключения к БД равна null.
        /// </summary>
        public bool IsNullOrEmptyConnectionString => ConnectionString.IsNullOrEmpty();
        
        /// <summary>
        /// Признак того, что к БД можно подключиться.
        /// </summary>
        public bool IsPossibleConnect => Database.CanConnect();

        /// <summary>
        /// Конструктор, запрещающий создание объекта без параметров.
        /// </summary>
        private AbstractDbContext()
        {
        }
        
        /// <summary>
        /// Конструктор.
        /// </summary>
        protected AbstractDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Сбросить начальные значения автоинкремента
        /// для каждого из типа сущности массива <paramref name="entityTypes"/>.
        /// </summary>
        /// <param name="autoincrementColumnName">столбец, у которого сбрасывается автоинкремент
        /// (необходимость данного параметра определяется провайдером БД).</param>
        /// <param name="entityTypes">массив типов сущностей, в которых сбрасываем автоинкремент.</param>
        public abstract void ClearAutoincrementSequence(string autoincrementColumnName, params Type[] entityTypes);
    }
}