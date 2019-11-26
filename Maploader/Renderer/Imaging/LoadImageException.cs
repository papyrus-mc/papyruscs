using System;

namespace Maploader.Renderer.Imaging
{
    public class LoadImageException : Exception
    {
        private readonly string message; 
        public LoadImageException(string path, Exception inner) : base()
        {
            message = $"Unable to load {path}, because {inner}";
        }

        public override string ToString()
        {
            return message;
        }
    }
}