using Monolithic.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Monolithic.Models.ReqParams;
using Monolithic.Models.Entities;
using Monolithic.Models.Context;
using Monolithic.Models.Common;
using Monolithic.Extensions;

namespace Monolithic.Repositories.Implement;

public class NotificationRepository : INotificationRepository
{
    private readonly DataContext _db;

    public NotificationRepository(DataContext db)
    {
        _db = db;
    }

    public async Task<PagedList<NotificationEntity>> GetNotifications(int userId, NotificationParams notificationParams)
    {
        var notifications = _db.Notifications
                            .OrderByDescending(n => n.CreatedAt)
                            .Where(n => n.TargetUserId == userId);
        if (notificationParams.Today)
        {
            notifications = notifications.Where(n => n.CreatedAt.Date == DateTime.Now.Date);
        }
        return await notifications.ToPagedList(notificationParams.PageNumber, notificationParams.PageSize);
    }

    public async Task<NotificationEntity> GetById(int id)
    {
        NotificationEntity notification = await _db.Notifications.FindAsync(id);
        if (notification == null) return null;
        _db.Entry(notification).State = EntityState.Detached;
        return notification;
    }

    public async Task<Tuple<int, int>> CountUnreadNotification(int userId)
    {
        var notifications = _db.Notifications.Where(n =>
                                    n.TargetUserId == userId &&
                                    n.HasRead == false);
        int countAll = await notifications.CountAsync();
        notifications = notifications.Where(n => n.CreatedAt.Date == DateTime.Now.Date);
        int countToday = await notifications.CountAsync();
        return new Tuple<int, int>(countAll, countToday);
    }

    public async Task<bool> CreateNotification(NotificationEntity notificationEntity)
    {
        await _db.Notifications.AddAsync(notificationEntity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateNotification(NotificationEntity notificationEntity)
    {
        _db.Notifications.Update(notificationEntity);
        return await _db.SaveChangesAsync() >= 0;
    }

    public async Task<bool> SetAllNotyHasRead(int userId)
    {
        var noties = await _db.Notifications.Where(n =>
                                        n.TargetUserId == userId &&
                                        n.HasRead == false).ToListAsync();
        if (noties.Count > 0)
        {
            noties.ForEach(n => n.HasRead = true);
            return await _db.SaveChangesAsync() > 0;
        }
        return true;
    }
}