using System;
using HRM_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories;

public class PolicyRepository
{
    private readonly HRMDbContext _context;

    public PolicyRepository(HRMDbContext context)
    {
        _context = context;
    }

    public async Task<Policy?> GetAll()
    {
        return await _context.Policies.FirstOrDefaultAsync();
    }

    public async Task<Policy?> UpdateAsync(Policy policy)
    {
        var existingPolicy = await _context.Policies.FindAsync(policy.Id);
        
        if (existingPolicy == null)
            return null;

        existingPolicy.WorkStartTime = policy.WorkStartTime;
        existingPolicy.WorkEndTime = policy.WorkEndTime;
        existingPolicy.LateThresholdMinutes = policy.LateThresholdMinutes;
        existingPolicy.LateDeductionPercent = policy.LateDeductionPercent;
        existingPolicy.LeaveEarlyThresholdMinutes = policy.LeaveEarlyThresholdMinutes;
        existingPolicy.LeaveEarlyDeductionPercent = policy.LeaveEarlyDeductionPercent;
        existingPolicy.MonthlyOvertimeHoursLimit = policy.MonthlyOvertimeHoursLimit;
        existingPolicy.AnnualLeaveMaxDays = policy.AnnualLeaveMaxDays;

        await _context.SaveChangesAsync();
        return existingPolicy;
    }
}
