namespace AzdoBoardsManager.Models
{
    public class WorkItemTypeState : IDefinition
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public string StateCategory { get; set; }

        public string WorkItemId { get; set; }

        public int Order { get; set; }

        public bool Hidden { get; set; }

        private const string DefinitionType = "workitemtypestate";

        public string GetDefinitionType()
        {
            return DefinitionType;
        }

        public string GetDefinitionName()
        {
            return string.Concat(WorkItemId, "_", Name);
        }
    }
}

