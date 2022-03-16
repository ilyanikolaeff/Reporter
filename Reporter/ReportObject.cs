using System;

namespace Reporter
{
    abstract class ReportObjectBase
    {

    }

    abstract class ReportObject : ReportObjectBase
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public DateTime TimeSet { get; set; }
        public DateTime TimeUnset { get; set; }
        public TimeSpan Duration => TimeUnset - TimeSet;
        public ObjectType Type { get; set; }

        public override string ToString()
        {
            return $"{Tag} - {Name} - {TimeSet} - {TimeUnset} - {Type}";
        }
    }

    /// <summary>
    /// Обычная защита
    /// </summary>
    class CommonObject : ReportObject
    { 
        public CommonObject()
        {
            Type = ObjectType.CommonObj;
        }
    }

    /// <summary>
    /// Защита второго уровня (необычная защита)
    /// </summary>
    abstract class FlagObject : ReportObject 
    {
        public int FlagNumber { get; set; }
        public int BitNumber { get; set; }

        public int Number => FlagNumber * 32 + BitNumber;
    }

    class SecondLevelProtection : FlagObject
    { 
    }

    class TorObject : FlagObject
    {
        public TorObject()
        {
            Type = ObjectType.TorFlag;
        }
    }
}
