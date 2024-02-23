using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace AzdoBoardsManager.Exporter
{
    public interface IExporter
    {
        T ConvertObject<T>(object origin) where T : Models.IDefinition;

        void SaveObject<T>(object rawDefinition) where T : Models.IDefinition;

        void Generate(VssConnection connection);
    }
}
