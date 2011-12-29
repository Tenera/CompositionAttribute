using System;

namespace ComposableObjects
{
    public class DoStuffEventArgs : EventArgs
    {
        public DoStuffEventArgs(string info)
        {
            Info = info;
        }
        public string Info { get; set; }
    }

    public class ClassA
    {
        public event EventHandler SomeEvent;
        public event EventHandler<DoStuffEventArgs> DoStuffEvent;

        public ClassA(int a, int b, bool c)
        {
            PropA = a;
            PropB = b;
            IsPropC = c; 
        }

        public int PropA { get; private set; }
        public int PropB { get; private set; }
        public bool IsPropC { get; set; }

        public void DoStuff()
        {
            Console.WriteLine("Dostuff");
            RaiseDoStuffEvent("DoStuff");
        }

        public int GetCount()
        {
            RaiseSomeEvent();
            return 100;
        }

        public void DoStuff(int input, string someText)
        {
            Console.WriteLine("DoStuff for {0} and \"{1}\"", input, someText);
            RaiseDoStuffEvent(string.Format("DoStuff for {0} and \"{1}\"", input, someText));
        }

        public void DoGenericStuff<T>(T input)
        {
            Console.WriteLine("DoGenericStuff for type " + input.GetType());
            RaiseDoStuffEvent("DoGenericStuff for type " + input.GetType());
        }

        private void RaiseDoStuffEvent(string info)
        {
            var ev = DoStuffEvent;
            if (ev != null)
            {
                ev(this, new DoStuffEventArgs(info));
            }
        }

        private void RaiseSomeEvent()
        {
            var ev = SomeEvent;
            if (ev != null)
            {
                ev(this, EventArgs.Empty);
            }
        }
    }
}
