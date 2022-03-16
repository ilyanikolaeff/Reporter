using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Reporter
{
    /// <summary>
    /// Группировщик защит
    /// </summary>
    class ObjectGrouper
    {
        private string _reportName;
        public List<ObjectsGroup> GroupsObjects(IEnumerable<ReportObject> reportObjects, string reportName)
        {
            _reportName = reportName;

            var groupsConfig = new ConfigurationProvider().GetGroupsConfigurations(); // берем конфигурацию групп защит (для второго уровня)

            // Выделяем отдельно защиты и экспортируем их
            // Выделяем защиты первого уровня
            var commonProtections = reportObjects.Where(p => p is CommonObject);
            // Защиты второго уровня
            var secondLevelProtections = reportObjects.Where(p => p is SecondLevelProtection);
            // Объекты ТОР
            var torObjects = reportObjects.Where(p => p is TorObject);

            var groups = new List<ObjectsGroup>();
            var commonGroups = GroupCommonProtections(commonProtections);
            var secondLevelGroups = GroupSecondLevelObjects(secondLevelProtections, groupsConfig);
            var torGroups = GroupTorObjects(torObjects);

            groups.AddRange(commonGroups);
            groups.AddRange(secondLevelGroups);
            groups.AddRange(torGroups);

            return groups;
        }

        private List<CommonProtectionsGroup> GroupCommonProtections(IEnumerable<ReportObject> commonProts)
        {
            var groups = new List<CommonProtectionsGroup>();
            foreach (var protection in commonProts)
            {
                var existGroup = groups.FirstOrDefault(p => p.Tag == protection.Tag);
                if (existGroup != null)
                    existGroup.Add(protection);
                else
                {
                    var currentGroup = new CommonProtectionsGroup()
                    {
                        Name = protection.Name,
                        Tag = protection.Tag
                    };
                    currentGroup.Add(protection);
                    groups.Add(currentGroup);
                }
            }
            return groups;
        }

        private List<SecondLevelObjects> GroupSecondLevelObjects(IEnumerable<ReportObject> secondLevelObjects, List<GroupsConfiguration> groupsConfigurations)
        {
            var groups = new List<SecondLevelObjects>();
            foreach (var protection in secondLevelObjects)
            {
                // Пробуем найти конфигурации группы по типу защиты
                var groupConfig = groupsConfigurations.FirstOrDefault(p => p.Type == protection.Type);
                if (groupConfig != null) // Конфигурацию найти удалось
                {
                    var groupName = groupConfig.GetNameOfRange((protection as SecondLevelProtection).Number);

                    // Пробуем найти может быть уже есть такая группу
                    var existGroup = groups.FirstOrDefault(p => p.Name == groupName);

                    if (existGroup != null)
                    {
                        existGroup.Add(protection);
                    }
                    else
                    {
                        var currentGroup = new SecondLevelObjects()
                        {
                            Name = protection.Name,
                            Tag = protection.Tag,
                            Number = (protection as SecondLevelProtection).Number
                        };
                        currentGroup.Add(protection);
                        groups.Add(currentGroup);
                    }
                }
            }
            return groups;
        }
        private List<TorObjectsGroup> GroupTorObjects(IEnumerable<ReportObject> torObjects)
        {
            var groups = new List<TorObjectsGroup>();
            foreach (var torObject in torObjects)
            {
                // пробуем найти группу
                var existGroup = groups.FirstOrDefault(p => p.Number == (torObject as TorObject).Number && p.Tag == torObject.Tag);
                if (existGroup != null)
                {
                    existGroup.Add(torObject);
                }
                else
                {
                    var currentGroup = new TorObjectsGroup()
                    {
                        Name = torObject.Name,
                        Tag = torObject.Tag,
                        Number = (torObject as TorObject).Number
                    };
                    currentGroup.Add(torObject);
                    groups.Add(currentGroup);
                }
            }
            return groups;
        }
    }
}
