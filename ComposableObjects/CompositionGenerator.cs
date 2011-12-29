


using System;

namespace ComposableObjects
{
	public  partial class ComposedClass
	{
		public event EventHandler SomeEvent;

		private void RaiseSomeEvent(EventArgs ea)
		{
			var ev = SomeEvent;
			if (ev != null) { ev(this, ea); }
		}

		private void Handle_classASomeEvent(object sender, EventArgs ea) { RaiseSomeEvent(ea); }

		public event EventHandler<ComposableObjects.DoStuffEventArgs> DoStuffEvent;

		private void RaiseDoStuffEvent(ComposableObjects.DoStuffEventArgs ea)
		{
			var ev = DoStuffEvent;
			if (ev != null) { ev(this, ea); }
		}

		private void Handle_classADoStuffEvent(object sender, ComposableObjects.DoStuffEventArgs ea) { RaiseDoStuffEvent(ea); }

		private void AttachEvents()
		{
			_classA.SomeEvent += Handle_classASomeEvent;
			_classA.DoStuffEvent += Handle_classADoStuffEvent;
		}

		private void DetachEvents()
		{
			_classA.SomeEvent -= Handle_classASomeEvent;
			_classA.DoStuffEvent -= Handle_classADoStuffEvent;
		}

		public int PropA
		{
			get { return _classA.PropA; }
		}

		public int PropB
		{
			get { return _classA.PropB; }
		}

		public bool IsPropC
		{
			get { return _classA.IsPropC; }
			set { _classA.IsPropC = value; }
		}

		public void DoStuff()
		{
			_classA.DoStuff();
		}

		public int GetCount()
		{
			return _classA.GetCount();
		}

		public void DoStuff(int input, string someText)
		{
			_classA.DoStuff(input, someText);
		}

		public void DoGenericStuff<T>(T input)
		{
			_classA.DoGenericStuff(input);
		}

	}

}


