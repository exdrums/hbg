using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Files.Model;
using Common.Models;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace API.Files
{
    public class FilesRepo
    {
        public FilesRepo()
        {
            
        }

        #region ChartConfigs

        /// <summary>
        /// Get deserialised ChartConfigs from files in the directory (Private or Profile)
        /// </summary>
        /// <param name="subject">UserId or profile name</param>
        /// <param name="level"> Read pivate user's directory or profile's directory</param>
        /// <returns></returns>
        public IEnumerable<ChartConfig> GetChartConfigs(string subject, PermissionLevel level) => Repository
            .GetAllFilesInDirectory(Path.Combine(Constants.BASE_PATH, level.GetDirectoryName(), subject, "charts"))
            .ReadAsJsonsAndDeserialize<ChartConfig>();
        

        /// <summary>
        /// Get deserialised ChartConfigs from files in the directory async (Private or Profile)
        /// </summary>
        /// <param name="subject">UserId or profile name</param>
        /// <param name="level"> Read pivate user's directory or profile's directory</param>
        /// <returns></returns>
        public async Task<IEnumerable<ChartConfig>> GetChartConfigsAsync(string subject, PermissionLevel level) => await Repository
            .GetAllFilesInDirectory(Path.Combine(Constants.BASE_PATH, level.GetDirectoryName(), subject, "charts"), Constants.ConfigExtensions)
            .ReadAsJsonsAndDeserializeAsync<ChartConfig>();

        /// <summary>
        /// PUT edited/created ChartConfigs to the directory (Private or Profile)
        /// </summary>
        /// <param name="chartConfig">
        ///     Complete ChartConfig to save on the FileServer.
        ///     If ChartId is null 
        ///     => It is new Chart, saved at first time 
        ///     => create new Guid for the ChartId and use it as Filename
        ///     Use ChartConfig.PermissionLevel to determine the directory ("users" or "profiles")
        /// </param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="FileAccessException"></exception>
        public async Task PutChartConfigAsync(ChartConfig chartConfig, string subject, string userId)
        {
            if(string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            // if(chartConfig.OwnerId != userId) throw new FileAccessException("Cannot store object owned by another user");

            if(string.IsNullOrEmpty(chartConfig.OwnerId)) {
                chartConfig.OwnerId = userId;
            }
            
            // save charts not owned by the current user as copy
            if(chartConfig.OwnerId != userId) {
                chartConfig.Name += " (copy)";
                chartConfig.Description += $" (copied {chartConfig.OwnerId})";
                chartConfig.OwnerId = userId;
            }
            // update date marker
            chartConfig.ModifiedAt = DateTime.UtcNow;

            // get new Guid if ChartId is null
            if (string.IsNullOrEmpty(chartConfig.ChartId))
            {
                chartConfig.ChartId = Guid.NewGuid().ToString();
                chartConfig.OwnerId = userId;
            }

            var path = Path.Combine(Constants.BASE_PATH, chartConfig.PermissionLevel.GetDirectoryName(), subject, "charts", chartConfig.ChartId + ".json");
            var json = JsonConvert.SerializeObject(chartConfig);
            await File.WriteAllTextAsync(path, json);
        }

        #endregion
    }
}