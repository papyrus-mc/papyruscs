// leveldb-sharp
//
// Copyright (c) 2011 The LevelDB Authors
// Copyright (c) 2012-2013, Mirco Bauer <meebey@meebey.net>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above
//      copyright notice, this list of conditions and the following disclaimer
//      in the documentation and/or other materials provided with the
//      distribution.
//    * Neither the name of Google Inc. nor the names of its
//      contributors may be used to endorse or promote products derived from
//      this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace leveldb_sharp_std
{
    /// <summary>
    /// Native method P/Invoke declarations for LevelDB
    /// </summary>
    public static class Native
    {
        public static void CheckError(string error)
        {
            if (String.IsNullOrEmpty(error)) {
                return;
            }

            throw new ApplicationException(error);
        }

        public static void CheckError(IntPtr error)
        {
            if (error == IntPtr.Zero) {
                return;
            }

            CheckError(GetAndReleaseString(error));
        }

        public static UIntPtr GetStringLength(string value)
        {
            if (value == null || value.Length == 0) {
                return UIntPtr.Zero;
            }
            return new UIntPtr((uint) Encoding.UTF8.GetByteCount(value));
        }

        public static string GetAndReleaseString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) {
                return null;
            }

            var str = Marshal.PtrToStringAnsi(ptr);
            leveldb_free(ptr);
            return str;
        }

#region DB operations
        #region leveldb_open
        // extern leveldb_t* leveldb_open(const leveldb_options_t* options, const char* name, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_open(IntPtr options, string name, out IntPtr error);

        public static IntPtr leveldb_open(IntPtr options, string name, out string error)
        {
            IntPtr errorPtr;
            var db = leveldb_open(options, name, out errorPtr);
            error = GetAndReleaseString(errorPtr);
            return db;
        }

        public static IntPtr leveldb_open(IntPtr options, string name)
        {
            string error;
            var db = leveldb_open(options, name, out error);
            CheckError(error);
            return db;
        }
        #endregion

        // extern void leveldb_close(leveldb_t* db);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_close(IntPtr db);

        #region leveldb_put
        // extern void leveldb_put(leveldb_t* db, const leveldb_writeoptions_t* options, const char* key, size_t keylen, const char* val, size_t vallen, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_put(IntPtr db,
                                              IntPtr writeOptions,
                                              IntPtr key,
                                              UIntPtr keyLength,
                                              IntPtr value,
                                              UIntPtr valueLength,
                                              out IntPtr error);


        public static void leveldb_put(IntPtr db,
            IntPtr writeOptions,
            byte[] key,
            byte[] value)
        {
            string error;
            var keyLength = (UIntPtr) key.Length;
            var valueLength = (UIntPtr) value.Length;
            unsafe
            {
                fixed (byte* pKey = key)
                fixed (byte* pValue = value)
                {
                    IntPtr errorPtr;
                    Native.leveldb_put(db, writeOptions, (IntPtr) pKey, keyLength, (IntPtr) pValue, valueLength, out errorPtr);
                    error = GetAndReleaseString(errorPtr);
                    CheckError(error);
                }
            }
        }

        #endregion

        #region leveldb_delete
        // extern void leveldb_delete(leveldb_t* db, const leveldb_writeoptions_t* options, const char* key, size_t keylen, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_delete(IntPtr db, IntPtr writeOptions, string key, UIntPtr keylen, out IntPtr error);

        public static void leveldb_delete(IntPtr db, IntPtr writeOptions, string key, UIntPtr keylen, out string error)
        {
            IntPtr errorPtr;
            leveldb_delete(db, writeOptions, key, keylen, out errorPtr);
            error = GetAndReleaseString(errorPtr);
        }

        public static void leveldb_delete(IntPtr db, IntPtr writeOptions, string key)
        {
            string error;
            var keyLength = GetStringLength(key);
            leveldb_delete(db, writeOptions, key, keyLength, out error);
            CheckError(error);
        }
        #endregion

        #region leveldb_write
        // extern void leveldb_write(leveldb_t* db, const leveldb_writeoptions_t* options, leveldb_writebatch_t* batch, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_write(IntPtr db, IntPtr writeOptions, IntPtr writeBatch, out IntPtr error);

        public static void leveldb_write(IntPtr db, IntPtr writeOptions, IntPtr writeBatch, out string error)
        {
            IntPtr errorPtr;
            leveldb_write(db, writeOptions, writeBatch, out errorPtr);
            error = GetAndReleaseString(errorPtr);
        }

        public static void leveldb_write(IntPtr db, IntPtr writeOptions, IntPtr writeBatch)
        {
            string error;
            leveldb_write(db, writeOptions, writeBatch, out error);
            CheckError(error);
        }
        #endregion

        #region leveldb_get
        // extern char* leveldb_get(leveldb_t* db, const leveldb_readoptions_t* options, const char* key, size_t keylen, size_t* vallen, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_get(IntPtr db,
                                                IntPtr readOptions,
                                                IntPtr key,
                                                UIntPtr keyLength,
                                                out UIntPtr valueLength,
                                                out IntPtr error);

     

        public static byte[] leveldb_get(IntPtr db,
                                         IntPtr readOptions,
                                         byte[] key)
        {
           

            unsafe
            {
                fixed (byte* p = key)
                {
                    UIntPtr valueLength;
                    string error;
                    var keyLength = (UIntPtr) key.Length;
                    IntPtr keyPtr = (IntPtr)p;
                    IntPtr errorPtr;

                    var valuePtr = leveldb_get(db, readOptions, keyPtr, keyLength, out valueLength, out errorPtr);
                    error = GetAndReleaseString(errorPtr);

                    CheckError(error);
                    if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero)
                    {
                        return null;
                    }

                    var ret = new byte[(int) valueLength];
                    Marshal.Copy(valuePtr, ret, 0, (int)valueLength);

                    leveldb_free(valuePtr);
                    return ret;
                }
            }
        }
        #endregion

        // extern leveldb_iterator_t* leveldb_create_iterator(leveldb_t* db, const leveldb_readoptions_t* options);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_create_iterator(IntPtr db, IntPtr readOptions);

        // extern const leveldb_snapshot_t* leveldb_create_snapshot(leveldb_t* db);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_create_snapshot(IntPtr db);

        // extern void leveldb_release_snapshot(leveldb_t* db, const leveldb_snapshot_t* snapshot);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_release_snapshot(IntPtr db, IntPtr snapshot);

        /// <summary>
        /// Returns NULL if property name is unknown.
        /// Else returns a pointer to a malloc()-ed null-terminated value.
        /// </summary>
        // extern char* leveldb_property_value(leveldb_t* db, const char* propname);
        [DllImport("leveldb", EntryPoint="leveldb_property_value", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_property_value_native(IntPtr db, string propname);
        public static string leveldb_property_value(IntPtr db, string propname)
        {
            var valuePtr = leveldb_property_value_native(db, propname);
            if (valuePtr == IntPtr.Zero) {
                return null;
            }
            var value = Marshal.PtrToStringAnsi(valuePtr);
            leveldb_free(valuePtr);
            return value;
        }

        // extern void leveldb_approximate_sizes(
        //     leveldb_t* db, int num_ranges,
        //     const char* const* range_start_key,
        //     const size_t* range_start_key_len,
        //     const char* const* range_limit_key,
        //     const size_t* range_limit_key_len,
        //     uint64_t* sizes);

        /// <summary>
        /// Compact the underlying storage for the key range [startKey,limitKey].
        /// In particular, deleted and overwritten versions are discarded,
        /// and the data is rearranged to reduce the cost of operations
        /// needed to access the data.  This operation should typically only
        /// be invoked by users who understand the underlying implementation.
        ///
        /// startKey==null is treated as a key before all keys in the database.
        /// limitKey==null is treated as a key after all keys in the database.
        /// Therefore the following call will compact the entire database:
        ///    leveldb_compact_range(db, null, null);
        /// </summary>
        // extern void leveldb_compact_range(leveldb_t* db,
        //     const char* start_key, size_t start_key_len,
        //     const char* limit_key, size_t limit_key_len);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_compact_range(IntPtr db,
                                                        string startKey,
                                                        UIntPtr startKeyLen,
                                                        string limitKey,
                                                        UIntPtr limitKeyLen);
        public static void leveldb_compact_range(IntPtr db,
                                                 string startKey,
                                                 string limitKey)
        {
            leveldb_compact_range(db,
                                  startKey, GetStringLength(startKey),
                                  limitKey, GetStringLength(limitKey));
        }
#endregion

#region Management operations
        #region leveldb_destroy_db
        // extern void leveldb_destroy_db(const leveldb_options_t* options, const char* name, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_destroy_db(IntPtr options, string path, out IntPtr error);

        public static void leveldb_destroy_db(IntPtr options, string path, out string error)
        {
            IntPtr errorPtr;
            leveldb_destroy_db(options, path, out errorPtr);
            error = GetAndReleaseString(errorPtr);
        }

        public static void leveldb_destroy_db(IntPtr options, string path)
        {
            string error;
            leveldb_destroy_db(options, path, out error);
            CheckError(error);
        }
        #endregion

        #region leveldb_repair_db
        // extern void leveldb_repair_db(const leveldb_options_t* options, const char* name, char** errptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_repair_db(IntPtr options, string path, out IntPtr error);

        public static void leveldb_repair_db(IntPtr options, string path, out string error)
        {
            IntPtr errorPtr;
            leveldb_repair_db(options, path, out errorPtr);
            error = GetAndReleaseString(errorPtr);
        }

        public static void leveldb_repair_db(IntPtr options, string path)
        {
            string error;
            leveldb_repair_db(options, path, out error);
            CheckError(error);
        }
        #endregion
#endregion

#region Write batch
        // extern leveldb_writebatch_t* leveldb_writebatch_create();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_writebatch_create();

        // extern void leveldb_writebatch_destroy(leveldb_writebatch_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_destroy(IntPtr writeBatch);

        // extern void leveldb_writebatch_clear(leveldb_writebatch_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_clear(IntPtr writeBatch);

        // extern void leveldb_writebatch_put(leveldb_writebatch_t*, const char* key, size_t klen, const char* val, size_t vlen);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_put(IntPtr writeBatch,
                                                         string key,
                                                         UIntPtr keyLength,
                                                         string value,
                                                         UIntPtr valueLength);
        public static void leveldb_writebatch_put(IntPtr writeBatch,
                                                  string key,
                                                  string value)
        {
            var keyLength = GetStringLength(key);
            var valueLength = GetStringLength(value);
            Native.leveldb_writebatch_put(writeBatch,
                                          key, keyLength,
                                          value, valueLength);
        }

        // extern void leveldb_writebatch_delete(leveldb_writebatch_t*, const char* key, size_t klen);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writebatch_delete(IntPtr writeBatch, string key, UIntPtr keylen);
        public static void leveldb_writebatch_delete(IntPtr writeBatch, string key)
        {
            var keyLength = GetStringLength(key);
            leveldb_writebatch_delete(writeBatch, key, keyLength);
        }

        // TODO:
        // extern void leveldb_writebatch_iterate(leveldb_writebatch_t*, void* state, void (*put)(void*, const char* k, size_t klen, const char* v, size_t vlen), void (*deleted)(void*, const char* k, size_t klen));

#endregion

#region Options
        // extern leveldb_options_t* leveldb_options_create();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_options_create();

        // extern void leveldb_options_destroy(leveldb_options_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_destroy(IntPtr options);

        // extern void leveldb_options_set_comparator(leveldb_options_t*, leveldb_comparator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_comparator(IntPtr options, IntPtr comparator);

        /// <summary>
        /// If true, the database will be created if it is missing.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_create_if_missing(leveldb_options_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_create_if_missing(IntPtr options, bool value);

        /// <summary>
        /// If true, an error is raised if the database already exists.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_error_if_exists(leveldb_options_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_error_if_exists(IntPtr options, bool value);

        /// <summary>
        /// If true, the implementation will do aggressive checking of the
        /// data it is processing and will stop early if it detects any
        /// errors.  This may have unforeseen ramifications: for example, a
        /// corruption of one DB entry may cause a large number of entries to
        /// become unreadable or for the entire DB to become unopenable.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_paranoid_checks(leveldb_options_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_paranoid_checks(IntPtr options, bool value);

        /// <summary>
        /// Number of open files that can be used by the DB.  You may need to
        /// increase this if your database has a large working set (budget
        /// one open file per 2MB of working set).
        /// Default: 1000
        /// </summary>
        // extern void leveldb_options_set_max_open_files(leveldb_options_t*, int);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_max_open_files(IntPtr options, int value);

        /// <summary>
        /// Each block is individually compressed before being written to
        /// persistent storage. Compression is on by default since the default
        /// compression method is very fast, and is automatically disabled for
        /// uncompressible data. In rare cases, applications may want to
        /// disable compression entirely, but should only do so if benchmarks
        /// show a performance improvement.
        /// Default: 1 (SnappyCompression)
        /// </summary>
        /// <seealso cref="T:CompressionType"/>
        // extern void leveldb_options_set_compression(leveldb_options_t*, int);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_compression(IntPtr options, int value);

        /// <summary>
        /// Control over blocks (user data is stored in a set of blocks, and
        /// a block is the unit of reading from disk).
        ///
        /// If non-NULL, use the specified cache for blocks.
        /// If NULL, leveldb will automatically create and use an 8MB internal cache.
        /// Default: NULL
        /// </summary>
        // extern void leveldb_options_set_cache(leveldb_options_t*, leveldb_cache_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_cache(IntPtr options, IntPtr cache);
        public static void leveldb_options_set_cache_size(IntPtr options, int capacity)
        {
            var cache = leveldb_cache_create_lru((UIntPtr) capacity);
            leveldb_options_set_cache(options, cache);
        }

        /// <summary>
        /// Approximate size of user data packed per block.  Note that the
        /// block size specified here corresponds to uncompressed data.  The
        /// actual size of the unit read from disk may be smaller if
        /// compression is enabled.  This parameter can be changed dynamically.
        ///
        /// Default: 4K
        /// </summary>
        // extern void leveldb_options_set_block_size(leveldb_options_t*, size_t);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_block_size(IntPtr options, UIntPtr size);
        public static void leveldb_options_set_block_size(IntPtr options, int size)
        {
            leveldb_options_set_block_size(options, (UIntPtr) size);
        }

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
        // extern void leveldb_options_set_write_buffer_size(leveldb_options_t*, size_t);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_write_buffer_size(IntPtr options, UIntPtr size);
        public static void leveldb_options_set_write_buffer_size(IntPtr options, int size)
        {
            leveldb_options_set_write_buffer_size(options, (UIntPtr) size);
        }

        /// <summary>
        /// Number of keys between restart points for delta encoding of keys.
        /// This parameter can be changed dynamically.  Most clients should
        /// leave this parameter alone.
        /// Default: 16
        /// </summary>
        // extern void leveldb_options_set_block_restart_interval(leveldb_options_t*, int);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_options_set_block_restart_interval(IntPtr options, int interval);
#endregion

#region Read Options
        // extern leveldb_readoptions_t* leveldb_readoptions_create();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_readoptions_create();

        // extern void leveldb_readoptions_destroy(leveldb_readoptions_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_destroy(IntPtr readOptions);

        // extern void leveldb_readoptions_set_verify_checksums(leveldb_readoptions_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_verify_checksums(IntPtr readOptions, bool value);

        // extern void leveldb_readoptions_set_fill_cache(leveldb_readoptions_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_fill_cache(IntPtr readOptions, bool value);

        // extern void leveldb_readoptions_set_snapshot(leveldb_readoptions_t*, const leveldb_snapshot_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_readoptions_set_snapshot(IntPtr readOptions, IntPtr snapshot);
#endregion

#region Write Options
        // extern leveldb_writeoptions_t* leveldb_writeoptions_create();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_writeoptions_create();

        // extern void leveldb_writeoptions_destroy(leveldb_writeoptions_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writeoptions_destroy(IntPtr writeOptions);

        // extern void leveldb_writeoptions_set_sync(leveldb_writeoptions_t*, unsigned char);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_writeoptions_set_sync(IntPtr writeOptions, bool value);
#endregion

#region Iterator
        // extern void leveldb_iter_seek_to_first(leveldb_iterator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek_to_first(IntPtr iter);

        // extern void leveldb_iter_seek_to_last(leveldb_iterator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek_to_last(IntPtr iter);

        // extern void leveldb_iter_seek(leveldb_iterator_t*, const char* k, size_t klen);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_seek(IntPtr iter, string key, UIntPtr keyLength);
        public static void leveldb_iter_seek(IntPtr iter, string key)
        {
            var keyLength = GetStringLength(key);
            leveldb_iter_seek(iter, key, keyLength);
        }

        // extern unsigned char leveldb_iter_valid(const leveldb_iterator_t*);
        [DllImport("leveldb", EntryPoint="leveldb_iter_valid", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte leveldb_iter_valid_native(IntPtr iter);
        public static bool leveldb_iter_valid(IntPtr iter)
        {
            return leveldb_iter_valid_native(iter) != 0;
        }

        // extern void leveldb_iter_prev(leveldb_iterator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_prev(IntPtr iter);

        // extern void leveldb_iter_next(leveldb_iterator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_next(IntPtr iter);

        // extern const char* leveldb_iter_key(const leveldb_iterator_t*, size_t* klen);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_iter_key(IntPtr iter, out UIntPtr keyLength);
        public static byte[] leveldb_iter_key(IntPtr iter)
        {
            UIntPtr keyLength;
            var keyPtr = leveldb_iter_key(iter, out keyLength);
            if (keyPtr == IntPtr.Zero || keyLength == UIntPtr.Zero) {
                return null;
            }

            var key = new byte[(int) keyLength];
            Marshal.Copy(keyPtr, key, 0, (int) keyLength);
            return key;
        }

        // extern const char* leveldb_iter_value(const leveldb_iterator_t*, size_t* vlen);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_iter_value(IntPtr iter, out UIntPtr valueLength);

        public static byte[] leveldb_iter_value(IntPtr iter)
        {
            UIntPtr valueLength;
            var valuePtr = leveldb_iter_value(iter, out valueLength);
            if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero) {
                return null;
            }
            byte[] value = new byte[(int)valueLength];
            Marshal.Copy(valuePtr, value, 0, (int) valueLength);
            return value;
        }

        // extern void leveldb_iter_destroy(leveldb_iterator_t*);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_iter_destroy(IntPtr iter);

        // TODO:
        // extern void leveldb_iter_get_error(const leveldb_iterator_t*, char** errptr);
#endregion

#region Cache
        // extern leveldb_cache_t* leveldb_cache_create_lru(size_t capacity);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr leveldb_cache_create_lru(UIntPtr capacity);

        // extern void leveldb_cache_destroy(leveldb_cache_t* cache);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_cache_destroy(IntPtr cache);
#endregion

#region Env
        // TODO:
        // extern leveldb_env_t* leveldb_create_default_env();
        // extern void leveldb_env_destroy(leveldb_env_t*);
#endregion

#region Utility
        /// <summary>
        /// Calls free(ptr).
        /// REQUIRES: ptr was malloc()-ed and returned by one of the routines
        /// in this file.  Note that in certain cases (typically on Windows),
        /// you may need to call this routine instead of free(ptr) to dispose
        /// of malloc()-ed memory returned by this library.
        /// </summary>
        // extern void leveldb_free(void* ptr);
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void leveldb_free(IntPtr ptr);

        /// <summary>
        /// Return the major version number for this release.
        /// </summary>
        // extern int leveldb_major_version();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int leveldb_major_version();

        /// <summary>
        /// Return the minor version number for this release.
        /// </summary>
        // extern int leveldb_minor_version();
        [DllImport("leveldb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int leveldb_minor_version();
#endregion

        public static void Dump(IntPtr db)
        {
            var options = Native.leveldb_readoptions_create();
            IntPtr iter = Native.leveldb_create_iterator(db, options);
            for (Native.leveldb_iter_seek_to_first(iter);
                 Native.leveldb_iter_valid(iter);
                 Native.leveldb_iter_next(iter)) {
                byte[] key = Native.leveldb_iter_key(iter);
                byte[] value = Native.leveldb_iter_value(iter);
                Console.WriteLine("'{0}' => '{1}'", key, value);
            }
            Native.leveldb_iter_destroy(iter);
        }
    }
}
