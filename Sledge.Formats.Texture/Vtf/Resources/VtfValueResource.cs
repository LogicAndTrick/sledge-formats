using System;

namespace Sledge.Formats.Texture.Vtf.Resources
{
    public class VtfValueResource : VtfResource
    {
        public uint Value { get; set; }

        public byte TextureLodSettingsResolutionClampU
        {
            get
            {
                if (Type != VtfResourceType.TextureLodSettings)
                    throw new InvalidOperationException($"Cannot set TextureLodSettings if the resource type is not {VtfResourceType.TextureLodSettings}.");
                throw new NotImplementedException();
            }
            set
            {
                if (Type != VtfResourceType.TextureLodSettings)
                    throw new InvalidOperationException($"Cannot set TextureLodSettings if the resource type is not {VtfResourceType.TextureLodSettings}.");
                throw new NotImplementedException();
            }
        }

        public byte TextureLodSettingsResolutionClampV
        {
            get
            {
                if (Type != VtfResourceType.TextureLodSettings)
                    throw new InvalidOperationException($"Cannot set TextureLodSettings if the resource type is not {VtfResourceType.TextureLodSettings}.");
                throw new NotImplementedException();
            }
            set
            {
                if (Type != VtfResourceType.TextureLodSettings)
                    throw new InvalidOperationException($"Cannot set TextureLodSettings if the resource type is not {VtfResourceType.TextureLodSettings}.");
                throw new NotImplementedException();
            }
        }
    }
}