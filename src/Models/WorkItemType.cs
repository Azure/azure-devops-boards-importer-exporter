using System;

namespace AzdoBoardsManager.Models
{
    public class WorkItemType : IDefinition
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public string Inherits { get; set; }

        public int Class { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

        public bool IsDisabled { get; set; }

        public string ProcessId { get; set; }

        private const string DefinitionType = "workitemtype";

        public string GetDefinitionType()
        {
            return DefinitionType;
        }

        public string GetDefinitionName()
        {
            return Id;
        }
    }
}

