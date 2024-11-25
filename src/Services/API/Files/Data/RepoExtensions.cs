using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace API.Files
{
    public static class Repository
    {

        /// <summary>
        /// Load all files of a directory as IFileInfo objects,
        /// ignore all child directories and files with excluded extensions
        /// </summary>
        /// <param name="path"></param>
        /// <param name="allowedExtensions"></param>
        /// <returns></returns>
        public static IEnumerable<IFileInfo> GetAllFilesInDirectory(string path, string[] allowedExtensions = null) 
        {
            // Create directory if not exists, and return empty list
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                return new List<IFileInfo>().AsEnumerable();
            }

            IEnumerable<IFileInfo> files = null;
            using(var provider = new PhysicalFileProvider(path)) 
            {
                var contents = provider.GetDirectoryContents("");
                // ignore all child directories
                files = contents.Where(f => f.IsDirectory == false);

                // get only files with allowed extensions
                if(allowedExtensions != null && allowedExtensions.Length > 0) {
                    files = files.Where(f => allowedExtensions.Contains(Path.GetExtension(f.Name)));
                }
            }

            return files;
        }


        /// <summary>
        /// Read all IFileInfos and retirn deserialised objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="files"></param>
        /// <returns></returns>
        public static IEnumerable<T> ReadAsJsonsAndDeserialize<T>(this IEnumerable<IFileInfo> files) 
        {
            var texts = files.Select(f => File.ReadAllText(f.PhysicalPath));
            var result = texts.Select(text => JsonConvert.DeserializeObject<T>(text));

            return result;
        }

        /// <summary>
        /// Read all IFileInfos and retirn deserialised objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="files"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> ReadAsJsonsAndDeserializeAsync<T>(this IEnumerable<IFileInfo> files) 
        {
            List<string> texts = new List<string>();
            foreach(var file in files) {

                string read = await File.ReadAllTextAsync(file.PhysicalPath);
                texts.Add(read);
            }

            var result = texts.Select(text => JsonConvert.DeserializeObject<T>(text));
            return result;
        }

        // public static string GetDirectoryName(this PermissionLevel level) => level switch
        // {
        //     PermissionLevel.Private => "users",
        //     PermissionLevel.Profile => "profiles",
        //     _ => throw new ArgumentException("Invalid permission level")
        // };
    }
}