using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Viking.Updating
{
    internal enum Mark
    {
        NoMark,
        TemporaryMark,
        PermanentMark
    }
    internal class UpdateNode<TTrigger>
    {
        internal Mark Mark { get; set; }
        private List<string> InternalNames { get; } = new List<string>();

        public UpdateNode(UpdateFunction<TTrigger> function)
        {
            Function = function.Updater;
            MethodInfo = function.MethodInfo;
            InternalNames.Add(function.Name);
        }


        public IEnumerable<string> Names => InternalNames;
        public Updater<TTrigger> Function { get; }
        public MethodInfo MethodInfo { get; }

        public HashSet<TTrigger> IncommingTriggers { get; } = new HashSet<TTrigger>();
        public HashSet<TTrigger> OutgoingTriggers { get; } = new HashSet<TTrigger>();

        public bool AddIncomming(TTrigger trigger) => IncommingTriggers.Add(trigger);
        public bool AddOutgoing(TTrigger trigger) => OutgoingTriggers.Add(trigger);

        public bool RemoveIncomming(TTrigger trigger) => IncommingTriggers.Remove(trigger);
        public bool RemoveOutgoing(TTrigger trigger) => OutgoingTriggers.Remove(trigger);

        public void AddName(string name)
        {
            if (!InternalNames.Contains(name))
                InternalNames.Add(name);
        }

        public int NodeRegisterCount { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            var type = MethodInfo.DeclaringType;
            var typeName = type.IsConstructedGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName;

            var function = FormattableString.Invariant($"running function {typeName}.{MethodInfo.Name}");

            if (InternalNames.Count == 1)
            {
                builder.Append(FormattableString.Invariant($"Update node '{InternalNames[0]}'"));
            }
            else
            {
                var quotedNames = Names.Select(s => FormattableString.Invariant($"'{s}'"));
                var aliases = string.Join(", ", quotedNames);
                builder.Append(FormattableString.Invariant($"Update node with multiple aliases ({aliases})"));
            }

            builder.Append(' ');
            builder.Append(function);

            return builder.ToString();
        }
    }
}
