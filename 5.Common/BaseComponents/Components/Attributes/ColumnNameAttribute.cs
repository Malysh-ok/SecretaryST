using System;

namespace Common.BaseComponents.Components.Attributes;

public class ColumnNameAttribute : Attribute
{
    /// <inheritdoc />
    public ColumnNameAttribute(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Наименование столбца.
    /// </summary>
    public string Name { get; set; }
}