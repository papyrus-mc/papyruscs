//  leveldb-sharp
//
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
using System.Collections;
using System.Collections.Generic;

namespace leveldb_sharp_std
{
    /// <summary>
    /// DB Iterator
    /// </summary>
    /// <remarks>
    /// This type is not thread safe.
    ///
    /// If two threads share this object, they must protect access to it using
    /// their own locking protocol.
    /// </remarks>
    public class Iterator : IEnumerator<KeyValuePair<byte[], byte[]>>
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }
        DB DB { get; set; }
        ReadOptions ReadOptions { get; set; }
        bool IsFirstMove { get; set; }

        public bool IsValid {
            get {
                return Native.leveldb_iter_valid(Handle);
            }
        }

        public byte[] Key {
            get {
                return Native.leveldb_iter_key(Handle);
            }
        }

        public byte[] Value {
            get {
                return Native.leveldb_iter_value(Handle);
            }
        }

        object IEnumerator.Current {
            get {
                return Current;
            }
        }

        public KeyValuePair<byte[], byte[]> Current {
            get {
                return new KeyValuePair<byte[], byte[]>(Key, Value);
            }
        }

        public Iterator(DB db, ReadOptions readOptions)
        {
            if (db == null) {
                throw new ArgumentNullException("db");
            }
            DB = db;
            // keep reference so it doesn't get GCed
            ReadOptions = readOptions;
            if (ReadOptions == null) {
                ReadOptions = new ReadOptions();
            }
            Handle = Native.leveldb_create_iterator(db.Handle, ReadOptions.Handle);
            IsFirstMove = true;
        }

        ~Iterator()
        {
            if (DB.Handle != IntPtr.Zero) {
                Native.leveldb_iter_destroy(Handle);
            }
        }

        public void SeekToFirst()
        {
            Native.leveldb_iter_seek_to_first(Handle);
        }

        public void SeekToLast()
        {
            Native.leveldb_iter_seek_to_last(Handle);
        }

        public void Seek(string key)
        {
            Native.leveldb_iter_seek(Handle, key);
        }

        public void Previous()
        {
            Native.leveldb_iter_prev(Handle);
        }

        public void Next()
        {
            Native.leveldb_iter_next(Handle);
        }

        public void Reset()
        {
            IsFirstMove = true;
            SeekToFirst();
        }

        public bool MoveNext()
        {
            if (IsFirstMove) {
                SeekToFirst();
                IsFirstMove = false;
                return IsValid;
            }
            Next();
            return IsValid;
        }

        public void Dispose()
        {
            // ~Iterator() takes already care
        }
    }
}
