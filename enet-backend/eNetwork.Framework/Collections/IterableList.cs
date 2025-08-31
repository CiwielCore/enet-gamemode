using System.Collections.Generic;

namespace eNetwork.Framework.Collections
{
    public class IterableList<T>
    {
        private readonly IEnumerator<T> _enumerator;

        private bool _isLoop = false;
        private bool _isFirstRequest = true;

        public IterableList(IEnumerable<T> value)
        {
            _enumerator = value.GetEnumerator();
        }

        public IterableList<T> IsLooped()
        {
            _isLoop = true;
            return this;
        }

        public T GetNext()
        {
            Next();
            return GetCurrent();
        }

        public void Next()
        {
            _isFirstRequest = false;
            bool movedNext = _enumerator.MoveNext();
            if (movedNext == false && _isLoop)
            {
                _enumerator.Reset();
                _enumerator.MoveNext();
            }
        }

        public T GetCurrent()
        {
            if (_isFirstRequest)
                Next();

            return _enumerator.Current;
        }
    }
}
