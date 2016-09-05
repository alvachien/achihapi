using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {
        // GET: api/album
        [HttpGet]
        public IEnumerable<AlbumViewModel> Get([FromQuery] String photoid = null)
        {
            List<AlbumViewModel> listVm = new List<AlbumViewModel>();
#if DEBUG
            SqlConnection conn = new SqlConnection(Startup.DebugConnectionString);
#else
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
#endif
            String queryString = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                if (usrObj == null)
                {
                    // Anonymous user
                    if (String.IsNullOrEmpty(photoid))
                    {
                        queryString = @"With albumfirstphoto as (select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                    join dbo.Photo as tabc
	                    on tabb.PhotoID = tabc.PhotoID
	                    group by tabb.AlbumID)
                        select taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                        tabb.PhotoCount, tabb.ThumbUrl
	                    from dbo.Album as taba
	                    left outer join albumfirstphoto as tabb
		                    on taba.AlbumID = tabb.AlbumID
                        where taba.IsPublic = 1";
                    }
                    else
                    {
                        queryString = @"With albumfirstphoto as (
	                        select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                        join dbo.Photo as tabc
	                        on tabb.PhotoID = tabc.PhotoID
	                        group by tabb.AlbumID)
                            select taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        from dbo.AlbumPhoto as tabc
	                        inner join dbo.Album as taba
		                        on tabc.AlbumID = taba.AlbumID
                                and taba.IsPublic = 1
	                        left outer join albumfirstphoto as tabb
		                        on taba.AlbumID = tabb.AlbumID
                            where tabc.PhotoID = N'";
                        queryString += photoid;
                        queryString += @"'";
                    }
                }
                else
                {
                    // Signed in user
                    if (String.IsNullOrEmpty(photoid))
                    {
                        queryString = @"With albumfirstphoto as (select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                    join dbo.Photo as tabc
	                    on tabb.PhotoID = tabc.PhotoID
	                    group by tabb.AlbumID)
                        select taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                        tabb.PhotoCount, tabb.ThumbUrl
	                    from dbo.Album as taba
	                    left outer join albumfirstphoto as tabb
		                    on taba.AlbumID = tabb.AlbumID
                        where taba.IsPublic = 1 or (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + "')";
                    }
                    else
                    {
                        queryString = @"With albumfirstphoto as (
	                        select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                        join dbo.Photo as tabc
	                        on tabb.PhotoID = tabc.PhotoID
	                        group by tabb.AlbumID)
                            select taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        from dbo.AlbumPhoto as tabc
	                        inner join dbo.Album as taba
		                        on tabc.AlbumID = taba.AlbumID
                                on taba.IsPublic = 1 or (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + "') " 
                                + 
                                @" left outer join albumfirstphoto as tabb
		                        on taba.AlbumID = tabb.AlbumID
                            where tabc.PhotoID = N'";
                        queryString += photoid;
                        queryString += @"'";
                    }
                }

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
                        {
                            // Cannot just release the AccessCode
                            //avm.AccessCode = reader.GetString(4);
                            if (!String.IsNullOrEmpty(reader.GetString(4)))
                                avm.AccessCode = "1";
                        }
                        if (!reader.IsDBNull(5))
                            avm.CreatedAt = reader.GetDateTime(5);
                        if (!reader.IsDBNull(6))
                            avm.CreatedBy = reader.GetString(6);
                        if (!reader.IsDBNull(7))
                            avm.PhotoCount = (Int32)reader.GetInt32(7);
                        if (!reader.IsDBNull(8))
                        {
                            avm.FirstPhotoThumnailUrl = reader.GetString(8);

                            if (!String.IsNullOrEmpty(avm.AccessCode))
                                avm.FirstPhotoThumnailUrl = String.Empty;
                        }
                            

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
        public IActionResult Get(int id, [FromQuery] String accessCode = null)
        {
#if DEBUG
            SqlConnection conn = new SqlConnection(Startup.DebugConnectionString);
#else
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
#endif
            AlbumWithPhotoViewModel avm = null;

            var usrObj = User.FindFirst(c => c.Type == "sub");
            String queryString = "";

            try
            {
                queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + id.ToString();

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); // Only one record!

                    String strAlbumAC = String.Empty;
                    String strCreatedBy = String.Empty;
                    Boolean bIsPublic = false;
                    if (!reader.IsDBNull(3))
                        strCreatedBy = reader.GetString(3);
                    if (!reader.IsDBNull(5))
                        bIsPublic = reader.GetBoolean(5);
                    if (!reader.IsDBNull(6))
                        strAlbumAC = reader.GetString(6);

                    if (usrObj == null)
                    {
                        // Anonymouse user
                        if (!bIsPublic)
                        {
                            return Unauthorized();
                        }

                        if (!String.IsNullOrEmpty(strAlbumAC))
                        {
                            if (String.IsNullOrEmpty(accessCode))
                            {
                                return Unauthorized();
                            }
                            else
                            {
                                if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                {
                                    return Unauthorized();
                                }
                            }
                        }
                    }
                    else
                    {
                        // Signed-in user
                        var scopeStr = User.FindFirst(c => c.Type == "GalleryNonPublicAlbumRead").Value;
                        var usrName = User.FindFirst(c => c.Type == "sub").Value;

                        if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                        {
                            if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                            {
                                // Not the album creator then needs the access code
                                if (bIsPublic)
                                {
                                    if (!String.IsNullOrEmpty(strAlbumAC))
                                    {
                                        if (String.IsNullOrEmpty(accessCode))
                                        {
                                            return Unauthorized();
                                        }
                                        else
                                        {
                                            if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                            {
                                                return Unauthorized();
                                            }
                                            else
                                            {
                                                // Access code accepted, do nothing
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Non public album, current user has no authority to view it.
                                    return Unauthorized();
                                }
                            }
                            else
                            {
                                // Creator of album, no need to access code at all
                            }
                        }
                        else if (String.CompareOrdinal(scopeStr, "All") == 0)
                        {
                            // Do nothing~
                        }
                        else
                        {
                            // Shall never happened!
                            return BadRequest();
                        }
                    }

                    avm = new AlbumWithPhotoViewModel();
                    avm.Id = reader.GetInt32(0);
                    avm.Title = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        avm.Desp = reader.GetString(2);
                    avm.CreatedBy = strCreatedBy;
                    if (!reader.IsDBNull(4))
                        avm.CreatedAt = reader.GetDateTime(4);
                    avm.IsPublic = bIsPublic;
                    avm.AccessCode = strAlbumAC;

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
                            PhotoViewModel pvm = new PhotoViewModel();
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

                            if (!reader.IsDBNull(16))
                                pvm.IsPublic = reader.GetBoolean(16);
                            if (!reader.IsDBNull(17))
                                pvm.ExifTags = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExifTagItem>>(reader.GetString(17));

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

        // POST api/album
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                vm.IsPublic = true;
                // Check existence
                //Boolean bExists = _dbContext.KnowledgeType.Any(x => x.Id == vm.ID);
                //if (bExists)
                //{
                //    return BadRequest("ID exists already");
                //}
            }
            else
            {
                return BadRequest();
            }

            // Create it into DB
            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
#if DEBUG
                using (SqlConnection conn = new SqlConnection(Startup.DebugConnectionString))
#else
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
#endif
                {
                    String cmdText = @"INSERT INTO [dbo].[Album]
                               ([Title]
                               ,[Desp]
                               ,[CreatedBy]
                               ,[CreateAt]
                               ,[IsPublic]
                               ,[AccessCode])
                         VALUES
                               (@Title
                               ,@Desp
                               ,@CreatedBy
                               ,@CreatedAt
                               ,@IsPublic
                               ,@AccessCode
                                )";
                    await conn.OpenAsync();

                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", String.IsNullOrEmpty(vm.Desp) ? String.Empty : vm.Desp);
                    cmd.Parameters.AddWithValue("@CreatedBy", usrName);
                    cmd.Parameters.AddWithValue("@CreatedAt", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    cmd.Parameters.AddWithValue("@AccessCode", String.IsNullOrEmpty(vm.AccessCode) ? String.Empty : vm.AccessCode);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                return Json(false);
            }

            return Json(true);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] AlbumViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (vm.Title != null)
                vm.Title = vm.Title.Trim();
            if (String.IsNullOrEmpty(vm.Title))
            {
                return BadRequest("Title is a must!");
            }
            
            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;

#if DEBUG
                using (SqlConnection conn = new SqlConnection(Startup.DebugConnectionString))
#else
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
#endif
                {
                    String cmdText = String.Empty;

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + vm.Id.ToString() + " FOR UPDATE ";

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
                        if (!reader.IsDBNull(3))
                            strCreatedBy = reader.GetString(3);

                        if (String.CompareOrdinal(scopeStr, "All") == 0)
                        {
                            // Do nothing
                        }
                        else if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                        {
                            if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                            {
                                return Unauthorized();
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    cmd.Dispose();
                    cmd = null;

                    cmdText = @"UPDATE [Album]
                            SET [Title] = @Title
                                ,[Desp] = @Desp
                                ,[IsPublic] = @IsPublic
                                ,[AccessCode] = @AccessCode
                            WHERE [AlbumID] = @Id
                        ";
                    cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@Id", vm.Id);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    if (String.IsNullOrEmpty(vm.Desp))
                        cmd.Parameters.AddWithValue("@Desp", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Desp", vm.Desp);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    if (vm.AccessCode == null)
                        cmd.Parameters.AddWithValue("@AccessCode", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@AccessCode", vm.AccessCode);

                    await cmd.ExecuteNonQueryAsync();
                    return new ObjectResult(true);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
            }

            return new ObjectResult(false);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(Int32 nID)
        {
            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumDelete").Value;

#if DEBUG
                using (SqlConnection conn = new SqlConnection(Startup.DebugConnectionString))
#else
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
#endif
                {
                    String cmdText = String.Empty;

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + nID.ToString() + " FOR UPDATE ";

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
                        if (!reader.IsDBNull(3))
                            strCreatedBy = reader.GetString(3);

                        if (String.CompareOrdinal(scopeStr, "All") == 0)
                        {
                            // Do nothing
                        }
                        else if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                        {
                            if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                            {
                                return Unauthorized();
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    cmd.Dispose();
                    cmd = null;

                    cmdText = @"DELETE FROM [Album] WHERE [AlbumID] = " + nID.ToString();
                    cmd = new SqlCommand(cmdText, conn);

                    await cmd.ExecuteNonQueryAsync();
                    return new ObjectResult(true);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                
            }
            finally
            {
            }

            return new ObjectResult(false);
        }
    }

    [Route("api/albumphotobyalbum")]
    public class AlbumPhotoByAlbumController : Controller
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumPhotoByAlbumViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
            }
            else
            {
                return BadRequest();
            }

            // Create it into DB
            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;

            try
            {
#if DEBUG
                using (SqlConnection conn = new SqlConnection(Startup.DebugConnectionString))
#else
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
#endif
                {
                    await conn.OpenAsync();

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + vm.AlbumID.ToString();

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
                        if (!reader.IsDBNull(3))
                            strCreatedBy = reader.GetString(3);

                        if (String.CompareOrdinal(scopeStr, "All") == 0)
                        {
                            // Do nothing
                        }
                        else if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                        {
                            if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                            {
                                return Unauthorized();
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    List<String> listCmds = new List<string>();
                    // Delete the records from album                    
                    String cmdText = @"DELETE FROM [dbo].[AlbumPhoto] WHERE [AlbumID] = " + vm.AlbumID.ToString();
                    listCmds.Add(cmdText);

                    foreach (String pid in vm.PhotoIDList)
                    {
                        cmdText = @"INSERT INTO [dbo].[AlbumPhoto]
                               ([AlbumID]
                               ,[PhotoID])
                             VALUES(" + vm.AlbumID.ToString()
                             + @", N'" + pid
                             + @"')";
                        listCmds.Add(cmdText);
                    }
                    String allQueries = String.Join(";", listCmds);

                    cmd = new SqlCommand(allQueries, conn);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                return Json(false);
            }

            return Json(true);
        }
    }

    [Route("api/albumphotobyphoto")]
    public class AlbumPhotoByPhotoController : Controller
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumPhotoByPhotoViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
            }
            else
            {
                return BadRequest();
            }

            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;
            try
            {
#if DEBUG
                using (SqlConnection conn = new SqlConnection(Startup.DebugConnectionString))
#else
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
#endif
                {
                    await conn.OpenAsync();

                    String albumList = String.Join(",", vm.AlbumIDList);

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] IN ( " + albumList + " )";

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        String strCreatedBy = String.Empty;
                        while(reader.Read())
                        {
                            if (!reader.IsDBNull(3))
                                strCreatedBy = reader.GetString(3);

                            if (String.CompareOrdinal(scopeStr, "All") == 0)
                            {
                                // Do nothing
                            }
                            else if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                            {
                                if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                                {
                                    return Unauthorized();
                                }
                                else
                                {
                                    // Do nothing
                                }
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    List<String> listCmds = new List<string>();
                    // Delete the records from album                    
                    String cmdText = @"DELETE FROM [dbo].[AlbumPhoto] WHERE [PhotoID] = N'" + vm.PhotoID + "'";
                    listCmds.Add(cmdText);

                    foreach (Int32 aid in vm.AlbumIDList)
                    {
                        cmdText = @"INSERT INTO [dbo].[AlbumPhoto]
                               ([AlbumID]
                               ,[PhotoID])
                         VALUES(" + aid.ToString()
                        + @", N'" + vm.PhotoID
                         + @"')";
                        listCmds.Add(cmdText);
                    }
                    String allQueries = String.Join(";", listCmds);

                    cmd = new SqlCommand(allQueries, conn);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                return Json(false);
            }

            return Json(true);
        }
    }
}
