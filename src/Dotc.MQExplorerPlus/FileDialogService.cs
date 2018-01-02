#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using Dotc.MQExplorerPlus.Core.Services;
using Dotc.Wpf;
using Microsoft.Win32;

namespace Dotc.MQExplorerPlus
{

    [Export(typeof(IFileDialogService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class FileDialogService : IFileDialogService
    {

        public FileDialogService()
        {
        }

        public FileDialogResult ShowOpenFileDialog(object owner, IList<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
        {
            if (fileTypes == null) { throw new ArgumentNullException(nameof(fileTypes)); }
            if (!fileTypes.Any()) { throw new ArgumentException("The fileTypes collection must contain at least one item."); }

            FileDialogResult result = null;

            UIDispatcher.Execute(() =>
            {
                var dialog = new OpenFileDialog();
                result = ShowFileDialog(owner, dialog, fileTypes, defaultFileType, defaultFileName);
            });

            return result;
        }


        public FileDialogResult ShowSaveFileDialog(object owner, IList<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
        {
            if (fileTypes == null) { throw new ArgumentNullException(nameof(fileTypes)); }
            if (!fileTypes.Any()) { throw new ArgumentException("The fileTypes collection must contain at least one item."); }

            FileDialogResult result = null;

            UIDispatcher.Execute(() =>
            {
                var dialog = new SaveFileDialog();
                result = ShowFileDialog(owner, dialog, fileTypes, defaultFileType, defaultFileName);
            });

            return result;
        }

        private static FileDialogResult ShowFileDialog(object owner, FileDialog dialog, IList<FileType> fileTypes,
            FileType defaultFileType, string defaultFileName)
        {
            var filterIndex = fileTypes.ToList().IndexOf(defaultFileType);
            if (filterIndex >= 0) { dialog.FilterIndex = filterIndex + 1; }
            if (!string.IsNullOrEmpty(defaultFileName))
            {
                dialog.FileName = Path.GetFileName(defaultFileName);
                var directory = Path.GetDirectoryName(defaultFileName);
                if (!string.IsNullOrEmpty(directory))
                {
                    dialog.InitialDirectory = directory;
                }
            }

            dialog.Filter = CreateFilter(fileTypes);
            if (dialog.ShowDialog(owner as Window) == true)
            {
                filterIndex = dialog.FilterIndex - 1;
                if (filterIndex >= 0 && filterIndex < fileTypes.Count)
                {
                    defaultFileType = fileTypes.ElementAt(filterIndex);
                }
                else
                {
                    defaultFileType = null;
                }
                return new FileDialogResult(dialog.FileName, defaultFileType);
            }
            return new FileDialogResult();
        }

        private static string CreateFilter(IEnumerable<FileType> fileTypes)
        {
            var filter = "";
            foreach (var fileType in fileTypes)
            {
                if (!string.IsNullOrEmpty(filter)) { filter += "|"; }
                filter += fileType.Description + "|*" + fileType.FileExtension;
            }
            return filter;
        }
    }
}
