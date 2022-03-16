using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reporter
{
    /// <summary>
    /// Сгруппированные защиты
    /// </summary>
    abstract class ObjectsGroup
    {
        public string Name { get; set; }
        public int ActivationsCount { get; set; }
        public TimeSpan Duration { get; set; }
        public double DurationInSeconds => Duration.TotalSeconds;
        public double DurationInDays => Duration.TotalDays;
        public string Tag { get; set; }

        public void Add(ReportObject reportObject)
        {
            if (reportObject.Duration > TimeSpan.Zero)
                ActivationsCount++;
            Duration += reportObject.Duration;
        }
    }

    /// <summary>
    /// Обычные защиты группируются по тегу
    /// </summary>
    class CommonProtectionsGroup : ObjectsGroup
    {
    }

    /// <summary>
    /// Флаговые объекты группируются по номеру (номер = номер флага * 32 + номер бита) и тегу флага 
    /// </summary>
    abstract class FlagObjectsGroup : ObjectsGroup
    {      
        public int Number { get; set; }
    }

    class SecondLevelObjects : FlagObjectsGroup
    {
    }

    class TorObjectsGroup : FlagObjectsGroup
    {
    }
}