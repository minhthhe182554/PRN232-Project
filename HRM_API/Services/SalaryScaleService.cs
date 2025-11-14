using HRM_API.Dtos;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class SalaryScaleService
    {
        private readonly SalaryScaleRepository _salaryScaleRepository;

        public SalaryScaleService(SalaryScaleRepository salaryScaleRepository)
        {
            _salaryScaleRepository = salaryScaleRepository;
        }

        public async Task<List<SalaryScaleResponse>> GetAllAsync()
        {
            var salaryScales = await _salaryScaleRepository.GetAllAsync();
            
            return salaryScales.Select(s => new SalaryScaleResponse
            {
                Id = s.Id,
                Role = s.Role.ToString(),
                Level = s.Level,
                BaseSalary = s.BaseSalary,
                Description = s.Description ?? string.Empty
            }).ToList();
        }

        public async Task<SalaryScaleResponse?> UpdateAsync(UpdateSalaryScaleRequest request)
        {
            var salaryScale = await _salaryScaleRepository.GetByIdAsync(request.Id);
            
            if (salaryScale == null)
                return null;

            var success = await _salaryScaleRepository.UpdateBaseSalaryAsync(request.Id, request.BaseSalary);

            if (!success)
                return null;

            return new SalaryScaleResponse
            {
                Id = salaryScale.Id,
                Role = salaryScale.Role.ToString(),
                Level = salaryScale.Level,
                BaseSalary = request.BaseSalary,
                Description = salaryScale.Description ?? string.Empty
            };
        }
    }
}

