using AutoMapper;
using HRM_API.Dtos.Dashboard.Policy;
using HumanResourceManagement.Models;

namespace HRM_API.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Policy, PolicyDto>();
            CreateMap<UpdatePolicyDto, PolicyDto>();
        }
    }
}
