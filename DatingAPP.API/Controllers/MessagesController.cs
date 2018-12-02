using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingAPP.API.Data;
using DatingAPP.API.Dtos;
using DatingAPP.API.Helpers;
using DatingAPP.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingAPP.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
	[ApiController]
	//[Route("api/users/{userId}/[controller]")]
	//[Route("api/users/[controller]")]
	[Route("api/users")]
	public class MessagesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

		// Retrieve an individual message
		[HttpGet("{userId}/[controller]/{id}", Name = "GetMesage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            // Check to see that the user token matches the userId
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Get message from repo
            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        // Get list of messages
        [HttpGet("{userId}/[controller]")]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            // Get messages from repo
            var messageFromRepo = await _repo.GetMessagesForUser(messageParams);

            // Map messages to MessageToReturnDto
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            // Return the pagination, so add it to response. (messageFromRepo is returning a PageList)
            Response.AddPagination(messageFromRepo.CurrentPage, messageFromRepo.PageSize, messageFromRepo.TotalCount, messageFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("{userId}/[controller]/thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<MessageToReturnDto>(messagesFromRepo);

            return Ok(messageThread);
        }

        // To retrieve messages from the database, users should be able to create messages
        [HttpPost("{userId}/[controller]")]

		public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = _repo.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Set messageForCreationDto SenderId to userId
            messageForCreationDto.SenderId = userId;

            // Get recipient from repo, send it as part of the request when a message sent to the user
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            // Check if recipient exist
            if (recipient == null)
                return BadRequest("Could not find user");

            // Match MessageForCreationDto to Message class
            var message = _mapper.Map<Message>(messageForCreationDto);

            // Add message to repo , not an async method because we're not querying the database
            _repo.Add(message);

            if (await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);  // Get location of message created
            }

            throw new Exception("Creating new message failed on save");
        }

        [HttpDelete("{userId}/[controller]/{id}")]      // message id
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if (messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{userId}/[controller]/{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId)
                return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }
    }
}