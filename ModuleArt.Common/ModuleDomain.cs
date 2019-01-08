//using System;
//using System.IO;
//using System.Reflection;
//using System.Security.Policy;

//namespace ModuleArt.Common
//{
//    [Serializable]
//    public class ModuleDomain<T> where T : class
//    {
//        /// <summary>
//        /// Key to set/get the Data in AppDomain
//        /// </summary>
//        private const string _GetDataModuleTypes = "moduleType";

//        /// <summary>
//        /// Created AppDomain
//        /// </summary>
//        public AppDomain AppDomain { get; set; }

//        /// <summary>
//        /// Instance from the loaded module
//        /// </summary>
//        public T Instance { get; protected set; }

//        /// <summary>
//        /// Type which implements the commited Interface
//        /// </summary>
//        public string ModuleType { get; } = string.Empty;

//        public string ModuleName => this.AppDomain.FriendlyName;

//        /// <summary>
//        /// Path to the module
//        /// </summary>
//        public string ModuleAssembly { get; } = string.Empty;


//        public virtual Assembly[] LoadedAssemblies => this.AppDomain.GetAssemblies();

//        /// <summary>
//        /// Creates a new AppDomain for the passed assembly
//        /// </summary>
//        /// <param name="assemblyPath">Path to the assembly</param>
//        /// <param name="interfaceOfModule">Type from the interface which the module implements</param>
//        public ModuleDomain(string assemblyPath, Type interfaceOfModule)
//        {
//            ModuleAssembly = assemblyPath;

//            // Get some informations from the assembly
//            FileInfo fileInfo = new FileInfo(assemblyPath);

//            AppDomainSetup appDomainSetup = new AppDomainSetup
//            {
//                ApplicationName = fileInfo.Name,
//                ApplicationBase = Environment.CurrentDirectory,
//                ConfigurationFile = $"{fileInfo.Name}.config",

//                //appDomainSetup.PrivateBinPath = fileinfo.Directory.FullName;
//                //appDomainSetup.PrivateBinPathProbe = fileinfo.Directory.FullName;
//                DisallowBindingRedirects = true,
//                DisallowCodeDownload = true,
//                ShadowCopyFiles = Boolean.TrueString,

//                // When creating the AppDomain the method GetInterfaceTypes will be called
//                AppDomainInitializer = new AppDomainInitializer(null),

//                // Set the parameter for the calling method GetInterfaceTypes
//                AppDomainInitializerArguments = new string[] { assemblyPath, interfaceOfModule.FullName }
//            };

//            //http://msdn.microsoft.com/en-us/library/system.security.policy.evidence(VS.90).aspx
//            Evidence appDomainEvidence = AppDomain.CurrentDomain.Evidence;

//            this.AppDomain = AppDomain.CreateDomain(fileInfo.Name, appDomainEvidence, appDomainSetup);

//            // The GetInterfaceTypes method was executed now we can catch our data
//            ModuleType = this.AppDomain.GetData(_GetDataModuleTypes) as string;

//            Instance = InstantiateModule();
//        }

//        private T InstantiateModule()
//        {
//            return this.AppDomain.CreateInstanceFromAndUnwrap(ModuleAssembly, ModuleType) as T;
//        }

//        private static void GetInterfaceTypes(string[] args)
//        {
//            AppDomain appDomain = AppDomain.CurrentDomain;

//            // Load the module assembly in our created AppDomain
//            Assembly assembly = Assembly.LoadFrom(args[0]);

//            string moduleType = string.Empty;

//            foreach (Type type in assembly.GetTypes())
//            {
//                if (type.IsNotPublic || type.IsAbstract || type.IsInterface)
//                    continue;
//                if (type.GetInterface(args[1], true) != null)
//                {
//                    moduleType = type.FullName;
//                    break;
//                }
//            }

//            // Set the data with the method SetData because the AppDomainInitializer Delegate got no return value. With GetData we can get the set data
//            // http://msdn.microsoft.com/en-us/library/system.appdomaininitializer.aspx
//            appDomain.SetData(_GetDataModuleTypes, moduleType);
//        }
//    }
//}
