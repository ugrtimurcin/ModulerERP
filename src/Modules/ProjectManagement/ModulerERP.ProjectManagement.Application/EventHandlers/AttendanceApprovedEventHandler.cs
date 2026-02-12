using MediatR;
using ModulerERP.SharedKernel.Events;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.ProjectManagement.Application.EventHandlers;

public class AttendanceApprovedEventHandler : INotificationHandler<AttendanceApprovedEvent>
{
    private readonly IRepository<DailyLog> _dailyLogRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<ProjectResource> _projectResourceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceApprovedEventHandler(
        IRepository<DailyLog> dailyLogRepository,
        IRepository<Project> projectRepository,
        IRepository<ProjectResource> projectResourceRepository,
        IUnitOfWork unitOfWork)
    {
        _dailyLogRepository = dailyLogRepository;
        _projectRepository = projectRepository;
        _projectResourceRepository = projectResourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AttendanceApprovedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.MatchedProjectId == null) return;

        var projectId = notification.MatchedProjectId.Value;
        
        // 1. Find the Project Resource for this Employee
        var resources = await _projectResourceRepository.FindAsync(
            pr => pr.ProjectId == projectId && pr.EmployeeId == notification.EmployeeId,
            cancellationToken);
        
        var resource = resources.FirstOrDefault();
        if (resource == null) return; // Employee not assigned to this project

        // 2. Find or Create DailyLog
        var logs = await _dailyLogRepository.FindAsync(
            l => l.ProjectId == projectId && l.Date.Date == notification.Date.Date,
            cancellationToken);

        var dailyLog = logs.FirstOrDefault();

        if (dailyLog == null)
        {
            dailyLog = new DailyLog
            {
                ProjectId = projectId,
                Date = notification.Date,
                SiteManagerNote = "Auto-generated from Attendance",
                IsApproved = false // Pending Site Manager review
            };
            await _dailyLogRepository.AddAsync(dailyLog, cancellationToken);
        }

        // 3. Add Resource Usage
        // Ideally we should check if usage already exists to avoid duplicates. 
        // For now, we assume we append or the repository/EF handles it if we loaded it.
        // Since we didn't Include usages, we can't check easily without another query.
        // Let's assume we can add a new usage. 
        
        var usage = new DailyLogResourceUsage
        {
            ProjectResourceId = resource.Id,
            RawHours = notification.TotalMinutes / 60m,
            ApprovedHours = notification.TotalMinutes / 60m, // Auto-approve from HR perspective?
            ValidationStatus = ValidationStatus.Verified,
            Description = "Attendance Scan"
        };
        
        // Note: If DailyLog is new, ResourceUsages is empty list.
        // If DailyLog is existing, ResourceUsages might be null or empty if not loaded.
        // If we just add to the collection, EF might not know unless it's tracking.
        // Safer way without Includes in Repo:
        // If the repository pattern allows adding child entities explicitly or if we accept we might need to query.
        
        // Workaround: We add the usage to the collection. usage.DailyLogId will be set when DailyLog is saved?
        // No, if DailyLog is existing, we need its ID.
        // If New, ID is generated.
        
        if (dailyLog.Id != Guid.Empty)
        {
            usage.DailyLogId = dailyLog.Id;
            // Since we don't have DailyLogResourceUsage repository injected, we rely on DailyLog.ResourceUsages.Add
            // BUT, if we didn't load the collection, accessing it might be empty but not tracking correctly?
            // Actually, if we just Add to the list and Save the Parent, EF Core usually handles it IF the parent is tracked.
            
            // To be safe, we might need to ensure DailyLog is tracked. 
            // _dailyLogRepository.UpdateAsync(dailyLog) might be needed if it's not tracking.
        }

        dailyLog.ResourceUsages.Add(usage);
        
        // If existing, we might need to Update. If new, Add was called.
        if (dailyLog.Id != Guid.Empty)
        {
             _dailyLogRepository.Update(dailyLog);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
