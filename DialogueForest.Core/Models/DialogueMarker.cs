namespace DialogueForest.Core.Models
{
    /// <summary>
    /// An in-sequence divider that groups the nodes following it within a <see cref="DialogueTree"/>
    /// (e.g. "Chapter 1"). 
    /// </summary>
    public class DialogueMarker
    {
        public DialogueMarker() { }

        /// <summary>
        /// Stable UUID. 
        /// Markers are not part of the regular Dialog ID system as they're not in final exports. 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display label shown in the list/tree.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// ID of the node this marker sits immediately before in the tree's ordered node list.
        /// 0 means the marker is trailing (after all nodes / on an empty group).
        /// </summary>
        public long BeforeNodeId { get; set; }
    }
}
