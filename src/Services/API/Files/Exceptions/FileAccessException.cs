using System;

namespace API.Files
{
    public class FileAccessException : Exception
    {
        public FileAccessException(string message) : base(message)
        {
        }
    }
}