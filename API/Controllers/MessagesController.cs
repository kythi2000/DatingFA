﻿using API.DTOs;
using API.Entity;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUsername();

            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messageParams.PageNumber, 
                messageParams.PageSize, messages.TotalCount, messages.TotalPage));

            return Ok(messages);
        }
  

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Delete failed");
        }
    }
}
