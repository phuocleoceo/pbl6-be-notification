using static Monolithic.Constants.PermissionPolicy;
using Microsoft.AspNetCore.Authorization;
using Monolithic.Services.Interface;
using Monolithic.Models.ReqParams;
using Monolithic.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Monolithic.Models.DTO;
using Monolithic.Constants;

namespace Monolithic.Controllers;

public class NotificationController : BaseController
{
    private INotificationService _notyService;
    public NotificationController(INotificationService notyService)
    {
        _notyService = notyService;
    }

    [HttpGet]
    [Authorize(Roles = NotificationPermission.ViewAll)]
    public async Task<BaseResponse<PagedList<NotificationDTO>>> GetPersonalNotfication(
                        [FromQuery] NotificationParams notificationParams)
    {
        ReqUser reqUser = HttpContext.Items["reqUser"] as ReqUser;
        var notifications = await _notyService.GetNotifications(reqUser, notificationParams);
        return new BaseResponse<PagedList<NotificationDTO>>(notifications);
    }

    [HttpGet("unread/count")]
    [Authorize(Roles = NotificationPermission.ViewAll)]
    public async Task<BaseResponse<CountUnreadNotificationDTO>> CountUnreadNotification()
    {
        ReqUser reqUser = HttpContext.Items["reqUser"] as ReqUser;
        var countUnread = await _notyService.CountUnreadNotification(reqUser.Id);
        return new BaseResponse<CountUnreadNotificationDTO>(countUnread);
    }

    [HttpPut("has-read/{id}")]
    [Authorize(Roles = NotificationPermission.Update)]
    public async Task<BaseResponse<bool>> SetNotificationHasRead(int id)
    {
        ReqUser reqUser = HttpContext.Items["reqUser"] as ReqUser;
        var notyUpdated = await _notyService.SetNotyHasRead(reqUser.Id, id);
        if (notyUpdated)
            return new BaseResponse<bool>(notyUpdated, HttpCode.NO_CONTENT);
        else
            return new BaseResponse<bool>(notyUpdated, HttpCode.BAD_REQUEST, "", false);
    }

    [HttpPut("mark-all-read")]
    [Authorize(Roles = NotificationPermission.Update)]
    public async Task<BaseResponse<bool>> SetNotificationAllRead()
    {
        ReqUser reqUser = HttpContext.Items["reqUser"] as ReqUser;
        var notyUpdated = await _notyService.SetAllNotyHasRead(reqUser.Id);
        if (notyUpdated)
            return new BaseResponse<bool>(notyUpdated, HttpCode.NO_CONTENT);
        else
            return new BaseResponse<bool>(notyUpdated, HttpCode.BAD_REQUEST, "", false);
    }

    [HttpPost("push/review")]
    // [Authorize]
    public async Task<BaseResponse<bool>> PushReviewNotification(ReviewNotificationDTO reviewDTO)
    {
        var notiCreated = await _notyService.CreateReviewOnPostNoty(reviewDTO);
        if (notiCreated)
            return new BaseResponse<bool>(notiCreated, HttpCode.CREATED);
        else
            return new BaseResponse<bool>(notiCreated, HttpCode.BAD_REQUEST, "", false);
    }

    [HttpPost("push/booking")]
    // [Authorize]
    public async Task<BaseResponse<bool>> PushBookingNotification(BookingNotificationDTO bookingDTO)
    {
        var notiCreated = await _notyService.CreateBookingOnPostNoty(bookingDTO);
        if (notiCreated)
            return new BaseResponse<bool>(notiCreated, HttpCode.CREATED);
        else
            return new BaseResponse<bool>(notiCreated, HttpCode.BAD_REQUEST, "", false);
    }

    [HttpPost("push/approve-meeting")]
    // [Authorize]
    public async Task<BaseResponse<bool>> PushApproveMeetingNotification(ApproveMeetingNotificationDTO approveDTO)
    {
        var notiCreated = await _notyService.CreateApproveMeetingNoty(approveDTO);
        if (notiCreated)
            return new BaseResponse<bool>(notiCreated, HttpCode.CREATED);
        else
            return new BaseResponse<bool>(notiCreated, HttpCode.BAD_REQUEST, "", false);
    }

    [HttpPost("push/confirm-met")]
    // [Authorize]
    public async Task<BaseResponse<bool>> PushConfirmMetNotification(ConfirmMetNotificationDTO confirmDTO)
    {
        var notiCreated = await _notyService.CreateConfirmMetNoty(confirmDTO);
        if (notiCreated)
            return new BaseResponse<bool>(notiCreated, HttpCode.CREATED);
        else
            return new BaseResponse<bool>(notiCreated, HttpCode.BAD_REQUEST, "", false);
    }
}