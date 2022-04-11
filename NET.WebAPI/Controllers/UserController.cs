using Contracts;
using Entities.Models;
using Entities.Models.Database;
using Microsoft.AspNetCore.Mvc;
using NET.WebAPI.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NET.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepositoryWrapper _repository;
        private readonly EncryptionService _encryption;
        public UserController(IRepositoryWrapper repository, EncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

		[HttpGet]
		public async Task<IActionResult> GetUsers([FromQuery] UserParameters parameters)
		{;
            PagedList<ShapedEntity> data = await _repository.User.GetPagedAsync(
                orderBy: parameters.OrderBy,
                page: parameters.PageNumber,
                pageSize: parameters.PageSize,
                onlyFields: parameters.Fields,
                includeProperties: parameters.IncludeEntities,
                searchTerm: parameters.SearchTerm,
                includeSearch: parameters.IncludeSearch);

            Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(data.MetaData));
            List<Entity> shapedData = data.Select(o => o.Entity).ToList();
            return Ok(shapedData);
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            user.Password = _encryption.EncryptString(user.Password);

            await _repository.User.InsertAsync(user);
            _repository.Save();

            return Ok(new { id = user.ID });
        }
	}
}
