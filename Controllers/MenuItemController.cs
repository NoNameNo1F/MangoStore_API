using System.Net;
using MangoStore_API.Data;
using MangoStore_API.Models;
using Microsoft.AspNetCore.Http;
using MangoStore_API.Logging;
using Microsoft.AspNetCore.Mvc;
using MangoStore_API.Repositories.IReposiitories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MangoStore_API.Models.Dtos.MenuItemDto;
using AutoMapper;
using MangoStore_API.Services.BlobService;
using MangoStore_API.Utils;
namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class MenuItemController : ControllerBase
    {
        private IMenuItemRepository _menuItemRepo;
        private readonly IBlobService _blobService;
        private readonly IMapper _mapper;
        protected ApiResponse _response;
        private ILogging _logger;
        public MenuItemController(IMenuItemRepository menuItemRepo, IBlobService blobService, IMapper mapper, ILogging logger)
        {
            _menuItemRepo = menuItemRepo;
            _blobService = blobService;
            _mapper = mapper;
            _response = new ApiResponse();
            _logger = logger;
        }

        /// <summary>
        /// Get All Menuitem
        /// </summary>
        /// <returns>List of MenuItem</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetMenuItems()
        {
            try
            {
                IEnumerable<MenuItem> items = await _menuItemRepo.GetAllAsync();
                // _response.Result = _mapper.Map<List<MenuItemDto>>(items);
                _response.Result = items;
                _response.StatusCode = HttpStatusCode.OK;

                _logger.Log("Get all MenuItems!","info");
                return Ok(_response);
            }
            catch (Exception exception)
            {
                _logger.Log($"{exception}", "error");
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    exception.ToString(),
                };
            }

            return _response;
        }

        /// <summary>
        ///  Get a specific menu item by itemId
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>MenuItem</returns>
        [HttpGet("{itemId:int}", Name = "GetMenuItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetMenuItem(int itemId)
        {
            try
            {
                if(itemId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    _logger.Log($"MenuItem id {itemId} not valid.","error");
                    return BadRequest(_response);
                }

                MenuItem menuItem = await _menuItemRepo.GetAsync(u => u.Id == itemId);

                if (menuItem == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    _logger.Log($"MenuItem id {itemId} not exists.","error");
                    return NotFound(_response);
                }

                // _response.Result = _mapper.Map<MenuItemDto>(menuItem);
                _response.Result = menuItem;
                _response.StatusCode = HttpStatusCode.OK;

                _logger.Log($"Get MenuItem id {itemId}.", "info");
                return Ok(_response);
            }
            catch (Exception exception)
            {
                _logger.Log($"{exception}", "error");
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    exception.ToString(),
                };
            }

            return _response;
        }

        /// <summary>
        ///  Create a new menu item from form
        /// </summary>
        /// <param name="menuItemDto">MenuItem</param>
        /// <returns>MenuItem</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemDto menuItemDto)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    if(menuItemDto.File == null || menuItemDto.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;

                        _logger.Log($"Not having MenuItem Image to create.", "error");
                        return BadRequest(_response);
                    }

                    bool isExists = await _menuItemRepo.GetAsync(u => u.Name.ToLower() == menuItemDto.Name.ToLower()) != null;

                    if(isExists)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;

                        _logger.Log($"MenuItem Name {menuItemDto.Name} already exists.", "error");
                        return BadRequest(_response);
                    }

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemDto.File.FileName)}";

                    //MenuItem menuItem = _mapper.Map<MenuItem>(menuItemDto);
                    MenuItem menuItem = new()
                    {
                        Name = menuItemDto.Name,
                        Price = menuItemDto.Price,
                        Category = menuItemDto.Category,
                        SpecialTag = menuItemDto.SpecialTag,
                        Description = menuItemDto.Description,
                        Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemDto.File)
                    };
                    await _menuItemRepo.CreateAsync(menuItem);

                    // _response.Result = _mapper.Map<MenuItemDto>(menuItem);
                    _response.Result = menuItem;
                    _response.StatusCode = HttpStatusCode.Created;

                    _logger.Log($"Created MenuItem is {menuItem.Id}", "info");
                    return CreatedAtRoute("GetMenuItem", new
                    {
                        itemId = menuItem.Id
                    }, _response);
                }

                else
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    _logger.Log($"MenuItem is not valid to create", "error");
                    return BadRequest(_response);
                }

            }
            catch (Exception exception)
            {
                _logger.Log($"{exception}", "error");
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    exception.ToString(),
                };
            }

            return _response;
        }
        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="itemId">id of menu item</param>
        /// <returns>Nothing</returns>
        [HttpPut("{itemId:int}", Name = "UpdateMenuItem")]
        [ProducesResponseType(204)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int itemId, [FromForm] MenuItemDto menuItemDto)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    if(menuItemDto == null || itemId != menuItemDto.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;

                        _logger.Log($"Not having MenuItem to updating", "error");
                        return BadRequest(_response);
                    }


                    //MenuItem menuItem = _mapper.Map<MenuItem>(menuItemDto);
                    MenuItem menuItem = await _menuItemRepo.GetAsync(u => u.Id == menuItemDto.Id);

                    menuItem.Name = menuItemDto.Name;
                    menuItem.Price = menuItemDto.Price;
                    menuItem.Category = menuItemDto.Category;
                    menuItem.SpecialTag = menuItemDto.SpecialTag;
                    menuItem.Description = menuItemDto.Description;
                    if(menuItemDto.File != null && menuItemDto.File.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemDto.File.FileName)}";
                        await _blobService.DeleteBlob(menuItem.Image.Split('/').Last(), SD.SD_Storage_Container);
                        menuItem.Image = await _blobService.UploadBlob(fileName, SD.SD_Storage_Container, menuItemDto.File);
                    }
                    await _menuItemRepo.UpdateAsync(menuItem);

                    _response.StatusCode = HttpStatusCode.NoContent;
                    _response.IsSuccess = true;

                    _logger.Log($"Update MenuItem Id {menuItem.Id} successfully!", "info");
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"{exception}", "error");
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    exception.ToString(),
                };
            }

            return _response;
        }

        [HttpDelete("{itemId:int}", Name = "Delete MenuItem")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int itemId)
        {
            try
            {
                if( itemId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    _logger.Log("Menu Item id: 0 not valid.", "info");
                    return BadRequest(_response);
                }
                var menuItem = await _menuItemRepo.GetAsync(u => u.Id == itemId);

                if(menuItem == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    _logger.Log($" MenuItem id {itemId} not exists","error");
                    return NotFound(_response);
                }
                await _blobService.DeleteBlob(menuItem.Image.Split('/').Last(), SD.SD_Storage_Container);

                await _menuItemRepo.RemoveAsync(menuItem);

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                _logger.Log($"Delete MenuItem Id: {itemId} successfully!", "info");
                return Ok(_response);
            }
            catch (Exception exception)
            {
                _logger.Log($"{exception}", "error");
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>()
                {
                    exception.ToString()
                };
            }

            return _response;
        }
    }
}
