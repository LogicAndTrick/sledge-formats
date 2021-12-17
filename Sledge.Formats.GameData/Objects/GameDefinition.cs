using System;
using System.Collections.Generic;
using System.Linq;
using Sledge.Formats.Tokens;

namespace Sledge.Formats.GameData.Objects
{
    public class GameDefinition
    {
        public int? MapSizeLow { get; set; }
        public int? MapSizeHigh { get; set; }
        public List<GameDataClass> Classes { get; }
        public List<string> Includes { get; }
        public List<string> MaterialExclusions { get; }
        public List<AutoVisgroupSection> AutoVisgroups { get; }
        public List<Token> Warnings { get; set; }
        public List<int> GridNavValues { get; set; } // not fully understood what these values mean at this point
        public List<EntityGroup> EntityGroups { get; set; }

        public GameDefinition()
        {
            Classes = new List<GameDataClass>();
            Includes = new List<string>();
            MaterialExclusions = new List<string>();
            AutoVisgroups = new List<AutoVisgroupSection>();
            Warnings = new List<Token>();
            EntityGroups = new List<EntityGroup>();
        }

        public void MergeClass(GameDataClass cls)
        {
            var existing = GetClass(cls.Name);
            if (existing != null)
            {
                Classes.Remove(existing);
                cls.Inherit(new []{ existing });
            }
            Classes.Add(cls);
        }

        public void MergeAutoVisGroupSection(AutoVisgroupSection section)
        {
            var existing = AutoVisgroups.FirstOrDefault(x => x.Name == section.Name);
            if (existing != null)
            {
                existing.Merge(section);
            }
            else
            {
                AutoVisgroups.Add(section);
            }
        }

        public void CreateDependencies()
        {
            var resolved = new List<string>();
            var unresolved = new List<GameDataClass>(Classes);
            while (unresolved.Any())
            {
                var resolve = unresolved.Where(x => x.BaseClasses.All(resolved.Contains)).ToList();
                if (!resolve.Any()) throw new Exception("Circular dependencies: " + String.Join(", ", unresolved.Select(x => x.Name)));
                resolve.ForEach(x => x.Inherit(Classes.Where(y => x.BaseClasses.Contains(y.Name))));
                unresolved.RemoveAll(resolve.Contains);
                resolved.AddRange(resolve.Select(x => x.Name));
            }
        }

        public void RemoveDuplicates()
        {
            foreach (var g in Classes.Where(x => x.ClassType != ClassType.BaseClass).GroupBy(x => x.Name.ToLowerInvariant()).Where(g => g.Count() > 1).ToList())
            {
                foreach (var obj in g.Skip(1)) Classes.Remove(obj);
            }
        }

        public GameDataClass GetClass(string name)
        {
            return Classes
                .Where(x => x.ClassType != ClassType.BaseClass)
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
