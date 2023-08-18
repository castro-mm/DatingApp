using System;
using API.Data;
using Microsoft.AspNetCore.Mvc;

namespace API;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    public BaseApiController() 
    {
    }
}
