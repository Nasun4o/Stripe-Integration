using AutoMapper;
using Entities.EntityModels;
using Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Services
{
    public abstract class BaseService
    {
        protected BaseService()
        {

        }

        protected BaseService(IData repositoryWrapper, IMapper mapper)
        {
            this.Data = repositoryWrapper;
            this.Mapper = mapper;
        }

        protected BaseService(IData repositoryWrapper,
            UserManager<ApplicationUser> userManager)
        {
            this.Data = repositoryWrapper;
            this.UserManager = userManager;
        }

        protected BaseService(IData repositoryWrapper,
             UserManager<ApplicationUser> userManager,
             IMapper mapper)
        {
            this.Data = repositoryWrapper;
            this.UserManager = userManager;
            this.Mapper = mapper;
        }

        protected UserManager<ApplicationUser> UserManager { get; set; }

        protected IData Data { get; set; }

        protected IMapper Mapper { get; set; }
    }
}
