using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using hihapi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hihapi.Utilities
{
    public class BlogSettingJson
    {
        public string title { get; set; }
        public string footer { get; set; }
        public string author { get; set; }
        public string authordesp { get; set; }
        public string authorimg { get; set; }
    }
    public class BlogPostDefJson
    {
        public int id { get; set; }
        public string title { get; set; }
        public string brief { get; set; }
        public string createdat { get; set; }
        public List<String> collection { get; set; }
        public List<string> tag { get; set; }

        public BlogPostDefJson()
        {
            collection = new List<string>();
            tag = new List<string>();
        }
    }

    public class BlogDeployUtility
    {
        const string PostDefineFile = @"post_def.json";
        const string SettingFile = @"blog_setting.json";
        const string PostsFolder = @"posts";

        private static string SanitizeDeployFolder(string deployFolder)
        {
            if (string.IsNullOrWhiteSpace(deployFolder))
                throw new ArgumentException("Deploy folder cannot be empty");

            // Reject path traversal sequences
            if (deployFolder.Contains("..") || deployFolder.Contains('/') || deployFolder.Contains('\\'))
                throw new ArgumentException("Deploy folder contains invalid characters");

            var fullPath = Path.GetFullPath(Path.Combine(HIHAPIUtility.BlogFolder, deployFolder));
            var blogDir = Path.GetFullPath(HIHAPIUtility.BlogFolder);
            if (!fullPath.StartsWith(blogDir, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Deploy folder is outside the blog directory");

            return fullPath;
        }

        public static async Task UpdatePostSettingAsync(BlogUserSetting setting)
        {
            string rootFolder = SanitizeDeployFolder(setting.DeployFolder);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            String fileName = Path.Combine(rootFolder, SettingFile);
            var sjon = new BlogSettingJson();
            sjon.title = setting.Name;
            sjon.footer = setting.Comment;

            var jsonString = JsonSerializer.Serialize(sjon);
            await File.WriteAllTextAsync(fileName, jsonString);
        }

        public static async Task DeployPost(string deployFolder, BlogPost post, List<BlogCollection> blogCollections)
        {
            if (String.IsNullOrEmpty(deployFolder) || !Directory.Exists(HIHAPIUtility.BlogFolder))
            {
                throw new ArgumentException("Deploy Folder " + deployFolder + "; Blog Folder " + HIHAPIUtility.BlogFolder);
            }

            string rootFolder = SanitizeDeployFolder(deployFolder);
            string postFolder = Path.Combine(rootFolder, PostsFolder);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            if (!Directory.Exists(postFolder))
            {
                Directory.CreateDirectory(postFolder);
            }

            // Blog define
            List<BlogPostDefJson> listposts = new List<BlogPostDefJson>();
            var jsonstr = "";
            String fileName = Path.Combine(rootFolder, PostDefineFile);
            if (File.Exists(fileName))
            {
                jsonstr = await File.ReadAllTextAsync(fileName);
                listposts = JsonSerializer.Deserialize<List<BlogPostDefJson>>(jsonstr);
            }

            var newpost = new BlogPostDefJson
            {
                id = post.ID,
                title = post.Title,
                brief = post.Brief,
            };
            // Tags
            foreach(var tagterm in post.BlogPostTags)
            {
                newpost.tag.Add(tagterm.Tag);
            }

            // Post itself
            if (post.CreatedAt.HasValue)
            {
                newpost.createdat = post.CreatedAt.Value.ToString("s");
            }
            else
            {
                newpost.createdat = DateTime.Now.ToString("s");
            }

            if (blogCollections != null && blogCollections.Count > 0)
            {
                foreach (var coll in post.BlogPostCollections)
                {
                    var collname = "";
                    foreach (var bcoll in blogCollections)
                    {
                        if (bcoll.ID == coll.CollectionID)
                        {
                            collname = bcoll.Name;
                        }
                    }
                    if (!string.IsNullOrEmpty(collname))
                        newpost.collection.Add(collname);
                }
            }
            var postidx = listposts.FindIndex(p => p.id == newpost.id);
            if (postidx == -1)
            {
                listposts.Add(newpost);
            }
            else
            {
                listposts[postidx] = newpost;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            {
                using FileStream createStream = File.Create(fileName);
                await JsonSerializer.SerializeAsync(createStream, listposts, options);
            }
            //File.WriteAllText(fileName, jsonstr);

            // MD file
            fileName = Path.Combine(postFolder, post.ID.ToString() + ".md");
            using (StreamWriter writer = File.CreateText(fileName))
            {
                await writer.WriteAsync(post.Content);
            }
        }
 
        public static void RevokePostDeliver(string deployFolder, int postid)
        {
            if (String.IsNullOrEmpty(deployFolder) || !Directory.Exists(HIHAPIUtility.BlogFolder))
            {
                return;
            }

            string rootFolder = SanitizeDeployFolder(deployFolder);
            if (!Directory.Exists(rootFolder))
            {
                return;
            }

            // Blog define
            String fileName = Path.Combine(rootFolder, PostDefineFile);
            if (File.Exists(fileName))
            {
                var jsonString = File.ReadAllText(fileName);
                var allposts = JsonSerializer.Deserialize<BlogPostDefJson[]>(jsonString);
                List<BlogPostDefJson> listposts = new List<BlogPostDefJson>(allposts);

                var postidx = listposts.FindIndex(p => p.id == postid);
                if (postidx != -1)
                {
                    listposts.RemoveAt(postidx);
                    jsonString = JsonSerializer.Serialize(listposts);
                    File.WriteAllText(fileName, jsonString);
                }
            }

            // MD file
            string postFolder = Path.Combine(rootFolder, PostsFolder);
            if (Directory.Exists(postFolder))
            {
                fileName = Path.Combine(postFolder, postid.ToString() + ".md");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }
    }
}
