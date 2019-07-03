using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Maploader.World;
using Microsoft.Extensions.ObjectPool;

namespace Maploader.Core
{
    public interface IResettable
    {
        void Reset();
    }

    public abstract class Pool<T> : IPooledObjectPolicy<T> where T : class
    {

        private int counter = 0;
        private Stopwatch sp;

        private readonly ObjectPool<T> pool;
        private readonly object lockObject = new object();

        protected Pool()
        {
            pool = new DefaultObjectPool<T>(this);
            sp = Stopwatch.StartNew();
        }

        public T Get()
        {
            //lock (lockObject)
            {
                Interlocked.Increment(ref counter);
                return pool.Get();
            }
        }

        public abstract T Create();
        bool IPooledObjectPolicy<T>.Return(T obj)
        {
            return true;
        }

        public void Return(T obj)
        {
            if (obj is IResettable r)
            {
                r.Reset();
            }

            //lock (lockObject)
            {
                Interlocked.Decrement(ref counter);
                pool.Return(obj);

                if (sp.ElapsedMilliseconds > 2000)
                {
                    Console.WriteLine("Pool stat {1} {0}", counter, typeof(T));
                    sp.Restart();
                }
            }
        }
    }

    public class ChunkPool : Pool<Chunk>
    {
        public override Chunk Create()
        {
            return new Chunk();
        }
    }

    public class GenericPool<T> : Pool<T> where T : class
    {
        private readonly Func<T> creator;

        public GenericPool(Func<T> creator)
        {
            this.creator = creator;
        }

        public override T Create() => creator();
    }


}
