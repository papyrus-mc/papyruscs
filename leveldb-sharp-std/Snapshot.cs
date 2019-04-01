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
    /// Snapshots provide consistent read-only views over the entire state of
    /// the key-value store. ReadOptions.Snapshot may be non-NULL to indicate
    /// that a read should operate on a particular version of the DB state.
    /// If ReadOptions.Snapshot is NULL, the read will operate on an implicit
    /// snapshot of the current state.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }
        DB DB { get; set; }

        public Snapshot(DB db)
        {
            if (db == null) {
                throw new ArgumentNullException("db");
            }

            DB = db;
            Handle = Native.leveldb_create_snapshot(db.Handle);
        }

        ~Snapshot()
        {
            var db = DB.Handle;
            if (db != IntPtr.Zero) {
                Native.leveldb_release_snapshot(db, Handle);
            }
        }
    }
}
