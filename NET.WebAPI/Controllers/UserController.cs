using Contracts;
using Entities.Models;
using Entities.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace NET.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepositoryWrapper _repository;
        public UserController(
            IRepositoryWrapper repository)
        {
            _repository = repository;
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
            var shapedData = data.Select(o => o.Entity).ToList();
            return Ok(shapedData);
        }
	}
}
