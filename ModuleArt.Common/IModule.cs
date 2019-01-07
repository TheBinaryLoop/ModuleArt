using System;

namespace ModuleArt.Common
{
    public interface IModule
    {
        /// <summary>
        /// The name of this module.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The version of this module.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// The <see cref="IModuleContext"/> used while loading.
        /// </summary>
        IModuleContext Context { get; }

        /// <summary>
        /// Initializes the module after it has been loaded.
        /// </summary>
        /// <param name="context">The <see cref="IModuleContext"/>.</param>
        /// <returns>true if everything worked, false if there was an exception.</returns>
        bool Initialize(IModuleContext context);

        /// <summary>
        /// Activates this module.
        /// </summary>
        /// <returns>true if successfull, false otherwise.</returns>
        bool Activate();

        /// <summary>
        /// Deactivates this module.
        /// </summary>
        /// <returns>true if successfull, false otherwise.</returns>
        bool Deactivate();
    }
}
