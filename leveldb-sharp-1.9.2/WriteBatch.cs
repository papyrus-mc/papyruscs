//  leveldb-sharp
//
//  Copyright (c) 2011 The LevelDB Authors
//  Copyright (c) 2013, Mirco Bauer <meebey@meebey.net>
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are
//  met:
//
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
//       copyright notice, this list of conditions and the following disclaimer
//       in the documentation and/or other materials provided with the
//       distribution.
//     * Neither the name of Google Inc. nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
//  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
//  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;

namespace LevelDB
{
    /// <summary>
    /// Note that if the process dies after the Put of key2 but before the
    /// delete of key1, the same value may be left stored under multiple keys.
    /// Such problems can be avoided by using the WriteBatch class to
    /// atomically apply a set of updates.
    /// The WriteBatch holds a sequence of edits to be made to the database,
    /// and these edits within the batch are applied in order. Note that we
    /// called Delete before Put so that if key1 is identical to key2, we do
    /// not end up erroneously dropping the value entirely.
    /// Apart from its atomicity benefits, WriteBatch may also be used to speed
    /// up bulk updates by placing lots of individual mutations into the same
    /// batch.
    /// </summary>
    /// <remarks>
    /// This type is not thread safe.
    ///
    /// If two threads share this object, they must protect access to it using
    /// their own locking protocol.
    /// </remarks>
    public class WriteBatch
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }

        public WriteBatch()
        {
            Handle = Native.leveldb_writebatch_create();
        }

        ~WriteBatch()
        {
            Native.leveldb_writebatch_destroy(Handle);
        }

        public WriteBatch Put(string key, string value)
        {
            Native.leveldb_writebatch_put(Handle, key, value);
            return this;
        }

        public WriteBatch Delete(string key)
        {
            Native.leveldb_writebatch_delete(Handle, key);
            return this;
        }

        public void Clear()
        {
            Native.leveldb_writebatch_clear(Handle);
        }
    }
}
