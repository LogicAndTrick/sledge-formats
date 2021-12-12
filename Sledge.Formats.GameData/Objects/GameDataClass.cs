using System.Collections.Generic;
using System.Linq;

namespace Sledge.Formats.GameData.Objects
{
    public class GameDataClass
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AdditionalInformation { get; set; }
        public ClassType ClassType { get; set; }
        public List<string> BaseClasses { get; private set; }
        public List<Behaviour> Behaviours { get; private set; }
        public List<Property> Properties { get; private set; }
        public List<IO> InOuts { get; private set; }

        public GameDataClass(string name, string description, ClassType classType)
        {
            Name = name;
            Description = description;
            ClassType = classType;
            AdditionalInformation = "";
            BaseClasses = new List<string>();
            Behaviours = new List<Behaviour>();
            Properties = new List<Property>();
            InOuts = new List<IO>();
        }

        public void Inherit(IEnumerable<GameDataClass> parents)
        {
            foreach (var gdo in parents)
            {
                MergeBehaviours(gdo.Behaviours);
                MergeProperties(gdo.Properties);
                MergeInOuts(gdo.InOuts);
            }
        }

        private void MergeInOuts(IEnumerable<IO> inOuts)
        {
            var inc = 0;
            foreach (var io in inOuts)
            {
                var existing = InOuts.FirstOrDefault(x => x.IOType == io.IOType && x.Name == io.Name);
                if (existing == null) InOuts.Insert(inc++, io);
            }
        }

        private void MergeProperties(IEnumerable<Property> properties)
        {
            var inc = 0;
            foreach (var p in properties)
            {
                var existing = Properties.FirstOrDefault(x => x.Name == p.Name);
                if (existing != null) existing.Options.AddRange(p.Options.Where(x => !existing.Options.Contains(x)));
                else Properties.Insert(inc++, p);
            }
        }

        private void MergeBehaviours(IEnumerable<Behaviour> behaviours)
        {
            var inc = 0;
            foreach (var b in behaviours)
            {
                var existing = Behaviours.FirstOrDefault(x => x.Name == b.Name);
                if (existing != null) existing.Values.AddRange(b.Values.Where(x => !existing.Values.Contains(x)));
                else Behaviours.Insert(inc++, b);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
