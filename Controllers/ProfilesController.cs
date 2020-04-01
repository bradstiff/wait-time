﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaitTime.Components;
using WaitTime.Entities;
using WaitTime.Models;

namespace wait_time.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WaitTimeContext _context;

        private string UserId => User.Claims.First(c => c.Type == "user_id").Value;

        public ProfilesController(WaitTimeContext context, IHttpContextAccessor httpContextAccessor, ILogger<ProfilesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpGet]
        [Route("{userId}")]
        //[Authorize]
        [ProducesResponseType(typeof(ProfileResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProfile(string userId)
        {
            //todo: privacy?
            try
            {
                var user = await _context.Users.SingleAsync(a => a.UserId == userId);
                var response = new ProfileResponseModel
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Gender = user.Gender,
                    City = user.City,
                    Region = user.Region,
                    Country = user.Country,
                    DefaultActivityType = ((ActivityTypeEnum)(user.DefaultActivityTypeId ?? (byte)ActivityTypeEnum.Ski)).ToString(),
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error fetching profile", e);
                _logger.LogError(e, "Error fetching profile");
                return ErrorResponse.AsStatusCodeResult(HttpStatusCode.InternalServerError, "Error fetching profile");
            }
        }

        [HttpPut]
        [ValidateModel]
        [Route("")]
        //[Authorize]
        [ProducesResponseType(typeof(SuccessResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SaveProfile([FromBody] ProfileSaveRequestModel model)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(a => a.UserId == UserId);
                if (user == null)
                {
                    user = new AppUser { UserId = this.UserId };
                    _context.Users.Add(user);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Gender = model.Gender;
                user.City = model.City;
                user.Region = model.Region;
                user.Country = model.Country;
                user.DefaultActivityTypeId = (byte)Enum.Parse<ActivityTypeEnum>(model.DefaultActivityType ?? "Ski", true);

                await _context.SaveChangesAsync();
                return Ok(new SuccessResponse());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving profile", e);
                _logger.LogError(e, "Error saving profile");
                return ErrorResponse.AsStatusCodeResult(HttpStatusCode.InternalServerError, "Error saving profile");
            }
        }
    }
}