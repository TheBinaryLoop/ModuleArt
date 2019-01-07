using System;
using ModuleArt.Common;
using ModuleArt.Common.Attributes;

namespace ModuleArt.Modules
{
    [Module]
    public class ExampleModule : IModule
    {
        public string Name => "ExampleModule";

        public Version Version => new Version(0, 1, 0, 1);

        public IModuleContext Context { get; private set; }

        public bool Initialize(IModuleContext context = null)
        {
            Context = context;
            return true;
        }

        public bool Activate()
        {
            return true;
        }

        public bool Deactivate()
        {
            return true;
        }

    }
}
