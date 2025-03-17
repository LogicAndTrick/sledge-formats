using System.Drawing;

namespace Sledge.Formats.Configuration.Worldcraft
{
    public class Worldcraft3DViewsConfiguration
    {
        /// <summary>
        /// Back clipping plane (0-10000)
        /// </summary>
        public int BackClippingPlane { get; set; }

        /// <summary>
        /// Filter textures
        /// </summary>
        public bool FilterTextures { get; set; }

        /// <summary>
        /// Animate models
        /// </summary>
        public bool AnimateModels { get; set; }

        /// <summary>
        /// Model render distance (0-2000)
        /// </summary>
        public int ModelRenderDistance { get; set; }

        /// <summary>
        /// Use mouselook navigation
        /// </summary>
        public bool UseMouselookNavigation { get; set; }

        /// <summary>
        /// Reverse mouse Y axis
        /// </summary>
        public bool ReverseMouseYAxis { get; set; }

        /// <summary>
        /// Forward speed (0-10000)
        /// </summary>
        public int ForwardSpeed { get; set; }

        /// <summary>
        /// Time to top speed (0-10 seconds)
        /// </summary>
        public decimal TimeToTopSpeed { get; set; }

        /// <summary>
        /// Reverse selection order
        /// </summary>
        public bool ReverseSelectionOrder { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Black;
    }
}