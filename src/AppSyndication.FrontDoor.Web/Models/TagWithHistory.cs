using System.Collections.Generic;
using AppSyndication.BackendModel.IndexedData;

namespace AppSyndication.FrontDoor.Web.Models
{
    public class TagWithHistory : Tag
    {
        public TagWithHistory(Tag tag, IEnumerable<TagHistory> history)
            : base(tag)
        {
            this.History = history;
        }

        public IEnumerable<TagHistory> History { get; }
    }
}
