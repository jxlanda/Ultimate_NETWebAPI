using Contracts;
using Entities.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace NET.WebAPI.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRepositoryWrapper _repository;
        public UserController(
            IRepositoryWrapper repository)
        {
            _repository = repository;
        }

		[HttpGet()]
		public async Task<IActionResult> GetUsers([FromQuery] UserParameters parameters)
		{
			var data = await _repository.Account.GetPagedAsync(
				orderBy: parameters.OrderBy,
				page: parameters.PageNumber,
				pageSize: parameters.PageSize,
				onlyFields: parameters.Fields,
				includeProperties: parameters.IncludeEntities,
				searchTerm: parameters.SearchTerm,
				includeSearch: parameters.IncludeSearch);

			var metadata = new
			{
				data.TotalCount,
				data.PageSize,
				data.CurrentPage,
				data.TotalPages,
				data.HasNext,
				data.HasPrevious,
			};

			Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
			Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
			var shapedData = data.Select(o => o.Entity).ToList();
			return Ok(shapedData);
		}
	}
}
