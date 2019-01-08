using System;
using System.Collections.Generic;
using System.IO;
using ModuleArt.Common;

namespace ModuleArt.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //ICollection<IModule> modules = GenericModuleLoader.LoadModules<IModule>("Modules", searchOption: System.IO.SearchOption.AllDirectories);
            //foreach (var module in modules)
            //{
            //    Console.WriteLine($"{module.Name} v{module.Version}");
            //}

            var moduleMananger = new ModuleManager<IModule>();

            foreach (var assembly in Directory.GetFiles(Path.GetFullPath("Modules"), "*.dll", SearchOption.AllDirectories))
            {
                moduleMananger.LoadModules(assembly);
            }

            foreach (var module in moduleMananger.Modules)
            {
                module.Activate();
            }

            Console.ReadLine();
        }
    }
}
