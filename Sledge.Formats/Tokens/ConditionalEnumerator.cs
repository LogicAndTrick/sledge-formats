using System;
using System.Collections;
using System.Collections.Generic;

namespace Sledge.Formats.Tokens
{
    public class ConditionalEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        public Predicate<T> Condition { get; set; }
        public T Current => _enumerator.Current;
        object IEnumerator.Current => _enumerator.Current;

        public ConditionalEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
            Condition = _ => true;
        }

        public bool MoveNext()
        {
            bool ret;
            do
            {
                ret = _enumerator.MoveNext();
            } while (!Condition.Invoke(_enumerator.Current));
            return ret;
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public void Dispose()
        {
            Condition = null;
            _enumerator.Dispose();
        }
    }
}
