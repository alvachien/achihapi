using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using achihapi.Models;
using achihapi.ViewModels;
using System.Data.Common;
using System.Data.SqlClient;

namespace achihapi.Controllers
{
    [Route("api/[controller]")]
    public class KnowledgeController : Controller
    {
        public KnowledgeController(achihdbContext context)
        {
            _dbContext = context;
        }

        private achihdbContext _dbContext = null;

        // GET: api/knowledge
        [HttpGet]
        public IEnumerable<KnowledgeViewModel> Get([FromQuery] Int16? typeid = null, 
            [FromQuery] DateTime? datefrom = null, [FromQuery] DateTime? dateto = null, [FromQuery] Int32? maxhit=null)
        {
            List<KnowledgeViewModel> vms = new List<KnowledgeViewModel>();

#if USE_EFCORE // In current EFCore 1.0, there alwasy returns duplicated records and I don't know why!
            var db1 = from kl in _dbContext.Knowledge
                      join kt in _dbContext.KnowledgeType
                        on kl.ContentType equals kt.Id
                      select new { kl.Id, kl.ContentType, kl.Title, kl.Content, kl.Tags, kl.CreatedAt, kl.ModifiedAt, ContentTypeName = kt.Name, ContentTypeParent = kt.ParentId };

            foreach (var db in db1)
            {
                KnowledgeViewModel vm = new KnowledgeViewModel();
                vm.ID = db.Id;
                vm.TypeID = db.ContentType;
                vm.Title = db.Title;
                vm.Content = db.Content;
                vm.Tags = db.Tags == null ? null : db.Tags.Trim();
                vm.CreatedAt = db.CreatedAt;
                vm.ModifiedAt = db.ModifiedAt;

                vms.Add(vm);
            }
#endif
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            List<String> arWhereStr = new List<string>();
            String strTop = "";
            String strWhere = "";
            if (typeid.HasValue)
            {
                arWhereStr.Add(" taba.ContentType = " + typeid.Value.ToString());
            }
            if (datefrom.HasValue)
            {
                arWhereStr.Add(" taba.ModifiedAt >= " + datefrom.Value.ToString());
            }
            if (dateto.HasValue)
            {
                arWhereStr.Add(" taba.ModifiedAt <= " + dateto.Value.ToString());
            }
            if (maxhit.HasValue && maxhit.Value > 0)
            {
                strTop = " TOP " + maxhit.Value.ToString();
            }
            if (arWhereStr.Count > 0)
            {
                for(Int32 i = 0; i < arWhereStr.Count; i ++)
                {
                    if (i == 0)
                        strWhere = arWhereStr[0];
                    else
                    {
                        strWhere += " AND " + arWhereStr[i];
                    }
                }
            }

            String queryString = "select " + strTop + @" taba.ID, taba.ContentType, taba.Title, taba.Content, taba.Tags, taba.CreatedAt, taba.ModifiedAt, tabb.ParentID as TypeParentID, tabb.Name as TypeName from dbo.Knowledge as taba
	                        left outer join dbo.KnowledgeType as tabb
	                        on taba.ContentType = tabb.Id";
            if (arWhereStr.Count > 0)
                queryString += " WHERE " + strWhere;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    KnowledgeViewModel vm = new KnowledgeViewModel();
                    vm.ID = reader.GetInt32(0);
                    if (reader.IsDBNull(1))
                        vm.TypeID = null;
                    else
                        vm.TypeID = reader.GetInt16(1);
                    vm.Title = reader.GetString(2).Trim();
                    vm.Content = reader.GetString(3).Trim().Substring(0, 30);
                    if (reader.IsDBNull(4))
                        vm.Tags = null;
                    else
                        vm.Tags = reader.GetString(4).Trim();
                    vm.CreatedAt = reader.GetDateTime(5);
                    vm.ModifiedAt = reader.GetDateTime(6);

                    // Type parent
                    //if (reader.IsDBNull(7)) { }
                    //else { reader.GetInt32(7)}
                    // Type name 
                    //reader.IsDBNull(8) { } else reader.GetString(8)

                    vms.Add(vm);
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

            return vms;
        }

        // GET api/knowledge/5
        [HttpGet("{id}", Name = "GetKnowledge")]
        public IActionResult Get(int id)
        {
            KnowledgeViewModel vm = new KnowledgeViewModel();

#if USE_EFCORE // In current EFCore 1.0, there alwasy returns duplicated records and I don't know why!
            var db1 = from kl in _dbContext.Knowledge
                      join kt in _dbContext.KnowledgeType
                        on kl.ContentType equals kt.Id
                      where kl.ContentType != null
                            && kl.Id == id
                      select new { kl.Id, kl.ContentType, kl.Title, kl.Content, kl.Tags, kl.CreatedAt, kl.ModifiedAt, ContentTypeName = kt.Name, ContentTypeParent = kt.ParentId };
            var db = db1.FirstOrDefault();
            if (db == null)
            {
                var db2 = from kl in _dbContext.Knowledge
                          where kl.ContentType != null 
                            && kl.Id == id
                          select new { kl.Id, kl.ContentType, kl.Title, kl.Content, kl.Tags, kl.CreatedAt, kl.ModifiedAt, ContentTypeName = String.Empty, ContentTypeParent = 0 };

                var odb = db2.FirstOrDefault();
                if (odb == null)
                    return NotFound();

                vm.ID = odb.Id;
                vm.TypeID = odb.ContentType;
                vm.Title = odb.Title;
                vm.Content = odb.Content;
                vm.Tags = odb.Tags == null ? null : odb.Tags.Trim();
                vm.CreatedAt = odb.CreatedAt;
                vm.ModifiedAt = odb.ModifiedAt;

            }
            else
            {
                vm.ID = db.Id;
                vm.TypeID = db.ContentType;
                vm.Title = db.Title;
                vm.Content = db.Content;
                vm.Tags = db.Tags == null ? null : db.Tags.Trim();
                vm.CreatedAt = db.CreatedAt;
                vm.ModifiedAt = db.ModifiedAt;
            }
#endif

            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = @"select taba.ID, taba.ContentType, taba.Title, taba.Content, taba.Tags, taba.CreatedAt, taba.ModifiedAt, tabb.ParentID as TypeParentID, tabb.Name as TypeName from dbo.Knowledge as taba
	                        left outer join dbo.KnowledgeType as tabb
	                        on taba.ContentType = tabb.Id where taba.[ID] = " + id.ToString();
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        vm.ID = reader.GetInt32(0);
                        if (reader.IsDBNull(1))
                            vm.TypeID = null;
                        else
                            vm.TypeID = reader.GetInt16(1);
                        vm.Title = reader.GetString(2).Trim();
                        vm.Content = reader.GetString(3).Trim();
                        if (reader.IsDBNull(4))
                            vm.Tags = null;
                        else
                            vm.Tags = reader.GetString(4).Trim();
                        vm.CreatedAt = reader.GetDateTime(5);
                        vm.ModifiedAt = reader.GetDateTime(6);
                    }
                }
                else
                {
                    return NotFound();
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

        // POST api/knowledge
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]KnowledgeViewModel vm)
        {
            // Create a new knowledge
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                // Do nothing here
            }
            else
            {
                return BadRequest();
            }

            // Add it into the database
            Knowledge word = new Knowledge();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //word.Id = vm.ID;
                    word.Content = vm.Content;
                    if (vm.TypeID.HasValue && vm.TypeID.Value != 0)
                    {
                        word.ContentType = (short) vm.TypeID.Value;
                    } 
                    else
                    {
                        word.ContentType = null;
                    }
                    word.Title = vm.Title;
                    word.ModifiedAt = DateTime.Now;
                    word.CreatedAt = word.ModifiedAt;
                    word.Tags = vm.Tags;
                    _dbContext.Knowledge.Add(word);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception exp)
                {
#if DEBUG
                    Console.WriteLine(exp.Message);
#endif

                    transaction.Rollback();
                    return BadRequest();
                }
            }

            return CreatedAtRoute("GetKnowledge", new { id = word.Id });
        }

        // PUT api/knowledge/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]KnowledgeViewModel vm)
        {
            var db = _dbContext.Knowledge.Single(x => x.Id == id);
            if (db == null)
            {
                return NotFound();
            }

            db.Content = vm.Content;
            if (vm.TypeID.HasValue)
            {
                db.ContentType = (short)vm.TypeID.Value;
            }
            else
            {
                db.ContentType = null;
            }
            db.ModifiedAt = DateTime.Now;
            db.CreatedAt = db.CreatedAt;
            db.Tags = vm.Tags;
            _dbContext.Knowledge.Update(db);
            _dbContext.SaveChangesAsync().Wait();

            return new ObjectResult(vm);
        }

        // DELETE api/knowledge/5
        [HttpDelete("{ids}")]
        public IActionResult Delete(string ids)
        {
            string[] stringSeparators = new string[] { "," };
            var idarr = ids.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            List<Int32> realids = new List<int>(); ;

            foreach(var idelem in idarr)
            {
                realids.Add(Int32.Parse(idelem));
            }

            var dbentries = _dbContext.Knowledge.Where(x => realids.Contains(x.Id));
            if (dbentries == null || dbentries.Count() <= 0)
            {
                return NotFound();
            }

            _dbContext.Knowledge.RemoveRange(dbentries);
            _dbContext.SaveChangesAsync().Wait();

            return Ok(ids);
        }
    }
}
