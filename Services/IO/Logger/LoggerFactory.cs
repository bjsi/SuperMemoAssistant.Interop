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
// Created On:   2020/01/22 09:58
// Modified On:  2020/01/22 10:16
// Modified By:  Alexis

#endregion




using System;
using Anotar.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using SuperMemoAssistant.Interop;
using SuperMemoAssistant.Services.Configuration;
using SuperMemoAssistant.Sys.IO;

namespace SuperMemoAssistant.Services.IO.Logger
{
  /*
   * This is separate from Logger due to race condition with Fody's Anotar.Serilog
   * See https://github.com/Fody/Anotar/issues/114
   */
  public static class LoggerFactory
  {
    #region Constants & Statics

    private const string OutputFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    #endregion




    #region Methods

    public static Logger Create(
      string                   appName,
      ConfigurationServiceBase sharedConfig,
      LoggerConfigPredicate    configPredicate = null)
    {
      if (Svc.Logger != null)
        throw new NotSupportedException();

      var config      = LoadConfig(sharedConfig);
      var levelSwitch = new LoggingLevelSwitch(config.LogLevel);

      var loggerConfig = new LoggerConfiguration()
                         .MinimumLevel.ControlledBy(levelSwitch)
                         .Enrich.WithExceptionDetails()
                         .Enrich.WithDemystifiedStackTraces()
                         .WriteTo.Debug(outputTemplate: OutputFormat)
                         .WriteTo.Async(
                           a =>
                             a.RollingFile(
                               GetLogFilePath(appName).FullPath,
                               fileSizeLimitBytes: 5242880, // Math.Max(ConfigMgr.AppConfig.LogMaxSize, 5242880),
                               retainedFileCountLimit: 7,
                               shared: false,
                               outputTemplate: OutputFormat
                             ));
      //.WriteTo.File(
      //  GetLogFilePath(appName).FullPath,
      //  outputTemplate: OutputFormat);
      //.WriteTo.RollingFile(
      //  GetLogFilePath(appName).FullPath,
      //  fileSizeLimitBytes: 5242880,
      //  retainedFileCountLimit: 7,
      //  shared: false,
      //  outputTemplate: OutputFormat
      //);

      if (configPredicate != null)
        loggerConfig = configPredicate(loggerConfig);

      Log.Logger = loggerConfig.CreateLogger();

      return new Logger(config, levelSwitch);
    }

    public static LoggerCfg LoadConfig(ConfigurationServiceBase sharedConfig)
    {
      try
      {
        return sharedConfig.Load<LoggerCfg>().Result ?? new LoggerCfg();
      }
      catch (Exception ex)
      {
        LogTo.Error(ex, "Exception while loading logger config");

        return new LoggerCfg();
      }
    }

    private static FilePath GetLogFilePath(string appName)
    {
      var logDir = SMAFileSystem.LogDir;

      if (logDir.Exists() == false)
        logDir.Create();

      var filePath = logDir.CombineFile($"{appName}-{{Date}}.log");

      return filePath;
    }

    #endregion




    public delegate LoggerConfiguration LoggerConfigPredicate(LoggerConfiguration config);
  }
}
