//  leveldb-sharp
// 
//  Copyright (c) 2011 The LevelDB Authors
//  Copyright (c) 2012-2013, Mirco Bauer <meebey@meebey.net>
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
    /// Read options
    /// </summary>
    public class ReadOptions
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// May be set to true to force checksum verification of all data that
        /// is read from the file system on behalf of a particular read.
        /// By default, no such verification is done.
        /// </summary>
        public bool VerifyChecksums {
            set {
                Native.leveldb_readoptions_set_verify_checksums(Handle, value);
            }
        }

        /// <summary>
        /// When performing a bulk read, the application may wish to disable
        /// caching so that the data processed by the bulk read does not end up
        /// displacing most of the cached contents.
        /// </summary>
        public bool FillCache {
            set {
                Native.leveldb_readoptions_set_fill_cache(Handle, value);
            }
        }

        public Snapshot Snapshot {
            set {
                if (value == null) {
                    Native.leveldb_readoptions_set_snapshot(Handle, IntPtr.Zero);
                } else {
                    Native.leveldb_readoptions_set_snapshot(Handle, value.Handle);
                }
            }
        }

        public ReadOptions()
        {
            Handle = Native.leveldb_readoptions_create();
        }

        ~ReadOptions()
        {
            Native.leveldb_readoptions_destroy(Handle);
        }
    }
}
