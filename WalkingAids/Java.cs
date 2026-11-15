
using System;
using System.Collections.Generic;
using System.IO;

namespace Java.Nio.File
{
	/// <summary>
	/// Defines the file tree traversal options.
	/// </summary>
	public enum FileVisitOption
	{
		NoFollowLinks,
		FollowLinks
	}
	
	/// <summary>
	/// Description of FileVisitResult.
	/// </summary>
	public enum FileVisitResult
	{
		/**
		 * Continue. When returned from a {@link FileVisitor#preVisitDirectory
		 * preVisitDirectory} method then the entries in the directory should also
		 * be visited.
		 */
		Continue,
		/**
		 * Terminate.
		 */
		Terminate,
		/**
		 * Continue without visiting the entries in this directory. This result
		 * is only meaningful when returned from the {@link
		 * FileVisitor#preVisitDirectory preVisitDirectory} method; otherwise
		 * this result type is the same as returning {@link #CONTINUE}.
		 */
		SkipSubTree,
		/**
		 * Continue without visiting the <em>siblings</em> of this file or directory.
		 * If returned from the {@link FileVisitor#preVisitDirectory
		 * preVisitDirectory} method then the entries in the directory are also
		 * skipped and the {@link FileVisitor#postVisitDirectory postVisitDirectory}
		 * method is not invoked.
		 */
		SkipSiblings
	}
	
	public interface FileVisitor
	{
		/// Invoked for a directory before entries in the directory are visited.
		/// 
		/// If this method returns <see cref="FileVisitResult.CONTINUE">CONTINUE</see>,
		/// then entries in the directory are visited. If this method returns
		/// <see cref="FileVisitResult.SKIP_SUBTREE">SKIP_SUBTREE</see> or
		/// <see cref="FileVisitResult.SKIP_SIBLINGS">SKIP_SIBLINGS</see> then entries in the
		/// directory (and any descendants) will not be visited.
		/// <param name="dir">a reference to the directory</param>
		/// <returns>the visit result</returns>
		FileVisitResult PreVisitDirectory(DirectoryInfo dir);
		
		/// <summary>
		/// Invoked for a file in a directory.
		/// </summary>
		/// <param name="file">a reference to the file</param>
		/// <returns></returns>
		FileVisitResult VisitFile(FileInfo file);
		
		/// <summary>
		/// Invoked for a file that could not be visited. This method is invoked
		/// if the file's attributes could not be read, the file is a directory
		/// that could not be opened, and other reasons.
		/// </summary>
		/// <param name="file">a reference to the file</param>
		/// <param name="exc">the I/O exception that prevented the file from being visited</param>
		/// <returns>the visit result</returns>
		FileVisitResult VisitFileFailed(FileSystemInfo file, IOException exc);
		
		/// <summary>
		/// Invoked for a directory after entries in the directory, and all of their
		/// descendants, have been visited. This method is also invoked when iteration
		/// of the directory completes prematurely (by a <see cref="VisitFile">VisitFile</see>
		/// method returning <see cref="FileVisitResult.SKIP_SIBLINGS">SKIP_SIBLINGS</see>,
		/// or an I/O error when iterating over the directory).
		/// </summary>
		/// <param name="dir">a reference to the directory</param>
		/// <param name="exc">
		/// <c>null</c> if the iteration of the directory completes without
		/// an error; otherwise the I/O exception that caused the iteration
		/// of the directory to complete prematurely
		/// </param>
		/// <returns>the visit result</returns>
		FileVisitResult PostVisitDirectory(DirectoryInfo dir, IOException exc);
	}
	
	/// <summary>
	/// A simple visitor of files with default behavior to visit all files and to re-throw I/O errors.
	/// Methods in this class may be overridden subject to their general contract.
	/// </summary>
	public class SimpleFileVisitor : FileVisitor
	{
		public virtual FileVisitResult PreVisitDirectory(DirectoryInfo dir)
		{
			return FileVisitResult.Continue;
		}
		
		public virtual FileVisitResult VisitFile(FileInfo file)
		{
			return FileVisitResult.Continue;
		}
		
		public virtual FileVisitResult VisitFileFailed(FileSystemInfo file, IOException exc)
		{
			return FileVisitResult.Continue;
		}
		
		public virtual FileVisitResult PostVisitDirectory(DirectoryInfo dir, IOException exc)
		{
			return FileVisitResult.Continue;
		}
	}
	
	/// <summary>
	/// Description of Files.
	/// </summary>
	public sealed class Files
	{		
		public static FileSystemInfo WalkFileTree(FileSystemInfo start,
		                                          IList<FileVisitOption> options,
		                                          int maxDepth,
		                                          FileVisitor visitor)
		{
			FileVisitResult result;
			
			if(System.IO.File.Exists(start.FullName)){
				result = VisitFile(new FileInfo(start.FullName), visitor);
			}
			else if(System.IO.Directory.Exists(start.FullName))
			{
				result = VisitDirectory(new DirectoryInfo(start.FullName), visitor);
			}
			else
			{
				Console.Error.WriteLine("Unknown path type: {0}", start.FullName);
			}
			return start;
		}
		
		public static FileSystemInfo WalkFileTree(FileSystemInfo start, FileVisitor visitor)
		{
			return WalkFileTree(start,
			                    new List<FileVisitOption>(),
			                    int.MaxValue,
			                    visitor);
		}
		
		private static FileVisitResult VisitDirectory(DirectoryInfo dir, FileVisitor visitor)
		{
			FileVisitResult result;
			List<IOException> exceptions = new List<IOException>();
			
			if(FSUtils.IsReparsePoint(dir))
			{
				result = visitor.VisitFileFailed(dir, new IOException("Skip reparse point"));
				return result;
			}
			
			result = visitor.PreVisitDirectory(dir);
			
			if(result == FileVisitResult.Terminate)
				return FileVisitResult.Terminate;
			else if(
				result == FileVisitResult.SkipSubTree)
				return FileVisitResult.Continue;
			
			FileInfo[] files = dir.GetFiles();
			foreach(FileInfo f in files)
			{
				try
				{
					result = VisitFile(f, visitor);
				}
				catch(Exception e)
				{
					result = visitor.VisitFileFailed(f, new IOException("Visit file failed", e));
				}
				
				if(result == FileVisitResult.Terminate)
					return FileVisitResult.Terminate;
				else if(result == FileVisitResult.SkipSubTree)
					return FileVisitResult.Continue;
				else if(result == FileVisitResult.SkipSiblings)
					break;
			}
			
			// Do not traverse down to sub-directories.
			if(result == FileVisitResult.SkipSubTree)
				return result;
			
			DirectoryInfo[] dirs = dir.GetDirectories();
			foreach(DirectoryInfo d in dirs)
			{
				try
				{
					result = VisitDirectory(d, visitor);
				}
				catch(Exception e)
				{
					result = visitor.VisitFileFailed(d, new IOException("Visit directory failed", e));
				}
				
				if(result == FileVisitResult.Terminate)
					return FileVisitResult.Terminate;
				else if(result == FileVisitResult.SkipSubTree)
					return FileVisitResult.Continue;
				else if(result == FileVisitResult.SkipSiblings)
					break;
			}
			
			IOException ex = exceptions.Count > 0 ? exceptions[0]: null;
			result = visitor.PostVisitDirectory(dir, ex);
			
			// Added for consistency and not drop down to the default action.
			if(result == FileVisitResult.Terminate)
				return result;
			
			return result;
		}
		
		private static FileVisitResult VisitFile(FileInfo file, FileVisitor visitor)
		{
			FileVisitResult result;
			
			if(FSUtils.IsReparsePoint(file))
			{
				result = visitor.VisitFileFailed(file, new IOException("Skip reparse point"));
				return result;
			}
			
			result = visitor.VisitFile(file);
			return result;
		}
	}
	
	internal sealed class FSUtils
	{
		internal static bool IsTypeOf(FileSystemInfo fsi, FileAttributes attr)
		{
			return (fsi.Attributes & attr) == attr;
		}
		
		internal static bool IsReparsePoint(FileSystemInfo fsi)
		{
			return IsTypeOf(fsi, FileAttributes.ReparsePoint);
		}
	}
}