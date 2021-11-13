// <copyright file="GameHelperTransformer.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace Launcher
{
    using System;
    using System.IO;
    using System.Reflection;
    using AsmResolver.PE;
    using AsmResolver.PE.File;
    using AsmResolver.PE.File.Headers;
    using AsmResolver.PE.Win32Resources.Builder;
    using AsmResolver.PE.Win32Resources.Version;

    /// <summary>
    ///     A helper class to transform the GameHelper tool name and metadata info.
    /// </summary>
    public static class GameHelperTransformer
    {
        private const string GameHelperFileName = "GameHelper.exe";

        /// <summary>
        ///     Transforms the GameHelper tool name and metadata info.
        /// </summary>
        /// <param name="newName">string to replace the name and metadata info with.</param>
        /// <returns></returns>
        public static string TransformGameHelperExecutable(string newName)
        {
            var currentProcessPath = Assembly.GetExecutingAssembly().Location;
            var directoryPath = Path.GetDirectoryName(currentProcessPath);
            var gameHelperPath = Path.Join(directoryPath, GameHelperFileName);
            var fileInfo = new FileInfo(gameHelperPath);
            if (!fileInfo.Exists)
            {
                throw new Exception("GameHelper.exe does not exist");
            }

            var newPath = Path.Join(directoryPath, $"{newName}.exe");
            TemporaryFileManager.AddFile(newPath);
            TransformExecutable(gameHelperPath, newPath, newName);
            return newPath;
        }

        private static void TransformExecutable(string inputPath, string outputPath, string infoName)
        {
            var peFile = PEFile.FromFile(inputPath);
            var peImage = PEImage.FromFile(peFile);
            var versionInfo = VersionInfoResource.FromDirectory(peImage.Resources);
            var stringInfo = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            var stringTable = stringInfo.Tables[0];
            stringTable[StringTable.CommentsKey] = "";
            stringTable[StringTable.CompanyNameKey] = infoName;
            stringTable[StringTable.FileDescriptionKey] = infoName;
            stringTable[StringTable.InternalNameKey] = infoName;
            stringTable[StringTable.OriginalFilenameKey] = Path.GetFileName(outputPath);
            stringTable[StringTable.ProductNameKey] = infoName;
            versionInfo.WriteToDirectory(peImage.Resources);
            var resourceDirectoryBuffer = new ResourceDirectoryBuffer();
            resourceDirectoryBuffer.AddDirectory(peImage.Resources);
            var directory = peFile.OptionalHeader.GetDataDirectory(DataDirectoryIndex.ResourceDirectory);
            var section = peFile.GetSectionContainingRva(directory.VirtualAddress);
            section.Contents = resourceDirectoryBuffer;
            peFile.Write(outputPath);
        }
    }
}
