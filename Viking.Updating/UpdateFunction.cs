using System;
using System.Reflection;

namespace Viking.Updating
{
    public delegate void Updater();
    public delegate void Updater<TTrigger>(IUpdateChain<TTrigger> updateChain);
    public struct UpdateFunction<TTrigger>
    {
        public UpdateFunction(Updater updater, string name) : this(_ => updater(), updater.Method, name) { }
        public UpdateFunction(Updater<TTrigger> updater, string name) : this(updater, updater.Method, name) { }
        internal UpdateFunction(Updater<TTrigger> updater, MethodInfo methodInfo, string name)
        {
            Updater = updater;
            MethodInfo = methodInfo;
            Name = name;
        }

        public Updater<TTrigger> Updater { get; }
        internal MethodInfo MethodInfo { get; }
        public string Name { get; }

        public static implicit operator UpdateFunction<TTrigger>(Action updater) => new Updater(updater);
        public static implicit operator UpdateFunction<TTrigger>(Updater updater)
        {
            var method = updater.Method;
            return new UpdateFunction<TTrigger>(updater, GetNameFromMethod(method));
        }

        public static implicit operator UpdateFunction<TTrigger>(Action<IUpdateChain<TTrigger>> updater) => new Updater<TTrigger>(updater);
        public static implicit operator UpdateFunction<TTrigger>(Updater<TTrigger> updater)
        {
            var method = updater.Method;
            return new UpdateFunction<TTrigger>(updater, method, GetNameFromMethod(method));
        }

        private static string GetNameFromMethod(MethodInfo method) => FormattableString.Invariant($"{method.DeclaringType.FullName}.{method.Name}");
    }
}
