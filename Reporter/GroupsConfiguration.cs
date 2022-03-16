using System.Collections.Generic;

namespace Reporter
{
    class GroupsConfiguration
    {
        public ObjectType Type { get; set; }
        public List<Range> Ranges { get; set; }

        public string GetNameOfRange(int protectionNumber)
        {
            foreach (var range in Ranges)
            {
                if (range.IsContains(protectionNumber))
                    return range.Name;
            }

            return "<No group>";
        }
    }
}
