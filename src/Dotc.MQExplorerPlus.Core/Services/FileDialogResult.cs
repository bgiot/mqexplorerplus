﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQExplorerPlus.Core.Services
{
    /// <summary>
    /// Contains the result information about the work with the file dialog box.
    /// </summary>
    public class FileDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogResult"/> class with null values.
        /// Use this constructor when the user canceled the file dialog box.
        /// </summary>
        public FileDialogResult() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDialogResult"/> class.
        /// </summary>
        /// <param name="fileName">The filename entered by the user.</param>
        /// <param name="selectedFileType">The file type selected by the user.</param>
        public FileDialogResult(string fileName, FileType selectedFileType)
        {
            FileName = fileName;
            SelectedFileType = selectedFileType;
        }


        /// <summary>
        /// Gets a value indicating whether this instance contains valid data. This property returns <c>false</c>
        /// when the user canceled the file dialog box.
        /// </summary>
        public bool IsValid => FileName != null && SelectedFileType != null;

        /// <summary>
        /// Gets the filename entered by the user or <c>null</c> when the user canceled the dialog box.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the file type selected by the user or <c>null</c> when the user canceled the dialog box.
        /// </summary>
        public FileType SelectedFileType { get; }
    }
}
