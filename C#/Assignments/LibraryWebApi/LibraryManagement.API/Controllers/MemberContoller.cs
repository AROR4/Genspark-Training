using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpPost]
        public ActionResult<Member> AddMember(MemberDTO memberDTO)
        {
            try
            {
                var addedMember = _memberService.AddMember(memberDTO);
                return CreatedAtAction(
                    nameof(GetMemberById),
                    new { id = addedMember.MemberId },
                    addedMember
                );            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<Member>> GetAllMembers()
        {
            try
            {
                var members = _memberService.GetAllMembers();
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetMemberById([FromRoute] int id)
        {
            try
            {
                var member = _memberService.GetMemberById(id);
                if (member == null)
                {
                    return NotFound();
                }
                return Ok(member);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}