namespace NSBBehaviourTest
{
    public static class SharedState
    {
        private static readonly object _lock = new object();
        private static string _data = "";

        public static string HandleSuccessMessage {
            get
            {
                lock (_lock)
                {
                    return _data;
                }
            }
            set
            {
                lock (_lock)
                {
                    _data = value;
                }
            }
        }
    }
}
