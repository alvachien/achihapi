using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        [HttpGet]
        public List<PhotoViewModel> GetPhotos()
        {
            List<PhotoViewModel> rstFiles = new List<PhotoViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            try
            {
                String queryString = @"SELECT [PhotoID]
                      ,[Title]
                      ,[Desp]
                      ,[UploadedAt]
                      ,[UploadedBy]
                      ,[OrgFileName]
                      ,[PhotoUrl]
                      ,[PhotoThumbUrl]
                      ,[IsOrgThumb]
                      ,[ThumbCreatedBy]
                      ,[CameraMaker]
                      ,[CameraModel]
                      ,[LensModel]
                      ,[AVNumber]
                      ,[ShutterSpeed]
                      ,[ISONumber]
                      ,[IsPublic]
                      ,[EXIFInfo]
                  FROM [dbo].[Photo] 
                  WHERE [IsPublic] = 1";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PhotoViewModel rst = new PhotoViewModel();

                    //cmd.Parameters.AddWithValue("@PhotoID", nid.ToString("N"));   // 1
                    rst.PhotoId = reader.GetString(0);
                    //cmd.Parameters.AddWithValue("@Title", nid.ToString("N"));     // 2
                    rst.Title = reader.GetString(1);
                    //cmd.Parameters.AddWithValue("@Desp", nid.ToString("N"));      // 3
                    rst.Desp = reader.GetString(2);
                    //cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);     // 4
                    rst.UploadedTime = reader.GetDateTime(3);
                    //cmd.Parameters.AddWithValue("@UploadedBy", "Tester");         // 5
                    //cmd.Parameters.AddWithValue("@OrgFileName", rst.OrgFileName); // 6
                    rst.OrgFileName = reader.GetString(5);
                    //cmd.Parameters.AddWithValue("@PhotoUrl", rst.FileUrl);        // 7
                    rst.FileUrl = reader.GetString(6); // 7 - 1
                    //cmd.Parameters.AddWithValue("@PhotoThumbUrl", rst.ThumbnailFileUrl); // 8
                    if (!reader.IsDBNull(7)) // 8 - 1
                        rst.ThumbnailFileUrl = reader.GetString(7);
                    //cmd.Parameters.AddWithValue("@IsOrgThumb", bThumbnailCreated);    // 9
                    //cmd.Parameters.AddWithValue("@ThumbCreatedBy", 2); // 1 for ExifTool, 2 stands for others; // 10
                    //cmd.Parameters.AddWithValue("@CameraMaker", "To-do"); // 11
                    //cmd.Parameters.AddWithValue("@CameraModel", "To-do"); // 12
                    //cmd.Parameters.AddWithValue("@LensModel", "To-do");   // 13
                    //cmd.Parameters.AddWithValue("@AVNumber", "To-do");    // 14
                    //cmd.Parameters.AddWithValue("@ShutterSpeed", "To-do"); // 15
                    //cmd.Parameters.AddWithValue("@IsPublic", true);       // 16
                    //cmd.Parameters.AddWithValue("@ISONumber", 0);         // 17

                    //String strJson = Newtonsoft.Json.JsonConvert.SerializeObject(rst.ExifTags);
                    //cmd.Parameters.AddWithValue("@EXIF", strJson);        // 18
                    if (!reader.IsDBNull(17))
                        rst.ExifTags = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExifTagItem>>(reader.GetString(17));
                    rstFiles.Add(rst);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return rstFiles;
        }
        
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/photo
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]PhotoViewModel vm)
        {
            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            try
            {
                // ID is set to identity
                String queryString = @"INSERT INTO [dbo].[Photo]
                           ([PhotoID]
                           ,[Title]
                           ,[Desp]
                           ,[UploadedAt]
                           ,[UploadedBy]
                           ,[OrgFileName]
                           ,[PhotoUrl]
                           ,[PhotoThumbUrl]
                           ,[IsOrgThumb]
                           ,[ThumbCreatedBy]
                           ,[CameraMaker]
                           ,[CameraModel]
                           ,[LensModel]
                           ,[AVNumber]
                           ,[ShutterSpeed]
                           ,[ISONumber]
                           ,[IsPublic]
                           ,[EXIFInfo])
                     VALUES
                           (@PhotoID
                           ,@Title
                           ,@Desp
                           ,@UploadedAt
                           ,@UploadedBy
                           ,@OrgFileName
                           ,@PhotoUrl
                           ,@PhotoThumbUrl
                           ,@IsOrgThumb
                           ,@ThumbCreatedBy
                           ,@CameraMaker
                           ,@CameraModel
                           ,@LensModel
                           ,@AVNumber
                           ,@ShutterSpeed
                           ,@ISONumber
                           ,@IsPublic
                           ,@EXIF)";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@PhotoID", vm.PhotoId);
                cmd.Parameters.AddWithValue("@Title", vm.Title);
                cmd.Parameters.AddWithValue("@Desp", vm.Desp);
                cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@UploadedBy", "Tester");
                cmd.Parameters.AddWithValue("@OrgFileName", vm.OrgFileName);
                cmd.Parameters.AddWithValue("@PhotoUrl", vm.FileUrl);
                cmd.Parameters.AddWithValue("@PhotoThumbUrl", vm.ThumbnailFileUrl);
                cmd.Parameters.AddWithValue("@IsOrgThumb", vm.IsOrgThumbnail);
                cmd.Parameters.AddWithValue("@ThumbCreatedBy", 2); // 1 for ExifTool, 2 stands for others
                cmd.Parameters.AddWithValue("@CameraMaker", "To-do");
                cmd.Parameters.AddWithValue("@CameraModel", "To-do");
                cmd.Parameters.AddWithValue("@LensModel", "To-do");
                cmd.Parameters.AddWithValue("@AVNumber", "To-do");
                cmd.Parameters.AddWithValue("@ShutterSpeed", "To-do");
                cmd.Parameters.AddWithValue("@IsPublic", true);
                cmd.Parameters.AddWithValue("@ISONumber", 0);

                String strJson = Newtonsoft.Json.JsonConvert.SerializeObject(vm.ExifTags);
                cmd.Parameters.AddWithValue("@EXIF", strJson);

                //SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                //idparam.Direction = ParameterDirection.Output;

                try
                {
                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception exp)
                {
                    System.Diagnostics.Debug.WriteLine(exp.Message);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return new ObjectResult(vm);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
