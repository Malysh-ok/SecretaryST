using System;
using System.Collections.Generic;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.DistanceEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
// ReSharper disable InvalidXmlDocComment

namespace ProblemDomain.Entities.LibraryEntities
{
    /// <summary>
    /// Дисциплина.
    /// </summary>
    public sealed class Discipline
        : AbstractEntity, ICloneable, ICopy
    {
        /// <summary>
        /// Конструктор для EF.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <inheritdoc cref="AbstractEntity(string, string)"/>
        private Discipline(DisciplineEnm id, string name, string? description = null) 
            : base(name, description)
        {
            Id = id;
        }
        
        /// <summary>
        /// Конструктор на основе готового экземпляра.
        /// </summary>
        private Discipline(Discipline discipline)
            : this(
                discipline.Id,
                discipline.Name,
                discipline.DisciplineGroup,
                discipline.DisciplineSubGroup,
                discipline.Description
            )
        {
        }
        
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <inheritdoc />
        /// <param name="disciplineGroup">Группа дисциплин.</param>
        /// <param name="disciplineSubGroup">Подгруппа дисциплин.</param>
        public Discipline(DisciplineEnm id, string name, 
            DisciplineGroup disciplineGroup, DisciplineSubGroup disciplineSubGroup, string? description = null)
            : this(id, name, description)
        {
            DisciplineGroup = disciplineGroup;
            DisciplineSubGroup = disciplineSubGroup;
        }
        
        /// <inheritdoc cref="AbstractEntity.Id"/>
        public new DisciplineEnm Id { get; set; }

        /// <summary>
        /// Связь с подгруппой дисциплин (объектом-владельцем).
        /// </summary>
        public DisciplineSubGroupEnm DisciplineSubGroupId { get; set; }

        /// <inheritdoc cref="DisciplineSubGroupId"/>
        public DisciplineSubGroup DisciplineSubGroup { get; set; } = null!;

        /// <summary>
        /// Связь с группой дисциплин (объектом-владельцем).
        /// </summary>
        public DisciplineGroupEnm DisciplineGroupId { get; set; }

        /// <inheritdoc cref="DisciplineGroupId"/>
        public DisciplineGroup DisciplineGroup { get; set; } = null!;
        
        /// <summary>
        /// Список видов программ.
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Global
        public ICollection<SportEvent> SportEvents { get; set; } = new HashSet<SportEvent>();

        /// <summary>
        /// Клонирование.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public Discipline Clone()
            => new(this);
    
        /// <inheritdoc />
        object ICloneable.Clone() {
            return Clone();
        }
    
        /// <inheritdoc cref="ICopy.Copy"/>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Copy(Discipline destination)
        {
            destination.Id = Id;
            destination.Name = Name;
            destination.Description = Description;
            destination.DisciplineGroup = DisciplineGroup;
            destination.DisciplineSubGroup = DisciplineSubGroup;
        }
    
        /// <inheritdoc />
        void ICopy.Copy(IAbstractEntity destination)
        {
            Copy((Discipline)destination);
        }
    }
}