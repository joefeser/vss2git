using System;
using System.Collections.Generic;
using System.IO;
using Hpdi.VssLogicalLib;
using GitFastImport;

namespace Hpdi.GitFastImport
{
	public class Exporter
	{
		private List<int> commitMarks = new List<int>();
		private List<SimpleRevision> blobMarks = new List<SimpleRevision>();
		private int markCount = 0;
		
		public Exporter ()
		{
		}
		
		public void PushBlob(VssFileRevision vfr, Stream blobRawData){
			
			MemoryStream mem = new MemoryStream();
			StreamUtil.CopyStream(blobRawData, mem);
			
			System.Console.Write("blob\nmark :{0}\ndata {1}\n", ++markCount, mem.Length);
			blobMarks.Add(new SimpleRevision(markCount, vfr.File.Name));
			
			//Write out the file data to console
			mem.Seek(0, SeekOrigin.Begin); //reset file pointer
			using (Stream stdout = Console.OpenStandardOutput())
			mem.WriteTo(stdout);
			System.Console.Write('\n');
		}
		
		//Create a commit using the queued actions and blobs
		public bool Commit(String authorName, String authorEmail, DateTime authorTime,
		                   String committerName, String committerEmail, DateTime committerTime,
		                   String commitMessage){
			if (commitMarks.Count == 0)
			    System.Console.Write("reset refs/heads/master\n");
			System.Console.Write("commit refs/heads/master\n");
			System.Console.Write("mark :{0}\n", ++markCount);
			System.Console.Write("author {0} <{1}> {2} {3}\n", authorName, authorEmail,
			                     TimeStamp.GetTimeSinceUnixEpoch((authorTime)), "-0600");
			System.Console.Write("committer {0} <{1}> {2} {3}\n", committerName, committerEmail,
			                     TimeStamp.GetTimeSinceUnixEpoch(committerTime), "-0600");
			System.Console.Write("data {0}\n", commitMessage.Length + 1);
			System.Console.Write(commitMessage + "\n");
			//Set the commit ancestor to the last commit
			if (commitMarks.Count != 0)
				System.Console.Write("from :{0}\n", commitMarks[commitMarks.Count - 1]);
			foreach (var blob in blobMarks) {
				System.Console.Write("M 100644 :{0} {1}\n", blob.mark, blob.name);
			}
			blobMarks.Clear(); //Empty the blob list
			System.Console.Write("\n");
			commitMarks.Add(markCount);
			return true;
		}
		
		public class SimpleRevision {
			public int mark { get; private set; }
			public String name { get; private set; }
			public SimpleRevision(int mark, String name){ this.mark = mark; this.name = name;}
		}
	}
}

