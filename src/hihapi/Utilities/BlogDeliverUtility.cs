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

    public class BlogDeliverUtility
    {
        public static void UpdatePostSetting(BlogUserSetting setting)
        {
            string rootFolder = Startup.BlogFolder + "\\" + setting.DeployFolder;
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            String fileName = rootFolder + "\\blog_setting.json";
            var sjon = new BlogSettingJson();
            sjon.title = setting.Name;
            sjon.footer = setting.Comment;

            var jsonString = JsonSerializer.Serialize(sjon);
            File.WriteAllText(fileName, jsonString);
        }

        public static void DeliverPost(string deliveryFolder, BlogPost post, List<BlogCollection> blogCollections)
        {
            string rootFolder = Startup.BlogFolder + "\\" + deliveryFolder;
            string postFolder = rootFolder + "\\posts";
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            if (!Directory.Exists(postFolder))
            {
                Directory.CreateDirectory(postFolder);
            }

            // Blog define
            String fileName = rootFolder + "\\post_def.json";
            var jsonString = File.ReadAllText(fileName);
            var allposts = JsonSerializer.Deserialize<BlogPostDefJson[]>(jsonString);
            List<BlogPostDefJson> listposts = new List<BlogPostDefJson>(allposts);

            var newpost = new BlogPostDefJson
            {
                id = post.ID,
                title = post.Title,
                brief = post.Brief,
            };
            if (post.CreatedAt != null)
            {
                newpost.createdat = post.CreatedAt.Value.ToString();
            }
            else
            {
                newpost.createdat = DateTime.Now.ToString();
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
            jsonString = JsonSerializer.Serialize(listposts);
            File.WriteAllText(fileName, jsonString);

            // MD file
            fileName = postFolder + "\\" + post.ID.ToString() + ".md";
            File.WriteAllText(fileName, post.Content);
        }
 
        public static void RevokePostDeliver(string deliveryFolder, int postid)
        {
            string rootFolder = Startup.BlogFolder + "\\" + deliveryFolder;
            string postFolder = rootFolder + "\\posts";
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            if (!Directory.Exists(postFolder))
            {
                Directory.CreateDirectory(postFolder);
            }

            // Blog define
            String fileName = rootFolder + "\\post_def.json";
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

            // MD file
            fileName = postFolder + "\\" + postid.ToString() + ".md";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
