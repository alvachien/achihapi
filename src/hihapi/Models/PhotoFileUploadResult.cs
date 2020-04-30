using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hihapi.Models
{
    public class PhotoFileUploadResult
    {
		public const string HandlerPath = "/";

		public string group { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public int size { get; set; }
		public string progress { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string error { get; set; }
	}
}
