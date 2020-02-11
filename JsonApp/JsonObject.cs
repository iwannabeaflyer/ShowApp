using System.Collections.Generic;

namespace JsonApp
{
    public class JsonObject
    {
        public JsonObject() { Items = new List<ModelItem>(); }
        public List<ModelItem> Items { get; set; }
    }
}
