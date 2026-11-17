
using System;
using System.Collections.Generic;
using System.IO;

namespace OS
{
	/// <summary>
	/// Description of Tuple
	/// </summary>
	public struct WalkTuple
	{
		private string dirpath;
		private string[] dirnames;
		private string[] filenames;
		
		public string DirPath
		{ 
			get { return this.dirpath; }
			// Make tuple read-only
			//set { this.dirpath = value };
		}
		
		public string[] DirNames
		{
			get { return this.dirnames; }
			// Make tuple read-only
			//set { this.dirnames = value };
		}
		
		public string[] FileNames
		{
			get { return this.filenames; }
			// Make tuple read-only
			//set { this.filenames = value };
		}
		
		/// <summary>
		/// Generate the file names in a directory tree by walking the tree 
		/// either top-down or bottom-up. For each directory in the tree 
		/// rooted at directory top (including top itself), it yields a 
		/// 3-tuple (dirpath, dirnames, filenames).
		/// </summary>
		/// <param name="dirpath">dirpath is a string, the path to the directory.</param>
		/// <param name="dirnames">dirnames is a list of the names of the 
		/// subdirectories in dirpath (excluding '.' and '..').
		/// </param>
		/// <param name="filenames">filenames is a list of the names of the 
		/// non-directory files in dirpath.
		/// </param>
		internal WalkTuple(string dirpath, string[] dirnames, string[] filenames)
		{
			this.dirpath = dirpath;
			this.dirnames = dirnames;
			this.filenames = filenames;
		}
	}
	
	/// <summary>
	/// Delegate declaration for an onerror handler function.
	/// </summary>
	public delegate void OnError(Exception exc);
	
	/// <summary>
	/// Description of OS.
	/// </summary>
	public sealed class OS
	{
		private const bool TOP_DOWN = true;
		private const bool FOLLOW_LINKS = false;
		
		public static WalkTuple[] Walk(string top)
		{
			return Walk(top, TOP_DOWN, null, FOLLOW_LINKS);
		}
		
		public static WalkTuple[] Walk(string top, bool topdown)
		{
			return Walk(top, topdown, null, FOLLOW_LINKS);
		}
		
		public static WalkTuple[] Walk(string top, bool topdown, OnError onError)
		{
			return Walk(top, topdown, onError, FOLLOW_LINKS);
		}
		
		public static WalkTuple[] Walk(string top, OnError onError, bool followLinks)
		{
			return Walk(top, TOP_DOWN, onError, followLinks);
		}
		
		public static WalkTuple[] Walk(string top, OnError onError)
		{
			return Walk(top, TOP_DOWN, onError, FOLLOW_LINKS);
		}
		
		public static WalkTuple[] Walk(string top, bool topdown, bool followLinks)
		{
			return Walk(top, topdown, null, followLinks);
		}
		
		public static WalkTuple[] Walk(string top, bool topdown, OnError onError, bool followLinks)
		{
			IList<WalkTuple> history = new List<WalkTuple>();
						
			Stack<string> pending = new Stack<string>();
			
			pending.Push(top);

			while(pending.Count > 0)
			{
				string dir = pending.Pop();
				
				try {
					WalkTuple t = Visit(dir);
					
					// Push in reverse order as the stack will be visited in 
					// reverse order.
					for(int i = t.DirNames.Length-1; i >= 0; i--)
					{
						pending.Push(Path.Combine(dir, t.DirNames[i]));
					}
					
					history.Add(t);
				} catch(DirectoryNotFoundException exc) {
					onError(exc);
				} catch(UnauthorizedAccessException exc) {
					onError(exc);
				} catch(IOException exc) {
					onError(exc);
				}			
			}
						
			WalkTuple[] tuples = new WalkTuple[history.Count];
			history.CopyTo(tuples, 0);
			return tuples;
		}
		
		private static WalkTuple Visit(string path)
		{
			WalkTuple t;
			
			if(!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException(path);				
			}			
			
			string[] dirs = Directory.GetDirectories(path);
			string[] files = Directory.GetFiles(path);
			
			for(int i = 0; i < dirs.Length; i++)
			{
				dirs[i] = Path.GetFileName(dirs[i]);
			}
			
			for(int i = 0; i < files.Length; i++)
			{
				files[i] = Path.GetFileName(files[i]);
			}
			
			t = new WalkTuple(
				Path.GetFullPath(path),
				dirs,
				files
			);
			
			return t;
		}
	}
}
