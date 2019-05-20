using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace AzdoBoardsManager.Exporter
{
    public abstract class Base : IExporter
    {
        public string BaseFolder { get; set; } = string.Empty;

        public T ConvertObject<T>(object origin) where T : Models.IDefinition
        {
            var serializedObject = JsonConvert.SerializeObject(origin, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var convertedObject = JsonConvert.DeserializeObject<T>(serializedObject);
            ProcessDefinition(convertedObject);
            return convertedObject;
        }

        public void SaveObject<T>(object rawDefinition) where T : Models.IDefinition
        {
            if (!System.IO.Directory.Exists(BaseFolder))
            {
                System.IO.Directory.CreateDirectory(BaseFolder);
            }

            var definition = ConvertObject<T>(rawDefinition);
            var definitionString = JsonConvert.SerializeObject(definition, Formatting.Indented);

            var definitionName = definition.GetDefinitionName().Replace('\\', '_').Replace('/', '_');
            System.IO.File.WriteAllText(System.IO.Path.Combine(BaseFolder, definition.GetDefinitionType() + "." + definitionName + ".json"), definitionString);
        }

        public virtual void ProcessDefinition<T>(T definition) where T:Models.IDefinition
        {
        }

        public abstract void Generate(VssConnection connection);
    }
}
