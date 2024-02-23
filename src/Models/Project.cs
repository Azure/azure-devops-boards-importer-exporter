namespace AzdoBoardsManager.Models
{
    public class Project : IDefinition
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int Visibility { get; set; }

        private const string DefinitionType = "project";

        public string GetDefinitionType()
        {
            return DefinitionType;
        }

        public string GetDefinitionName()
        {
            return Name;
        }
    }
}
