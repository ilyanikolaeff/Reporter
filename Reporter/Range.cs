using System.Linq;

namespace Reporter
{
    class Range
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsContains(int protectionNumber)
        {
            var rangeParts = Value.Split(',');
            foreach (var rangePart in rangeParts)
            {
                var trimmedPart = rangePart.Replace(" ", "");
                if (trimmedPart.Contains("-"))
                {
                    var parts = trimmedPart.Split('-');
                    var startValue = int.Parse(parts[0]);
                    var endValue = int.Parse(parts[1]);

                    if (protectionNumber >= startValue || protectionNumber <= endValue)
                        return true;
                }
                else
                {
                    var value = int.Parse(trimmedPart);
                    if (protectionNumber == value)
                        return true;
                }
            }
            return false;
        }
    }
}
