using System;
using System.Collections.Generic;
using ModuleArt.Common;

namespace ModuleArt.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ICollection<IModule> modules = GenericModuleLoader.LoadModules<IModule>("Modules");
            foreach (var module in modules)
            {
                Console.WriteLine($"{module.Name} v{module.Version}");
            }
            Console.ReadLine();
        }
    }
}
