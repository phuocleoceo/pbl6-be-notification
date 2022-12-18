using Monolithic.Repositories.Interface;
using Monolithic.Services.Interface;
using Monolithic.Models.ReqParams;
using Monolithic.Models.Entities;
using Monolithic.Models.Common;
using Monolithic.Models.DTO;
using Monolithic.Constants;
using Monolithic.Helpers;
using AutoMapper;

namespace Monolithic.Services.Implement;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notyRepo;
    private readonly IHttpHelper<UserDTO> _userHttpHelper;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    public NotificationService(INotificationRepository notyRepo,
                               IHttpHelper<UserDTO> userHttpHelper,
                               IConfiguration configuration,
                               IMapper mapper)
    {
        _notyRepo = notyRepo;
        _userHttpHelper = userHttpHelper;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<PagedList<NotificationDTO>> GetNotifications(int userId,
                                                string token,
                                                NotificationParams notificationParams)
    {
        PagedList<NotificationEntity> notyEntityList = await _notyRepo.GetNotifications(userId, notificationParams);
        var notyDTOList = notyEntityList.Records
                                .Select(b => mappingNoti(b, token))
                                .Select(b => b.Result).ToList();
        return new PagedList<NotificationDTO>(notyDTOList, notyEntityList.TotalRecords);
    }

    private async Task<NotificationDTO> mappingNoti(NotificationEntity noti, string token)
    {
        var notiDTO = _mapper.Map<NotificationDTO>(noti);
        var monolithicService = _configuration["MonolithicService"];
        string userUrl = $"{monolithicService}/api/user/anonymous?userId={noti.OriginUserId}";

        var userDTO = await _userHttpHelper.GetAsync(userUrl, token);
        notiDTO.OriginUserName = userDTO.DisplayName;
        notiDTO.OriginUserEmail = userDTO.UserAccountEmail;
        notiDTO.OriginUserAvatar = userDTO.Avatar;
        return notiDTO;
    }

    public async Task<CountUnreadNotificationDTO> CountUnreadNotification(int userId)
    {
        var countUnread = await _notyRepo.CountUnreadNotification(userId);
        return new CountUnreadNotificationDTO()
        {
            AllTime = countUnread.Item1,
            Today = countUnread.Item2,
        };
    }

    public async Task<bool> SetNotyHasRead(int userId, int notyId)
    {
        NotificationEntity notyEntity = await _notyRepo.GetById(notyId);
        if (notyEntity.TargetUserId != userId)
            throw new BaseException(HttpCode.BAD_REQUEST, "Cannot set has read for other user notification");
        notyEntity.HasRead = true;
        return await _notyRepo.UpdateNotification(notyEntity);
    }

    public async Task<bool> SetAllNotyHasRead(int userId)
    {
        return await _notyRepo.SetAllNotyHasRead(userId);
    }

    // public async Task<bool> CreateReviewOnPostNoty(ReviewNotificationDTO createDTO)
    // {
    //     PostEntity post = await _postRepo.GetPostById(createDTO.PostId);
    //     if (post == null)
    //         throw new BaseException(HttpCode.BAD_REQUEST, "Invalid review on post");

    //     NotificationEntity notyEntity = new NotificationEntity()
    //     {
    //         Code = NotificationCode.REVIEW__HAS_REVIEW_ON_POST,
    //         ExtraData = JsonConvert.SerializeObject(new
    //         {
    //             PostId = createDTO.PostId,
    //             PostTitle = post.Title,
    //             ReviewId = createDTO.ReviewId,
    //             ReviewContent = createDTO.ReviewContent,
    //             ReviewRating = createDTO.ReviewRating,
    //         }),
    //         HasRead = false,
    //         OriginUserId = createDTO.OriginUserId,
    //         TargetUserId = post.HostId,
    //     };
    //     return await _notyRepo.CreateNotification(notyEntity);
    // }

    // public async Task<bool> CreateBookingOnPostNoty(BookingNotificationDTO createDTO)
    // {
    //     PostEntity post = await _postRepo.GetPostById(createDTO.PostId);
    //     if (post == null)
    //         throw new BaseException(HttpCode.BAD_REQUEST, "Invalid booking on post");

    //     NotificationEntity notyEntity = new NotificationEntity()
    //     {
    //         Code = NotificationCode.BOOKING__HAS_BOOKING_ON_POST,
    //         ExtraData = JsonConvert.SerializeObject(new
    //         {
    //             PostId = createDTO.PostId,
    //             PostTitle = post.Title,
    //             BookingId = createDTO.BookingId,
    //             BookingTime = createDTO.BookingTime,
    //         }),
    //         HasRead = false,
    //         OriginUserId = createDTO.OriginUserId,
    //         TargetUserId = post.HostId,
    //     };
    //     return await _notyRepo.CreateNotification(notyEntity);
    // }

    // public async Task<bool> CreateApproveMeetingNoty(ApproveMeetingNotificationDTO createDTO)
    // {
    //     PostEntity post = await _postRepo.GetPostById(createDTO.PostId);
    //     if (post == null)
    //         throw new BaseException(HttpCode.BAD_REQUEST, "Invalid meeting on post");

    //     NotificationEntity notyEntity = new NotificationEntity()
    //     {
    //         Code = NotificationCode.BOOKING__HOST_APPROVE_MEETING,
    //         ExtraData = JsonConvert.SerializeObject(new
    //         {
    //             PostId = createDTO.PostId,
    //             PostTitle = post.Title,
    //             BookingId = createDTO.BookingId,
    //         }),
    //         HasRead = false,
    //         OriginUserId = post.HostId,
    //         TargetUserId = createDTO.TargetUserId,
    //     };
    //     return await _notyRepo.CreateNotification(notyEntity);
    // }

    // public async Task<bool> CreateConfirmMetNoty(ConfirmMetNotificationDTO createDTO)
    // {
    //     PostEntity post = await _postRepo.GetPostById(createDTO.PostId);
    //     if (post == null)
    //         throw new BaseException(HttpCode.BAD_REQUEST, "Invalid meeting on post");

    //     NotificationEntity notyEntity = new NotificationEntity()
    //     {
    //         Code = NotificationCode.BOOKING__HOST_CONFIRM_MET,
    //         ExtraData = JsonConvert.SerializeObject(new
    //         {
    //             PostId = createDTO.PostId,
    //             PostTitle = post.Title,
    //             BookingId = createDTO.BookingId,
    //         }),
    //         HasRead = false,
    //         OriginUserId = post.HostId,
    //         TargetUserId = createDTO.TargetUserId,
    //     };
    //     return await _notyRepo.CreateNotification(notyEntity);
    // }
}