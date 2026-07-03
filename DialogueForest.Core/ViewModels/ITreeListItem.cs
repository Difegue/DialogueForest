namespace DialogueForest.Core.ViewModels
{
    /// <summary>
    /// Common surface for items shown in the ordered node list (real nodes and markers),
    /// so a single collection + template selector can host both.
    /// </summary>
    public interface ITreeListItem
    {
        /// <summary>True for markers, false for real dialogue nodes.</summary>
        bool IsMarker { get; }
    }
}
