using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;

namespace ModuleArt.Common
{
    [Serializable]
    public class ModuleManager<T>
    {
        private const string _ModuleBase = "ModuleBase";
        private const string _ModuleClassesNames = "ModuleClasses";
        private readonly IDictionary<string, AppDomain> _moduleStore;
        public List<T> Modules { get; }


        public ModuleManager()
        {
            _moduleStore = new Dictionary<string, AppDomain>();
            Modules = new List<T>();
        }

        public void LoadModules(string modulePath)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface.");

            modulePath = Path.GetFullPath(modulePath);

            if (!File.Exists(modulePath))
                throw new FileNotFoundException($"{modulePath} isn't a valid file.");

            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                ShadowCopyFiles = bool.TrueString,
            };
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            var moduleClassNames = AnalyzeAssembly<T>(modulePath);
            foreach (var typeName in moduleClassNames)
            {
                Console.WriteLine(typeName);
                AppDomain moduleAppDomain = AppDomain.CreateDomain($"Domain_Module_{typeName}", evidence, appDomainSetup);
                moduleAppDomain.SetData(_ModuleBase, Path.GetDirectoryName(modulePath));
                moduleAppDomain.UnhandledException += new UnhandledExceptionEventHandler(ModuleAppDomain_UnhandledException);
                Modules.Add((T)moduleAppDomain.CreateInstanceFromAndUnwrap(modulePath, typeName));
                _moduleStore[typeName] = moduleAppDomain;
            }
            //AppDomain tempAppDomain = AppDomain.CreateDomain($"Domain_Analyzer_{Path.GetFileName(modulePath)}", evidence, appDomainSetup);
            //moduleAppDomain.SetData(_ModuleInterfaceName, typeof(T).FullName);
        }

        //public ICollection<T> LoadModules<T>(string modulePath)
        //{
        //    if (!typeof(T).IsInterface)
        //        throw new ArgumentException("T must be an interface.");

        //    modulePath = Path.GetFullPath(modulePath);

        //    if (!File.Exists(modulePath))
        //        throw new FileNotFoundException($"{modulePath} isn't a valid file.");

        //    ICollection<T> modules = new List<T>();

        //    AppDomainSetup appDomainSetup = new AppDomainSetup
        //    {
        //        ApplicationBase = Environment.CurrentDirectory,
        //        DisallowBindingRedirects = true,
        //        DisallowCodeDownload = true,
        //        ShadowCopyFiles = bool.TrueString,
        //    };
        //    Evidence evidence = AppDomain.CurrentDomain.Evidence;
        //    var moduleClassNames = AnalyzeAssembly<T>(modulePath);
        //    foreach (var typeName in moduleClassNames)
        //    {
        //        Console.WriteLine(typeName);
        //        AppDomain moduleAppDomain = AppDomain.CreateDomain($"Domain_Module_{typeName}", evidence, appDomainSetup);
        //        moduleAppDomain.SetData(_ModuleBase, Path.GetDirectoryName(modulePath));
        //        modules.Add((T)moduleAppDomain.CreateInstanceFromAndUnwrap(modulePath, typeName));
        //        moduleAppDomain.UnhandledException += new UnhandledExceptionEventHandler(ModuleAppDomain_UnhandledException);
        //        _moduleStore[typeName] = moduleAppDomain;
        //    }
        //    //AppDomain tempAppDomain = AppDomain.CreateDomain($"Domain_Analyzer_{Path.GetFileName(modulePath)}", evidence, appDomainSetup);
        //    //moduleAppDomain.SetData(_ModuleInterfaceName, typeof(T).FullName);
        //    return modules;
        //}


        private ICollection<string> AnalyzeAssembly<T>(string modulePath)
        {
            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory,
                DisallowBindingRedirects = true,
                DisallowCodeDownload = true,
                ShadowCopyFiles = bool.TrueString,
                AppDomainInitializer = new AppDomainInitializer(FindModuleTypes),
                AppDomainInitializerArguments = new string[] { modulePath, typeof(T).FullName },
            };
            Stopwatch sw = Stopwatch.StartNew();
            AppDomain analyzerAppDomain = AppDomain.CreateDomain($"Domain_Analyzer_{Path.GetFileName(modulePath)}", AppDomain.CurrentDomain.Evidence, appDomainSetup);
            List<string> moduleClassNames = analyzerAppDomain.GetData(_ModuleClassesNames) as List<string>;
            AppDomain.Unload(analyzerAppDomain);
            sw.Stop();
            Console.WriteLine($"Elapsed: {sw.Elapsed}, ElapsedMilliseconds: {sw.ElapsedMilliseconds}ms");
            return moduleClassNames;
        }

        private static void FindModuleTypes(string[] args)
        {
            Assembly assembly = Assembly.LoadFile(args[0]);
            ICollection<string> moduleClassNames = new List<string>();

            if (assembly == null) return;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsInterface || type.IsAbstract)
                        continue;
                    if (type.GetInterface(args[1], true) != null)
                        moduleClassNames.Add(type.FullName);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // TODO: Proper logging
                foreach (Exception exception in ex.LoaderExceptions)
                {
                    Console.WriteLine(exception);
                }
            }

            AppDomain.CurrentDomain.SetData(_ModuleClassesNames, moduleClassNames);
        } 

        private void ModuleAppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain appDomain = sender as AppDomain;
            Exception exception = e.ExceptionObject as Exception;

            exception.Data.Add("ModuleDomain", this);

            if (_moduleStore.Values.Contains(appDomain) && appDomain.IsDefaultAppDomain() == false)
            {
                new Thread(delegate()
                {
                    string name = appDomain.FriendlyName;
                    try
                    {
                        Console.WriteLine($"Unloading application domain \"{name}\". Reason: offending");
                        AppDomain.Unload(appDomain);
                        var msos = _moduleStore.Where(mso => mso.Value == appDomain);
                        foreach (var mso in msos)
                        {
                            Modules.Remove(Modules.FirstOrDefault(m => m.GetType().FullName == mso.Key));
                            _moduleStore.Remove(mso);
                        }
                        appDomain = null;
                    }
                    catch (ThreadAbortException ex)
                    {
                        var msos = _moduleStore.Where(mso => mso.Value == appDomain);
                        foreach (var mso in msos)
                        {
                            Modules.Remove(Modules.FirstOrDefault(m => m.GetType().FullName == mso.Key));
                            _moduleStore.Remove(mso);
                        }
                        appDomain = null;
                        Console.WriteLine(ex);
                    }
                    catch (CannotUnloadAppDomainException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Console.WriteLine($"Unloaded application domain \"{name}\". Reason: offending");
                }).Start();
            }
        }
    }
}
