using System;
using API.Data;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API;

/// <summary>
/// 
/// </summary>
[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    public BaseApiController() 
    {
    }
}
