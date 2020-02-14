using System.Collections.Generic;

namespace JsonApp
{
    public class JsonObject
    {
        public List<ModelItem> Items { get; set; }

        public JsonObject() { Items = new List<ModelItem>(); }
    }
}