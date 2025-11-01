using System;
using HRM_API.Dtos;
using HRM_API.Models;
using HRM_API.Repositories;

namespace HRM_API.Services;

public class PolicyService
{
    private readonly PolicyRepository _policyRepository;

    public PolicyService(PolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<PolicyResponse?> GetAll()
    {
        var policy = await _policyRepository.GetAll();

        if (policy == null) return null;

        return new PolicyResponse
        {
            Id = policy.Id,
            WorkStartTime = policy.WorkStartTime,
            WorkEndTime = policy.WorkEndTime,
            LateThresholdMinutes = policy.LateThresholdMinutes,
            LateDeductionPercent = policy.LateDeductionPercent,
            LeaveEarlyThresholdMinutes = policy.LeaveEarlyThresholdMinutes,
            LeaveEarlyDeductionPercent = policy.LeaveEarlyDeductionPercent,
            MonthlyOvertimeHoursLimit = policy.MonthlyOvertimeHoursLimit,
            AnnualLeaveMaxDays = policy.AnnualLeaveMaxDays
        };
    }
    
    public async Task<PolicyResponse?> UpdateAsync(UpdatePolicyRequest request)
    {
        var policy = new Policy
        {
            Id = request.Id,
            WorkStartTime = request.WorkStartTime,
            WorkEndTime = request.WorkEndTime,
            LateThresholdMinutes = request.LateThresholdMinutes,
            LateDeductionPercent = request.LateDeductionPercent,
            LeaveEarlyThresholdMinutes = request.LeaveEarlyThresholdMinutes,
            LeaveEarlyDeductionPercent = request.LeaveEarlyDeductionPercent,
            MonthlyOvertimeHoursLimit = request.MonthlyOvertimeHoursLimit,
            AnnualLeaveMaxDays = request.AnnualLeaveMaxDays
        };

        var updatedPolicy = await _policyRepository.UpdateAsync(policy);

        if (updatedPolicy == null)
            return null;

        return new PolicyResponse
        {
            Id = updatedPolicy.Id,
            WorkStartTime = updatedPolicy.WorkStartTime,
            WorkEndTime = updatedPolicy.WorkEndTime,
            LateThresholdMinutes = updatedPolicy.LateThresholdMinutes,
            LateDeductionPercent = updatedPolicy.LateDeductionPercent,
            LeaveEarlyThresholdMinutes = updatedPolicy.LeaveEarlyThresholdMinutes,
            LeaveEarlyDeductionPercent = updatedPolicy.LeaveEarlyDeductionPercent,
            MonthlyOvertimeHoursLimit = updatedPolicy.MonthlyOvertimeHoursLimit,
            AnnualLeaveMaxDays = updatedPolicy.AnnualLeaveMaxDays
        };
    }
}
