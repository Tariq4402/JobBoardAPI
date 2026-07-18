using AutoMapper;
using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
namespace JobBoardAPI.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Job, JobResponseDTO>()
				.ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Employer != null ? src.Employer.CompanyName : null));

			CreateMap<JobCreateDTO, Job>();

			CreateMap<JobApplicationCreateDTO, JobApplication>();

			CreateMap<JobApplication, JobApplicationResponseDTO>()
				.ForMember(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.Applicant != null ? src.Applicant.Name : null))
	            .ForMember(dest => dest.ApplicantEmail, opt => opt.MapFrom(src => src.Applicant != null ? src.Applicant.Email : null))
	            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job != null ? src.Job.JobTitle : null));

			CreateMap<User, UserResponseDTO>();

			CreateMap<User, AdminUserResponseDTO>();

			//CreateMap<User, EmailUpdateDTO>();
		}

	}
}
