namespace GameHelper.Plugin
{
    using System.Reflection;
    using System.Runtime.Loader;

    internal class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver resolver;

        public PluginAssemblyLoadContext(string assemblyLocation)
        {
            this.resolver = new AssemblyDependencyResolver(assemblyLocation);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var path = this.resolver.ResolveAssemblyToPath(assemblyName);
            if (path != null)
            {
                return this.LoadFromAssemblyPath(path);
            }

            return null;
        }
    }
}
