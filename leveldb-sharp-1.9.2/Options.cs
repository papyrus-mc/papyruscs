//  leveldb-sharp
// 
//  Copyright (c) 2011 The LevelDB Authors
//  Copyright (c) 2012, Mirco Bauer <meebey@meebey.net>
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
    /// DB options
    /// </summary>
    public class Options
    {
#pragma warning disable 414
        Cache f_BlockCache;
#pragma warning restore 414

        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle { get; private set; }

        // TODO:
        // const Comparator* comparator;

        /// <summary>
        /// If true, the database will be created if it is missing.
        /// Default: false
        /// </summary>
        // bool create_if_missing;
        public bool CreateIfMissing {
            set {
                Native.leveldb_options_set_create_if_missing(Handle, value);
            }
        }

        /// <summary>
        /// If true, an error is raised if the database already exists.
        /// Default: false
        /// </summary>
        // bool error_if_exists;
        public bool ErrorIfExists {
            set {
                Native.leveldb_options_set_error_if_exists(Handle, value);
            }
        }

        /// <summary>
        /// If true, the implementation will do aggressive checking of the
        /// data it is processing and will stop early if it detects any
        /// errors.  This may have unforeseen ramifications: for example, a
        /// corruption of one DB entry may cause a large number of entries to
        /// become unreadable or for the entire DB to become unopenable.
        /// Default: false
        /// </summary>
        // bool paranoid_checks;
        public bool ParanoidChecks {
            set {
                Native.leveldb_options_set_paranoid_checks(Handle, value);
            }
        }

        // TODO:
        // Env* env;
        // Logger* info_log;

        /// <summary>
        /// Amount of data to build up in memory (backed by an unsorted log
        /// on disk) before converting to a sorted on-disk file.
        ///
        /// Larger values increase performance, especially during bulk loads.
        /// Up to two write buffers may be held in memory at the same time,
        /// so you may wish to adjust this parameter to control memory usage.
        /// Also, a larger write buffer will result in a longer recovery time
        /// the next time the database is opened.
        ///
        /// Default: 4MB
        /// </summary>
        // size_t write_buffer_size;
        public int WriteBufferSize {
            set {
                Native.leveldb_options_set_write_buffer_size(Handle, value);
            }
        }

        /// <summary>
        /// Number of open files that can be used by the DB.  You may need to
        /// increase this if your database has a large working set (budget
        /// one open file per 2MB of working set).
        /// Default: 1000
        /// </summary>
        // int max_open_files;
        public int MaxOpenFiles {
            set {
                Native.leveldb_options_set_max_open_files(Handle, value);
            }
        }

        /// <summary>
        /// Control over blocks (user data is stored in a set of blocks, and
        /// a block is the unit of reading from disk).
        ///
        /// If non-NULL, use the specified cache for blocks.
        /// If NULL, leveldb will automatically create and use an 8MB internal cache.
        /// Default: NULL
        /// </summary>
        // Cache* block_cache;
        public Cache BlockCache {
            set {
                // keep a reference to Cache so it doesn't get GCed
                f_BlockCache = value;
                if (value == null) {
                    Native.leveldb_options_set_cache(Handle, IntPtr.Zero);
                } else {
                    Native.leveldb_options_set_cache(Handle, value.Handle);
                }
            }
        }

        /// <summary>
        /// Approximate size of user data packed per block.  Note that the
        /// block size specified here corresponds to uncompressed data.  The
        /// actual size of the unit read from disk may be smaller if
        /// compression is enabled.  This parameter can be changed dynamically.
        ///
        /// Default: 4K
        /// </summary>
        // size_t block_size;
        public int BlockSize {
            set {
                Native.leveldb_options_set_block_size(Handle, value);
            }
        }

        /// <summary>
        /// Number of keys between restart points for delta encoding of keys.
        /// This parameter can be changed dynamically.  Most clients should
        /// leave this parameter alone.
        /// Default: 16
        /// </summary>
        // int block_restart_interval;
        public int BlockRestartInterval {
            set {
                Native.leveldb_options_set_write_buffer_size(Handle, value);
            }
        }

        /// <summary>
        /// Each block is individually compressed before being written to
        /// persistent storage. Compression is on by default since the default
        /// compression method is very fast, and is automatically disabled for
        /// uncompressible data. In rare cases, applications may want to
        /// disable compression entirely, but should only do so if benchmarks
        /// show a performance improvement.
        /// Default: SnappyCompression
        /// </summary>
        // CompressionType compression;
        public CompressionType Compression {
            set {
                Native.leveldb_options_set_compression(Handle, (int) value);
            }
        }

        public Options()
        {
            Handle = Native.leveldb_options_create();
        }

        ~Options()
        {
            Native.leveldb_options_destroy(Handle);
        }
    }
}
