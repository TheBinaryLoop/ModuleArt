using System;
using System.Timers;
using ModuleArt.Common;
using ModuleArt.Common.Attributes;
using Newtonsoft.Json;

namespace ModuleArt.Module
{
    [Module, Serializable]
    public class ExampleModule : MarshalByRefObject, IModule
    {
        public string Name => "ExampleModule";

        public Version Version => new Version(0, 1, 0, 1);

        public IModuleContext Context { get; private set; }

        public ExampleModule()
        {
            //Timer t = new Timer(2000);
            //t.Enabled = true;
            //t.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            //t.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer_Elapsed");
            throw new ArgumentNullException();
        }

        public bool Initialize(IModuleContext context = null)
        {
            Context = context;
            return true;
        }

        public bool Activate()
        {
            int a = 0;
            int i = 0 / a;
            return true;
        }

        public bool Deactivate()
        {
            return true;
        }

    }
}
