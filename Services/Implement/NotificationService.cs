using Monolithic.Repositories.Interface;
using Monolithic.Services.Interface;
using Monolithic.Models.ReqParams;
using Monolithic.Models.Entities;
using Monolithic.Models.Common;
using Monolithic.Models.DTO;
using Monolithic.Constants;
using Monolithic.Helpers;
using Newtonsoft.Json;
using AutoMapper;

namespace Monolithic.Services.Implement;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notyRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpHelper<List<UserNotificationDTO>> _userHttpHelper;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    public NotificationService(INotificationRepository notyRepo,
                               IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               IMapper mapper)
    {
        _notyRepo = notyRepo;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mapper = mapper;
        _userHttpHelper = new HttpHelper<List<UserNotificationDTO>>(_httpClientFactory);
    }

    public async Task<PagedList<NotificationDTO>> GetNotifications(ReqUser reqUser,
                                                NotificationParams notificationParams)
    {
        PagedList<NotificationEntity> notyEntityList = await _notyRepo.GetNotifications(reqUser.Id, notificationParams);
        var originUserIds = string.Join(",", notyEntityList.Records.Select(n => n.OriginUserId).Distinct());
        var originUsers = await GetUsersNotification(originUserIds, reqUser.Token);

        var notyDTOList = notyEntityList.Records
                                        .Select(b => mappingNotification(b, originUsers))
                                        .ToList();
        return new PagedList<NotificationDTO>(notyDTOList, notyEntityList.TotalRecords);
    }

    private NotificationDTO mappingNotification(NotificationEntity noti, List<UserNotificationDTO> originUsers)
    {
        var notiDTO = _mapper.Map<NotificationDTO>(noti);
        var userDTO = originUsers.FirstOrDefault(u => u.OriginUserId == noti.OriginUserId);
        notiDTO.OriginUserName = userDTO.OriginUserName;
        notiDTO.OriginUserEmail = userDTO.OriginUserEmail;
        notiDTO.OriginUserAvatar = userDTO.OriginUserAvatar;
        return notiDTO;
    }

    private async Task<List<UserNotificationDTO>> GetUsersNotification(string originUserIds, string token)
    {
        var monolithicService = _configuration["MonolithicService"];
        string userUrl = $"{monolithicService}/api/user/notification/{originUserIds}";
        return await _userHttpHelper.GetAsync(userUrl, token);
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

    public async Task<bool> CreateReviewOnPostNoty(ReviewNotificationDTO createDTO)
    {
        NotificationEntity notyEntity = new NotificationEntity()
        {
            Code = NotificationCode.REVIEW__HAS_REVIEW_ON_POST,
            ExtraData = JsonConvert.SerializeObject(new
            {
                PostId = createDTO.PostId,
                PostTitle = createDTO.PostTitle,
                ReviewId = createDTO.ReviewId,
                ReviewContent = createDTO.ReviewContent,
                ReviewRating = createDTO.ReviewRating,
            }),
            HasRead = false,
            OriginUserId = createDTO.OriginUserId,
            TargetUserId = createDTO.HostId,
        };
        return await _notyRepo.CreateNotification(notyEntity);
    }

    public async Task<bool> CreateBookingOnPostNoty(BookingNotificationDTO createDTO)
    {
        NotificationEntity notyEntity = new NotificationEntity()
        {
            Code = NotificationCode.BOOKING__HAS_BOOKING_ON_POST,
            ExtraData = JsonConvert.SerializeObject(new
            {
                PostId = createDTO.PostId,
                PostTitle = createDTO.PostTitle,
                BookingId = createDTO.BookingId,
                BookingTime = createDTO.BookingTime,
            }),
            HasRead = false,
            OriginUserId = createDTO.OriginUserId,
            TargetUserId = createDTO.HostId,
        };
        return await _notyRepo.CreateNotification(notyEntity);
    }

    public async Task<bool> CreateApproveMeetingNoty(ApproveMeetingNotificationDTO createDTO)
    {
        NotificationEntity notyEntity = new NotificationEntity()
        {
            Code = NotificationCode.BOOKING__HOST_APPROVE_MEETING,
            ExtraData = JsonConvert.SerializeObject(new
            {
                PostId = createDTO.PostId,
                PostTitle = createDTO.PostTitle,
                BookingId = createDTO.BookingId,
            }),
            HasRead = false,
            OriginUserId = createDTO.HostId,
            TargetUserId = createDTO.TargetUserId,
        };
        return await _notyRepo.CreateNotification(notyEntity);
    }

    public async Task<bool> CreateConfirmMetNoty(ConfirmMetNotificationDTO createDTO)
    {
        NotificationEntity notyEntity = new NotificationEntity()
        {
            Code = NotificationCode.BOOKING__HOST_CONFIRM_MET,
            ExtraData = JsonConvert.SerializeObject(new
            {
                PostId = createDTO.PostId,
                PostTitle = createDTO.PostTitle,
                BookingId = createDTO.BookingId,
            }),
            HasRead = false,
            OriginUserId = createDTO.HostId,
            TargetUserId = createDTO.TargetUserId,
        };
        return await _notyRepo.CreateNotification(notyEntity);
    }
}