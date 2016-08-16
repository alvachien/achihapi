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
    public class AlbumController : Controller
    {
        private IConfiguration Configuration { get; set; }

        public AlbumController(IConfiguration config)
        {
            Configuration = config;
        }

        // GET: api/album
        [HttpGet]
        public IEnumerable<AlbumViewModel> Get()
        {
            List<AlbumViewModel> listVm = new List<AlbumViewModel>();
            SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            try
            {
                String queryString = @"With albumfirstphoto as (select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                    join dbo.Photo as tabc
	                    on tabb.PhotoID = tabc.PhotoID
	                    group by tabb.AlbumID)
                    select taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                    tabb.PhotoCount, tabb.ThumbUrl
	                    from dbo.Album as taba
	                    left outer join albumfirstphoto as tabb
		                    on taba.AlbumID = tabb.AlbumID";

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        AlbumViewModel avm = new AlbumViewModel();
                        avm.Id = reader.GetInt32(0);
                        avm.Title = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Desp = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            avm.IsPublic = reader.GetBoolean(3);
                        if (!reader.IsDBNull(4))
                            avm.AccessCode = reader.GetString(4);
                        if (!reader.IsDBNull(5))
                            avm.CreatedAt = reader.GetDateTime(5);
                        if (!reader.IsDBNull(6))
                            avm.CreatedBy = reader.GetString(6);
                        if (!reader.IsDBNull(7))
                            avm.PhotoCount = (Int32)reader.GetInt32(7);
                        if (!reader.IsDBNull(8))
                            avm.FirstPhotoThumnailUrl = reader.GetString(8);

                        listVm.Add(avm);
                    }
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

            return listVm;
        }

        // GET api/album/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            AlbumWithPhotoViewModel avm = null;

            try
            {
                String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                        where [AlbumID] = " + id.ToString();

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    avm = new AlbumWithPhotoViewModel();
                    while (reader.Read())
                    {
                        avm.Id = reader.GetInt32(0);
                        avm.Title = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            avm.Desp = reader.GetString(2);
                        if (!reader.IsDBNull(3))
                            avm.CreatedBy = reader.GetString(3);
                        if (!reader.IsDBNull(4))
                            avm.CreatedAt = reader.GetDateTime(4);
                        if (!reader.IsDBNull(5))
                            avm.IsPublic = reader.GetBoolean(5);
                        if (!reader.IsDBNull(6))
                            avm.AccessCode = reader.GetString(6);
                    }
                    reader.Dispose();
                    cmd.Dispose();
                    reader = null;
                    cmd = null;

                    queryString = @"select tabb.[PhotoID]
                       ,tabb.[Title]
                       ,tabb.[Desp]
                       ,tabb.[UploadedAt]
                       ,tabb.[UploadedBy]
                       ,tabb.[OrgFileName]
                       ,tabb.[PhotoUrl]
                       ,tabb.[PhotoThumbUrl]
                       ,tabb.[IsOrgThumb]
                       ,tabb.[ThumbCreatedBy]
                       ,tabb.[CameraMaker]
                       ,tabb.[CameraModel]
                       ,tabb.[LensModel]
                       ,tabb.[AVNumber]
                       ,tabb.[ShutterSpeed]
                       ,tabb.[ISONumber]
                       ,tabb.[IsPublic]
                       ,tabb.[EXIFInfo]
	                 from dbo.AlbumPhoto as taba
	                left outer join dbo.Photo as tabb
		                on taba.PhotoID = tabb.PhotoID
	                where taba.AlbumID = " + id.ToString();

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            PhotoViewModelBase pvm = new PhotoViewModelBase();
                            //cmd.Parameters.AddWithValue("@PhotoID", nid.ToString("N"));   // 1
                            pvm.PhotoId = reader.GetString(0);
                            //cmd.Parameters.AddWithValue("@Title", nid.ToString("N"));     // 2
                            pvm.Title = reader.GetString(1);
                            //cmd.Parameters.AddWithValue("@Desp", nid.ToString("N"));      // 3
                            if (!reader.IsDBNull(2))
                                pvm.Desp = reader.GetString(2);
                            //cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);     // 4
                            if (!reader.IsDBNull(3))
                                pvm.UploadedTime = reader.GetDateTime(3);
                            //cmd.Parameters.AddWithValue("@UploadedBy", "Tester");         // 5
                            //cmd.Parameters.AddWithValue("@OrgFileName", rst.OrgFileName); // 6
                            if (!reader.IsDBNull(5))
                                pvm.OrgFileName = reader.GetString(5);
                            //cmd.Parameters.AddWithValue("@PhotoUrl", rst.FileUrl);        // 7
                            pvm.FileUrl = reader.GetString(6); // 7 - 1
                            //cmd.Parameters.AddWithValue("@PhotoThumbUrl", rst.ThumbnailFileUrl); // 8
                            if (!reader.IsDBNull(7)) // 8 - 1
                                pvm.ThumbnailFileUrl = reader.GetString(7);

                            avm.PhotoList.Add(pvm);
                        }
                    }
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

            if (avm != null)
                return new ObjectResult(avm);

            return NotFound();
        }
    }
}
