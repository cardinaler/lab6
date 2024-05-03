namespace ClassLibrary
{
    public struct SplineDataItem
    {
        public double Node
        { get; set; }
        public double NodeVal
        { get; set; }
        public double SplineNodeVal
        { get; set; }

        public SplineDataItem(double Node, double NodeVal, double SplineNodeVal)
        {
            this.Node = Node;
            this.NodeVal = NodeVal;
            this.SplineNodeVal = SplineNodeVal;
        }

        public string ToString(string format)
        {
            return $"Node = {Node.ToString(format)}, True val in node = {NodeVal.ToString(format)}, Spline val in node = {SplineNodeVal.ToString(format)}\n";
        }
        public override string ToString()
        {
            return $"Node = {Node}, NodeVal = {NodeVal}, SplineNodeVal = {SplineNodeVal}\n";
        }

    }

}