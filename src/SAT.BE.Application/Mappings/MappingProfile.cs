using AutoMapper;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Employee;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Employee;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Department;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Department;

namespace SAT.BE.src.SAT.BE.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Employee Mappings
            CreateMap<Employee, EmployeeResponseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
                .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src => src.WorkPosition.PositionName));

            CreateMap<CreateEmployeeRequestDto, Employee>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateEmployeeRequestDto, Employee>()
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Department Mappings
            CreateMap<Department, DepartmentResponseDto>()
                .ForMember(dest => dest.LeaderName, opt => opt.MapFrom(src => src.Leader != null ? src.Leader.FullName : null))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count));

            CreateMap<CreateDepartmentRequestDto, Department>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateDepartmentRequestDto, Department>()
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}