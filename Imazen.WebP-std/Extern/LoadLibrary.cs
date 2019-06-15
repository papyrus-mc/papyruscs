using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Imazen.WebP.Extern
{
    public static class LoadLibrary
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, uint dwFlags);

        public static bool EnsureLoadedByPath(string fullPath, bool throwException)
        {
            //canonicalize as much as we can
            fullPath = Path.GetFullPath(fullPath);
            lock (lockObj)
            {
                IntPtr handle;
                if (loaded.TryGetValue(fullPath, out handle))
                {
                    return true;
                }
                else
                {
                    handle = LoadByPath(fullPath, throwException);
                    if (handle != IntPtr.Zero)
                    {
                        loaded.Add(fullPath, handle);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Calls LoadLibraryEx with (Search DLL Load Dir and System32) flags. May increment reference count. Use EnsureLoadedByPath instead if you don't need a pointer.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static IntPtr LoadByPath(string fullPath, bool throwException)
        {
            const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;
            const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

            var moduleHandle = LoadLibraryEx(fullPath, IntPtr.Zero, LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LOAD_LIBRARY_SEARCH_SYSTEM32);
            if (moduleHandle == IntPtr.Zero && throwException)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return moduleHandle;
        }

  
        public static void LoadWebPOrFail()
        {
            if (!AutoLoadNearby("libwebp.dll", true))
            {
                throw new FileNotFoundException("Failed to locate libwebp.dll");
            }
        }
        /// <summary>
        /// Looks for 'name' inside /x86/ or /x64/ (depending on arch) subfolders of known assembly locations
        /// </summary>
        /// <param name="name"></param>
        /// <param name="throwFailure"></param>
        /// <returns></returns>
        public static bool AutoLoadNearby(string name, bool throwFailure)
        {
            var a = Assembly.GetExecutingAssembly();
            return AutoLoad(name, new string[]{Path.GetDirectoryName(a.Location), Path.GetDirectoryName(new Uri(a.CodeBase).LocalPath)},throwFailure,throwFailure);
        }

        static object lockObj = new object();
        static Dictionary<string, IntPtr> loaded = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Looks for 'name' inside /x86/ and /x64/ subfolders of 'folder', depending on executing architecture. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchFolders"></param>
        /// <param name="throwNotFound"></param>
        /// <param name="throwExceptions"></param>
        /// <returns></returns>
        public static bool AutoLoad(string name, string[] searchFolders, bool throwNotFound, bool throwExceptions)
        {
            string searched = "";
            foreach (string folder in searchFolders)
            {
                var basePath = Path.Combine(folder, (IntPtr.Size == 8) ? "x64" : "x86");
                var fullPath = Path.Combine(basePath, name);
                if (string.IsNullOrEmpty(Path.GetExtension(fullPath)))
                {
                    fullPath = fullPath + ".dll";
                }
                searched = searched + "\"" + fullPath + "\", ";
                if (File.Exists(fullPath))
                {
                    if (EnsureLoadedByPath(fullPath, throwExceptions))
                    {
                        return true;
                    }
                }
            }
            if (throwNotFound)
            {
                throw new FileNotFoundException("Failed to locate '" + name + "' as " + searched.TrimEnd(' ', ','));
            }
            return false;
        }

    }
}
