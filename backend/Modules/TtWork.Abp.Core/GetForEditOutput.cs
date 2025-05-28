using Newtonsoft.Json.Linq;

namespace TtWork.Abp.Core
{
    public class GetForEditOutput<T> : IHaveSchema
    {
        public GetForEditOutput(T data, JToken schema)
        {
            Data = data;
            Schema = schema;
        }
        public T Data { get; set; }
        public JToken Schema { get; set; }
    }

    public interface IHaveSchema
    {
        JToken Schema { get; set; }
    }

    public interface ICanSchema
    {

    }
}