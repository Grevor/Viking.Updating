namespace Viking.Updating
{
    internal class TarjanNode<TTrigger>
    {
        public TarjanNode(UpdateNode<TTrigger> node)
        {
            Node = node;
        }

        public UpdateNode<TTrigger> Node { get; }
        public int LowLink { get; set; }
        public int Index { get; set; }
        public bool OnStack { get; set; }
    }
}
