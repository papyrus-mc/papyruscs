using System;
using System.Collections.Generic;
using System.Text;
using Maploader.World;
using Microsoft.Extensions.ObjectPool;

namespace Maploader.Core
{
    public class ChunkPool
    {
        private readonly ObjectPool<Chunk> pool;
        private object lockObject = new object();
        public ChunkPool()
        {
            pool = new DefaultObjectPool<Chunk>(new DefaultPooledObjectPolicy<Chunk>());
        }

        public Chunk Get()
        {
            lock (lockObject)
            {
                return pool.Get();
            }
        }

        public void Return(Chunk obj)
        {
            obj.Reset();
            lock (lockObject)
            {
                pool.Return(obj);
            }
        }
    }
}
