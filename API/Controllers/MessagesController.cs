using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IMessageRepository messageRepository, 
    IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var username = User.GetUsername();

        if(username == createMessageDTO.RecipientUsername.ToLower())
            return BadRequest("You can't message yourself");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await  userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

        if(recipient == null || sender == null)
            return BadRequest("Cannot send messages at this time");
        
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.Content
        };

        messageRepository.AddMessage(message);

        if(await messageRepository.SaveAllAsync())
            return Ok(mapper.Map<MessageDTO>(message));
        
        return BadRequest("Failed to save messages");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser(
        [FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var message = await messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(message);

        return message;
    }

    [HttpGet("thread/{username}")] 
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await messageRepository.GetMessageThread(currentUsername,username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username =  User.GetUsername();

        var message = await messageRepository.GetMessage(id);

        if(message == null) return BadRequest("Cannot delete this message");

        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Forbid();

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message is {SenderDeleted: true, RecipientDeleted: true}) {
            messageRepository.DeleteMessage(message);
        }

        if(await messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deteleting the messgae");
    }

}
