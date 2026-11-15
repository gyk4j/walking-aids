
using System;
using System.IO;
using Java.Nio.File;

namespace WalkingAids
{
	class Program
	{
		//private const string START_DIR = @"C:\Windows";
		private static string START_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		
		public static void Main(string[] args)
		{
			FilesWalkFileTreeTest();
			Console.WriteLine("Press any key to continue . . . ");
			Console.ReadKey(true);
		}

		private class ConsoleFileVisitor : SimpleFileVisitor
		{
			public override FileVisitResult PreVisitDirectory(DirectoryInfo dir)
			{
				Console.WriteLine("PreVisitDirectory: {0}", dir.FullName);
				return base.PreVisitDirectory(dir);
			}
			
			public override FileVisitResult VisitFile(FileInfo file)
			{
				Console.WriteLine("VisitFile: {0}", file.FullName);
				return base.VisitFile(file);
			}
			
			public override FileVisitResult VisitFileFailed(FileSystemInfo file, IOException exc)
			{
				Console.Error.WriteLine("VisitFileFailed: {0}, Exception: {1}", file.FullName, exc.Message);
				return base.VisitFileFailed(file, exc);
			}
			
			public override FileVisitResult PostVisitDirectory(DirectoryInfo dir, IOException exc)
			{
				if (exc != null)
				{
					Console.WriteLine("PostVisitDirectory: {0}, Exception: {1}", dir.FullName, exc.Message);
				}
				else
				{
					Console.WriteLine("PostVisitDirectory: {0}", dir.FullName);
				}
				return base.PostVisitDirectory(dir, exc);
			}
		}
		
		private static void FilesWalkFileTreeTest()
		{
			DirectoryInfo dir = new DirectoryInfo(START_DIR);
			FileVisitor visitor = new ConsoleFileVisitor();
			Files.WalkFileTree(dir, visitor);
		}
	}
}