using System;
using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.GameData.Objects
{
    public class AutoVisgroupSection
    {
        public string Name { get; set; }
        public List<AutoVisgroup> Groups { get; private set; }

        public AutoVisgroupSection()
        {
            Groups = new List<AutoVisgroup>();
        }

        public void Merge(AutoVisgroupSection section)
        {
            foreach (var group in section.Groups)
            {
                var existing = Groups.FirstOrDefault(x => x.Name == group.Name);
                if (existing != null)
                {
                    existing.EntityNames.AddRange(group.EntityNames.Where(x => !existing.EntityNames.Contains(x)));
                }
                else
                {
                    Groups.Add(group);
                }
            }
        }
    }
}