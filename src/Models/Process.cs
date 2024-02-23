using System;
using System.Collections.Generic;
using System.Text;

namespace AzdoBoardsManager.Models
{
    public class Process : IDefinition
    {
        public string Description { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEnabled { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }

        public string ReferenceName { get; set; }

        public Guid ParentProcessTypeId { get; set; }

        public string ParentProcessId { get; set; }

        private const string DefinitionType = "process";

        public string GetDefinitionType()
        {
            return DefinitionType;
        }

        public string GetDefinitionName()
        {
            return ReferenceName ?? Name;
        }
    }
}
