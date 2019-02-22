﻿#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/02/13 13:55
// Modified On:  2019/02/22 14:07
// Modified By:  Alexis

#endregion




using System;
using System.IO;
using System.Threading.Tasks;
using Anotar.Serilog;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Services.Configuration
{
  public class PluginConfigurationService : ConfigurationServiceBase
  {
    #region Properties & Fields - Non-Public

    protected ISMAPlugin Plugin { get; }

    #endregion




    #region Constructors

    public PluginConfigurationService(ISMAPlugin plugin)
    {
      Plugin = plugin;
      
      EnsureFolderExists();
    }

    #endregion




    #region Methods Impl

    protected override DirectoryPath GetDefaultConfigDirectoryPath() =>
      SMAFileSystem.ConfigDir.CombineFile(Plugin.AssemblyName).FullPath;

    #endregion
  }

  public class ConfigurationService : ConfigurationServiceBase
  {
    #region Properties & Fields - Non-Public

    private readonly DirectoryPath _dirPath;

    #endregion




    #region Constructors

    /// <inheritdoc />
    public ConfigurationService(DirectoryPath dirPath)
    {
      _dirPath = dirPath;

      EnsureFolderExists();
    }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    protected override DirectoryPath GetDefaultConfigDirectoryPath()
    {
      return _dirPath;
    }

    #endregion
  }


  public abstract class ConfigurationServiceBase
  {
    #region Methods

    public async Task<T> Load<T>(DirectoryPath dirPath = null)
    {
      dirPath = dirPath ?? GetDefaultConfigDirectoryPath();

      try
      {
        using (var stream = OpenConf(dirPath.FullPath, typeof(T), FileAccess.Read))
        using (var reader = new StreamReader(stream))
          return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false));
      }
      catch (Exception ex)
      {
        var filePath = GetConfigFilePath(dirPath, typeof(T));
        LogTo.Warning(ex, $"Failed to load config {filePath}");

        throw;
      }
    }

    public async Task<bool> Save<T>(T             config,
                                    DirectoryPath dirPath = null)
    {
      dirPath = dirPath ?? GetDefaultConfigDirectoryPath();

      try
      {
        using (var stream = OpenConf(dirPath.FullPath, typeof(T), FileAccess.Write))
        using (var writer = new StreamWriter(stream))
          await writer.WriteAsync(JsonConvert.SerializeObject(config, Formatting.Indented)).ConfigureAwait(false);

        return true;
      }
      catch (Exception ex)
      {
        var filePath = GetConfigFilePath(dirPath, typeof(T));
        LogTo.Warning(ex, $"Failed to save config {filePath}");

        throw;
      }
    }

    protected FileStream OpenConf(DirectoryPath dirPath,
                                  Type          confType,
                                  FileAccess    fileAccess)
    {
      if (!dirPath.Exists())
        return null;

      var filePath = GetConfigFilePath(dirPath, confType);

      return File.Open(filePath.FullPath, fileAccess == FileAccess.Read ? FileMode.OpenOrCreate : FileMode.Create, fileAccess);
    }

    protected virtual FilePath GetConfigFilePath(DirectoryPath dirPath,
                                                 Type          confType)
    {
      return dirPath.CombineFile(confType.Name + ".json");
    }

    protected void EnsureFolderExists()
    {
      var dirPath = GetDefaultConfigDirectoryPath();

      if (dirPath.Exists() == false)
        Directory.CreateDirectory(dirPath.FullPath);
    }

    #endregion




    #region Methods Abs

    protected abstract DirectoryPath GetDefaultConfigDirectoryPath();

    #endregion
  }
}
