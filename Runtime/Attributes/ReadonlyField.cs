using System;

namespace VisualNovelFramework.GraphFramework.Attributes
{
    /// <summary>
    /// When using the default BaseNodeUI layout system, this will automatically set a field to
    /// disabled when laid out by the UI so it is visible but not editable by the user.
    /// </summary>
    public class ReadonlyField : Attribute
    {
    }
}