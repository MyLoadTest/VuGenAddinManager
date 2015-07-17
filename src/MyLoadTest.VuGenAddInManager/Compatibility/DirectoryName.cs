// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ICSharpCode.Core;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// Represents a path to a directory.
    /// The equality operator is overloaded to compare for path equality (case insensitive, normalizing paths with '..\')
    /// </summary>
    [TypeConverter(typeof(DirectoryNameConverter))]
    public sealed class DirectoryName : PathName
    {
        public DirectoryName(string path)
            : base(path)
        {
        }

        [Obsolete("The input already is a DirectoryName")]
        public DirectoryName(DirectoryName path)
            : base(path)
        {
        }

        public static bool operator ==(DirectoryName left, DirectoryName right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(DirectoryName left, DirectoryName right)
        {
            return !(left == right);
        }

        [Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
        public static bool operator ==(DirectoryName left, string right)
        {
            return (string)left == right;
        }

        [Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
        public static bool operator !=(DirectoryName left, string right)
        {
            return (string)left != right;
        }

        [Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
        public static bool operator ==(string left, DirectoryName right)
        {
            return left == (string)right;
        }

        [Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
        public static bool operator !=(string left, DirectoryName right)
        {
            return left != (string)right;
        }

        /// <summary>
        /// Creates a DirectoryName instance from the string.
        /// It is valid to pass null or an empty string to this method (in that case, a null reference will be returned).
        /// </summary>
        public static DirectoryName Create(string directoryName)
        {
            return string.IsNullOrEmpty(directoryName) ? null : new DirectoryName(directoryName);
        }

        [Obsolete("The input already is a DirectoryName")]
        public static DirectoryName Create(DirectoryName directoryName)
        {
            return directoryName;
        }

        /// <summary>
        /// Combines this directory name with a relative path.
        /// </summary>
        public DirectoryName Combine(DirectoryName relativePath)
        {
            return relativePath == null ? null : Create(Path.Combine(NormalizedPath, relativePath));
        }

        /// <summary>
        /// Combines this directory name with a relative path.
        /// </summary>
        public FileName Combine(FileName relativePath)
        {
            return relativePath == null ? null : FileName.Create(Path.Combine(NormalizedPath, relativePath));
        }

        /// <summary>
        /// Combines this directory name with a relative path.
        /// </summary>
        public FileName CombineFile(string relativeFileName)
        {
            return relativeFileName == null ? null : FileName.Create(Path.Combine(NormalizedPath, relativeFileName));
        }

        /// <summary>
        /// Combines this directory name with a relative path.
        /// </summary>
        public DirectoryName CombineDirectory(string relativeDirectoryName)
        {
            return relativeDirectoryName == null ? null : Create(Path.Combine(NormalizedPath, relativeDirectoryName));
        }

        /// <summary>
        /// Converts the specified absolute path into a relative path (relative to <c>this</c>).
        /// </summary>
        public DirectoryName GetRelativePath(DirectoryName path)
        {
            return path == null ? null : Create(FileUtility.GetRelativePath(NormalizedPath, path));
        }

        /// <summary>
        /// Converts the specified absolute path into a relative path (relative to <c>this</c>).
        /// </summary>
        public FileName GetRelativePath(FileName path)
        {
            return path == null ? null : FileName.Create(FileUtility.GetRelativePath(NormalizedPath, path));
        }

        /// <summary>
        /// Gets the directory name as a string, including a trailing backslash.
        /// </summary>
        public string ToStringWithTrailingBackslash()
        {
            // trailing backslash exists in normalized version for root of drives ("C:\")
            return NormalizedPath.EndsWith("\\", StringComparison.Ordinal) ? NormalizedPath : NormalizedPath + "\\";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DirectoryName);
        }

        public bool Equals(DirectoryName other)
        {
            return other != null && string.Equals(NormalizedPath, other.NormalizedPath, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizedPath);
        }
    }
}