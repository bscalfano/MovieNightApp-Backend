using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieNightApp.Data;
using MovieNightApp.DTOs;
using MovieNightApp.Models;
using System.Security.Claims;

namespace MovieNightApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly MovieNightAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendsController(MovieNightAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        // GET: api/Friends/stats
        [HttpGet("stats")]
        public async Task<ActionResult<FriendStatsDto>> GetFriendStats()
        {
            var userId = GetUserId();

            var friendsCount = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Accepted &&
                    (fr.SenderId == userId || fr.ReceiverId == userId))
                .CountAsync();

            var pendingRequestsCount = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Pending && fr.ReceiverId == userId)
                .CountAsync();

            return Ok(new FriendStatsDto
            {
                FriendsCount = friendsCount,
                PendingRequestsCount = pendingRequestsCount
            });
        }

        // GET: api/Friends/search?query=john
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> SearchUsers([FromQuery] string query)
        {
            var currentUserId = GetUserId();

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query cannot be empty");
            }

            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId &&
                    (u.Email!.Contains(query) ||
                     (u.FirstName != null && u.FirstName.Contains(query)) ||
                     (u.LastName != null && u.LastName.Contains(query))))
                .Take(10)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Get all friend requests involving current user and searched users
            var friendRequests = await _context.FriendRequests
                .Where(fr => (fr.SenderId == currentUserId && userIds.Contains(fr.ReceiverId)) ||
                             (fr.ReceiverId == currentUserId && userIds.Contains(fr.SenderId)))
                .ToListAsync();

            var result = users.Select(u =>
            {
                var friendRequest = friendRequests.FirstOrDefault(fr =>
                    (fr.SenderId == currentUserId && fr.ReceiverId == u.Id) ||
                    (fr.ReceiverId == currentUserId && fr.SenderId == u.Id));

                string status = "none";
                if (friendRequest != null)
                {
                    if (friendRequest.Status == FriendRequestStatus.Accepted)
                    {
                        status = "friends";
                    }
                    else if (friendRequest.Status == FriendRequestStatus.Pending)
                    {
                        status = friendRequest.SenderId == currentUserId ? "pending_sent" : "pending_received";
                    }
                }

                return new UserSearchDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    FriendshipStatus = status
                };
            });

            return Ok(result);
        }

        // GET: api/Friends
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSearchDto>>> GetFriends()
        {
            var userId = GetUserId();

            var friendRequests = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Accepted &&
                    (fr.SenderId == userId || fr.ReceiverId == userId))
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .ToListAsync();

            var friends = friendRequests.Select(fr =>
            {
                var friend = fr.SenderId == userId ? fr.Receiver : fr.Sender;
                return new UserSearchDto
                {
                    Id = friend.Id,
                    Email = friend.Email!,
                    FirstName = friend.FirstName,
                    LastName = friend.LastName,
                    ProfilePictureUrl = friend.ProfilePictureUrl,
                    FriendshipStatus = "friends"
                };
            });

            return Ok(friends);
        }

        // GET: api/Friends/requests
        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests()
        {
            var userId = GetUserId();

            var requests = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Pending && fr.ReceiverId == userId)
                .Include(fr => fr.Sender)
                .Select(fr => new FriendRequestDto
                {
                    Id = fr.Id,
                    SenderId = fr.SenderId,
                    ReceiverId = fr.ReceiverId,
                    SenderEmail = fr.Sender.Email!,
                    SenderFirstName = fr.Sender.FirstName,
                    SenderLastName = fr.Sender.LastName,
                    SenderProfilePictureUrl = fr.Sender.ProfilePictureUrl,
                    CreatedAt = fr.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // POST: api/Friends/request/{userId}
        [HttpPost("request/{userId}")]
        public async Task<IActionResult> SendFriendRequest(string userId)
        {
            var currentUserId = GetUserId();

            if (currentUserId == userId)
            {
                return BadRequest("Cannot send friend request to yourself");
            }

            var userToAdd = await _userManager.FindByIdAsync(userId);
            if (userToAdd == null)
            {
                return NotFound("User not found");
            }

            // Check for existing request in either direction
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    (fr.SenderId == currentUserId && fr.ReceiverId == userId) ||
                    (fr.SenderId == userId && fr.ReceiverId == currentUserId));

            if (existingRequest != null)
            {
                if (existingRequest.Status == FriendRequestStatus.Accepted)
                {
                    return BadRequest("Already friends");
                }
                if (existingRequest.Status == FriendRequestStatus.Pending)
                {
                    return BadRequest("Friend request already pending");
                }
            }

            var friendRequest = new FriendRequest
            {
                SenderId = currentUserId,
                ReceiverId = userId,
                Status = FriendRequestStatus.Pending
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Friends/accept/{requestId}
        [HttpPost("accept/{requestId}")]
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            var userId = GetUserId();

            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId);

            if (request == null)
            {
                return NotFound("Friend request not found");
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest("Friend request is not pending");
            }

            request.Status = FriendRequestStatus.Accepted;
            request.AcceptedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: api/Friends/reject/{requestId}
        [HttpPost("reject/{requestId}")]
        public async Task<IActionResult> RejectFriendRequest(int requestId)
        {
            var userId = GetUserId();

            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId);

            if (request == null)
            {
                return NotFound("Friend request not found");
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest("Friend request is not pending");
            }

            request.Status = FriendRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Friends/{userId}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveFriend(string userId)
        {
            var currentUserId = GetUserId();

            var friendship = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Status == FriendRequestStatus.Accepted &&
                    ((fr.SenderId == currentUserId && fr.ReceiverId == userId) ||
                     (fr.SenderId == userId && fr.ReceiverId == currentUserId)));

            if (friendship == null)
            {
                return NotFound("Friendship not found");
            }

            _context.FriendRequests.Remove(friendship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Friends/request/{userId}
        [HttpDelete("request/{userId}")]
        public async Task<IActionResult> CancelFriendRequest(string userId)
        {
            var currentUserId = GetUserId();

            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Status == FriendRequestStatus.Pending &&
                    fr.SenderId == currentUserId && fr.ReceiverId == userId);

            if (request == null)
            {
                return NotFound("Friend request not found");
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}