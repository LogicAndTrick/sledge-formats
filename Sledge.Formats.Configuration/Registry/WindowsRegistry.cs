using Microsoft.Win32;

namespace Sledge.Formats.Configuration.Registry
{
    /// <summary>
    /// This class implements <see cref="IRegistry"/>, providing test-able access to the registry. Clients should
    /// never use the registry directly, it should use <see cref="IRegistry"/> so that we can test these interactions.
    /// </summary>
    /// <seealso cref="IRegistry" />
    public class WindowsRegistry : IRegistry
    {
        /// <inheritdoc />
        public IRegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
        {
            //  Proxy directly to the windows registry.
            var key = RegistryKey.OpenBaseKey(hKey, view);
            return new WindowsRegistryKey(key);
        }
    }
}
